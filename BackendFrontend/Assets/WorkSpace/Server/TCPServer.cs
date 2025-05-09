using System;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using UniRx;
using MongoDB.Bson;

public class TCPServer : MonoBehaviour
{
    [SerializeField] int port;
    [SerializeField] int playerCount;
    TcpListener tcpListener;

    // Thread Safe
    public ConcurrentDictionary<TcpClient, ClientInfo> Clients { get; set; } = new ConcurrentDictionary<TcpClient, ClientInfo>();
    ConcurrentQueue<TcpClient> matchQueue = new ConcurrentQueue<TcpClient>();
    CancellationToken token;

    private void Awake()
    {
        string localIP = Global.GetLocalIPAddress();
        string publicIP = Global.GetPublicIPAddress();
        Debug.Log($"[TCPServer] Running on IP: {publicIP}, Port: {port}");
    }
    private void Start()
    {
        // 오브젝트 파괴 시, Cancel
        token = this.GetCancellationTokenOnDestroy();

        // 서버 시작
        tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();

        // 클라이언트 연결
        _ = UniTask.RunOnThreadPool(() => AcceptClientAsync());

        // 매칭
        _ = UniTask.RunOnThreadPool(() => MatchMakingAsync());
    }
    private async UniTask AcceptClientAsync()
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                // 연결 대기
                TcpClient _client = await tcpListener.AcceptTcpClientAsync();
                Debug.Log($"[TCPServer] Accepted client");

                // 클라이언트 관리
                ClientInfo _clientInfo = new ClientInfo();
                _clientInfo.Address = null;
                _clientInfo.Port = 0;
                Clients.TryAdd(_client, _clientInfo);

                // 메세지 수신
                _ = UniTask.RunOnThreadPool(() => ReceiveMessageAsync(_client));
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[TCPServer] Error accepting client: {e.Message}");
        }
    }
    private async UniTask SendMessageAsync(TcpClient client, PacketID packetID, Dictionary<string, string> body)
    {
        // Body 직렬화
        string json = JsonConvert.SerializeObject(body);
        byte[] bodyBytes = Encoding.UTF8.GetBytes(json);

        // Header 생성
        int totalSize = PacketHeader.Size + bodyBytes.Length;
        byte[] packet = new byte[totalSize];

        // Header 삽입
        Array.Copy(BitConverter.GetBytes(totalSize), 0, packet, 0, 4);          // Packet Size
        Array.Copy(BitConverter.GetBytes((ushort)packetID), 0, packet, 4, 2);   // Packet ID

        // Body 삽입
        Array.Copy(bodyBytes, 0, packet, PacketHeader.Size, bodyBytes.Length);

        // 전송
        try
        {
            await client.GetStream().WriteAsync(packet, 0, packet.Length);
            Debug.Log($"[TCPServer] Sent message: {packetID}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[TCPServer] Error sending message: {e.Message}");
        }
    }
    private async UniTask ReceiveMessageAsync(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] headerBuffer = new byte[PacketHeader.Size];

        try
        {
            while (client.Connected && !token.IsCancellationRequested)
            {
                // Header 수신
                int headerRead = await stream.ReadAsync(headerBuffer, 0, PacketHeader.Size);
                if (headerRead < PacketHeader.Size) break;

                int packetSize = BitConverter.ToInt32(headerBuffer, 0);
                ushort packetID = BitConverter.ToUInt16(headerBuffer, 4);

                int bodySize = packetSize - PacketHeader.Size;

                // Body 수신
                byte[] boddyBuffer = new byte[bodySize];
                    // Body Byte데이터를 BodySize만큼 다 읽기 위한 로직
                int totalRead = 0;
                while(totalRead < bodySize)
                {
                    int read = await stream.ReadAsync(boddyBuffer, totalRead, bodySize - totalRead);
                    if (read == 0)
                    {
                        Debug.LogWarning("[TCPServer] Client Disconnected");
                        break;
                    }
                    totalRead += read;
                }

                string json = Encoding.UTF8.GetString(boddyBuffer);
                var body = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                HandlePacket(client, packetID, body);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[TCPServer] Error receiving message: {e.Message}");
        }
        finally

        {
            DisconnectClientAsync(client);
        }
    }
    // 라우터
    private void HandlePacket(TcpClient client, ushort packetID, Dictionary<string, string> body)
    {
        PacketID _packetID = (PacketID)packetID;

        switch (_packetID)
        {
            case PacketID.Ping:
                HandlePing(client, body);
                break;
            case PacketID.SignIn:
                HandleSignIn(client, body);
                break;
            case PacketID.SignUp:
                HandleSignUp(client, body);
                break;
            case PacketID.SetNickname:
                HandleSetNickname(client, body);
                break;
            case PacketID.Matching:
                HandleMatch(client, body);
                break;
            default:
                Debug.LogWarning($"[서버] 알 수 없는 PacketID: {packetID}");
                break;
        }
    }
    private void SendPong(TcpClient client, Dictionary<string, string> body)
    {
        Dictionary<string, string> pong = new Dictionary<string, string>();

        if (body.TryGetValue("Timestamp", out string ts))
            pong["Timestamp"] = ts;

        _ = SendMessageAsync(client, PacketID.Pong, pong);
    }
    private void HandlePing(TcpClient client, Dictionary<string, string> body)
    {
        Debug.Log("[TCPServer] Received Ping");

        SendPong(client, body);
    }
    // 클래스 분리 필요
    private async void HandleSignIn(TcpClient client, Dictionary<string, string> body)
    {
        Debug.Log("[TCPServer] Received SignIn request");

        Dictionary<string, string> sendMessage = new Dictionary<string, string> { { "Result", "" } };

        if (body.TryGetValue("ID", out string id) && body.TryGetValue("Password", out string pw))
        {
            // 리팩토링 필요
            BsonDocument _bsonDocument = await MongoDBCommunity.Instance.GetBsonDocumentAsync("ID", id);
            if (_bsonDocument != null)
            {
                if (_bsonDocument["Password"] == pw)
                {
                    Debug.Log("[TCPServer] SignIn Succeed");

                    // UserData 가져오기
                    UserData _userData = new UserData();

                    if (_bsonDocument.TryGetValue("_id", out BsonValue guid))
                        _userData.GUID = guid.ToString();

                    if (_bsonDocument.TryGetValue("Nickname", out BsonValue value))
                        _userData.Nickname = value.ToString();
                    else
                        _userData.Nickname = "Unknown";

                    // 성공 메세지
                    sendMessage["Result"] = "Succeed";
                    sendMessage.Add("UserData", JsonConvert.SerializeObject(_userData));

                    _ = SendMessageAsync(client, PacketID.SignIn, sendMessage);
                    return;
                }
            }
        }

        // 실패 메세지
        Debug.Log("[TCPServer] SignIn Failed");
        sendMessage["Result"] = "Failed";
        _ = SendMessageAsync(client, PacketID.SignIn, sendMessage);
    }
    private async void HandleSignUp(TcpClient client, Dictionary<string, string> body)
    {
        Debug.Log("[TCPServer] Received SignUp request");

        Dictionary<string, string> sendMessage = new Dictionary<string, string>() { { "Result", "" } };

        if (body.TryGetValue("ID", out string id) && body.TryGetValue("Password", out string pw))
        {
            if (await MongoDBCommunity.Instance.TryInsertAccountDataAsync(id, pw))
            {
                Debug.Log("[TCPServer] SignUp Succeed");

                sendMessage["Result"] = "Succeed";
                _ = SendMessageAsync(client, PacketID.SignUp, sendMessage);
                return;
            }
        }

        // 실패 메세지
        Debug.Log("[TCPServer] SignUp Failed");
        sendMessage["Result"] = "Failed";
        _ = SendMessageAsync(client, PacketID.SignUp, sendMessage);
    }
    private async void HandleSetNickname(TcpClient client, Dictionary<string, string> body)
    {
        Debug.Log("[TCPServer] Received SetNickname request");

        Dictionary<string, string> sendMessage = new Dictionary<string, string>() { { "Result", "" } };

        // 성공 시,
        if (body.TryGetValue("Nickname", out string nickname))
        {
            if(await MongoDBCommunity.Instance.IsUnique("Nickname", nickname))
            {
                if (body.TryGetValue("GUID", out string guid))
                {
                    await MongoDBCommunity.Instance.UpdateDataAsync(guid, "Nickname", nickname);

                    Debug.Log("[TCPServer] SetNickname Succeed");

                    sendMessage["Result"] = "Succeed";
                    sendMessage.Add("Nickname", nickname);

                    _ = SendMessageAsync(client, PacketID.SetNickname, sendMessage);

                    return;
                }
            }
        }

        // 실패 시,
        Debug.Log("[TCPServer] SetNickname Failed");
        sendMessage["Result"] = "Failed";
        _ = SendMessageAsync(client, PacketID.SetNickname, sendMessage);
    }
    private void HandleMatch(TcpClient client, Dictionary<string, string> body)
    {
        Debug.Log("[TCPServer] Received Match request");

        Dictionary<string, string> sendMessage = new Dictionary<string, string>() { { "Result", "" } };

        if (body.TryGetValue("IPEndPoint", out string ipEndPoint))
        {
            if (Clients.TryGetValue(client, out ClientInfo info))
            {
                string[] _parts = ipEndPoint.Split(':');
                string _address = _parts[0];
                int _port = int.Parse(_parts[1]);

                // UDP IPEndPoint 저장
                info.Address = _address;
                info.Port = _port;
            }
            else
            {
                Debug.LogError("[TCPServer] The client is not registered");
            }
        }
        else if (body.TryGetValue("Result", out string result))
        {
            if (result.Equals("Requested"))
            {
                matchQueue.Enqueue(client);
                Debug.Log("[TCPServer] Client match requested");
                Debug.Log($"[TCPServer] Current matching client count: {matchQueue.Count}");

                sendMessage["Result"] = "Requested";
                // 매칭 요청
            }
            else if (result.Equals("Canceled"))
            {
                DequeueInMatchQueue(client);
                Debug.Log("[TCPServer] Client match request canceled");

                sendMessage["Result"] = "Canceled";
                // 매칭 요청 취소
            }
            else
            {
                Debug.LogError("[TCPServer] Request message is not identify");

                sendMessage["Result"] = "Canceled";
            }

            _ = SendMessageAsync(client, PacketID.Matching, sendMessage);
        }
    }
    private void DisconnectClientAsync(TcpClient client)
    {
        try
        {
            // 매칭 큐에서 제거
            DequeueInMatchQueue(client);

            // 클라이언트 제거
            Clients.TryRemove(client, out _);

            // 클라이언트 연결 종료
            if (client.Connected)
                client.Close();

            Debug.Log("[TCPServer] Disconnected client");
        }
        catch (Exception e)
        {
            Debug.LogError($"[TCPServer] Error while disconnecting client: {e.Message}");
        }
    }
    private void DequeueInMatchQueue(TcpClient client)
    {
        int _queueSize = matchQueue.Count;
        for (int i = 0; i < _queueSize; i++)
        {
            if (matchQueue.TryDequeue(out TcpClient dequeuedClient))
                if (dequeuedClient != client)
                    matchQueue.Enqueue(dequeuedClient); // 제거할 클라이언트가 아니면 다시 큐에 추가
        }
        Debug.Log($"[TCPServer] Current matching client count: {matchQueue.Count}");
    }
    private async UniTask MatchMakingAsync()
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await UniTask.Delay(3000); // 매칭 주기 (3초마다 체크)

                if (matchQueue.Count >= playerCount) // 최소 인원이 있어야 매칭
                {
                    // 매칭 메세지
                    Dictionary<string, string> _matchMessage = new Dictionary<string, string>
                    {
                        { "Result", "Found" }
                    };

                    // 매칭 그룹
                    ConcurrentQueue<TcpClient> _matchingGroup = new ConcurrentQueue<TcpClient>();

                    // 메세지 전송
                    for (int i = 0; i < playerCount; i++)
                    {
                        if (matchQueue.TryDequeue(out TcpClient client))
                        {
                            _matchingGroup.Enqueue(client);
                            _ = SendMessageAsync(client, PacketID.Matching, _matchMessage);
                        }
                        else
                        {
                            Debug.Log("[TCPServer] Match making failed");
                        }
                    }

                    // 준비 전 확인 작업 및 메세지 전송 (UDP Client IPEndPoint)
                    _ = CheckMatchedClientsReadyAsync(_matchingGroup);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[TCPServer] Error match making: {e.Message}");
            }
        }
    }
    private async UniTask CheckMatchedClientsReadyAsync(ConcurrentQueue<TcpClient> matchedClients)
    {
        int _matchedPlayerCount = matchedClients.Count; // 매칭 된 클라이언트 수
        int _checkCount = 10; // 10번 확인
        int _checkedClientCount = 0;  // 확인 된 클라이언트
        int _currentCheckCount = 0; // 현재 확인 사이클

        // 복사
        ConcurrentQueue<TcpClient> _temp = new ConcurrentQueue<TcpClient>(matchedClients);

        while (!token.IsCancellationRequested)
        {
            await UniTask.Delay(1000); // 1초마다 체크
            _currentCheckCount++;
            _checkedClientCount = 0;

            try
            {
                for (int i = 0; i < _matchedPlayerCount; i++)
                {
                    if (matchedClients.TryDequeue(out TcpClient client))
                    {
                        if(Clients.TryGetValue(client, out ClientInfo info))
                        {
                            Debug.Log($"[Debug] {info.Address}, {info.Port}");
                            if (info.Address != null && info.Port != 0)
                                _checkedClientCount++;
                        }
                        else
                        {
                            Debug.LogError("[TCPServer] Error getting matched clients value");
                        }
                    }
                    else
                    {
                        Debug.LogError("[TCPServer] Error taking matched clients");
                    }
                }
                // 다시 할당
                matchedClients = new ConcurrentQueue<TcpClient>(_temp);

                
                // 모두 확인 시,
                if (_checkedClientCount == _matchedPlayerCount)
                {
                    Dictionary<string, string> _matchMessage = new Dictionary<string, string>
                    {
                        { "Result", "Succeed" },
                        { "ClientCount", $"{_checkedClientCount.ToString()}" }
                    };

                    // Client들의 IPEndPoint 메세지에 담기
                    for (int i = 0; i < _matchedPlayerCount; i++)
                    {
                        if (matchedClients.TryDequeue(out TcpClient client))
                        {
                            if (Clients.TryGetValue(client, out ClientInfo info))
                            {
                                _matchMessage.Add($"Client{i}", info.Address + ":" + info.Port.ToString());
                            }
                            else
                            {
                                Debug.LogError("[TCPServer] Error getting matched clients value");
                            }
                        }
                        else
                        {
                            Debug.LogError("[TCPServer] Error taking matched clients");
                        }
                    }

                    // 메세지 전송
                    for (int i = 0; i < _matchedPlayerCount; i++)
                    {
                        if (_temp.TryDequeue(out TcpClient client))
                        {
                            _ = SendMessageAsync(client, PacketID.Matching, _matchMessage);
                        }
                        else
                        {
                            Debug.LogError("[TCPServer] Error getting matched clients value");
                        }
                    }
                    break;
                }

                // 확인이 오래걸릴 시,
                if (_currentCheckCount > _checkCount)
                {
                    Dictionary<string, string> _matchMessage = new Dictionary<string, string>
                    {
                        { "Type", "Match" },
                        { "Result", "Failed" }
                    };

                    for (int i = 0; i < _matchedPlayerCount; i++)
                    {
                        if (matchedClients.TryDequeue(out TcpClient client))
                        {
                            // 실패 메세지 전달
                            _ = SendMessageAsync(client, PacketID.Matching, _matchMessage);
                        }
                        else
                        {
                            Debug.LogError("[TCPServer] Error taking matched clients");
                        }
                    }
                    break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[TCPServer] Error checking matched clients ready: {e.Message}");
            }
        }
    }
    private void OnApplicationQuit()
    {
        tcpListener?.Stop();
        tcpListener = null;
    }
}

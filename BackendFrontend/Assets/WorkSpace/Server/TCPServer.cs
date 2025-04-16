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
    #region Basic TCP Server
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

                // 메세지 전송
                _ = SendMessageAsync(_client, new Dictionary<string, string>() { { "Type", "Connect" } });
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[TCPServer] Error accepting client: {e.Message}");
        }
    }
    private async UniTask ReceiveMessageAsync(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        try
        {
            while (client.Connected && !token.IsCancellationRequested)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;  // 클라이언트 연결 종료 감지

                // Byte[] to string
                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                // string to Dictionary<string, string>
                var messageData = JsonConvert.DeserializeObject<Dictionary<string, string>>(receivedMessage);
                if (messageData == null) return;

                Debug.Log($"[TCPServer] Received message: {receivedMessage}");

                // 메세지 처리 함수
                ReceivedMessageProcess(client, messageData);
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
    private async UniTask SendMessageAsync(TcpClient client, Dictionary<string, string> message)
    {
        string json = JsonConvert.SerializeObject(message);
        byte[] data = Encoding.UTF8.GetBytes(json);

        try
        {
            await client.GetStream().WriteAsync(data, 0, data.Length);
            Debug.Log($"[TCPServer] Sent message: {json}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[TCPServer] Error sending message: {e.Message}");
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
    #endregion
    async void ReceivedMessageProcess(TcpClient client, Dictionary<string, string> message)
    {
        if (message.TryGetValue("Type", out string type))
        {
            switch (type)
            {
                case "Connect":

                    break;
                case "SignIn":
                    Debug.Log("[TCPServer] Client SignIn requested");
                    if (message.TryGetValue("ID", out string signInID))
                    {
                        // DB에서 ID값이 일치하는 데이터 Get
                        BsonDocument _bsonDocument = await MongoDBCommunity.Instance.GetBsonDocumentAsync("ID", signInID);

                        if(_bsonDocument != null)
                        {
                            if (message.TryGetValue("Password", out string signInPassword) && _bsonDocument["Password"] == signInPassword)
                            {
                                Debug.Log("[TCPServer] Client SignIn Succeed");

                                // UserData 가져오기
                                UserData _userData = new UserData();
                                _userData.GUID = _bsonDocument["_id"].ToString();
                                if(_bsonDocument.TryGetValue("Nickname", out BsonValue value))
                                    _userData.Nickname = value.ToString();
                                else
                                    _userData.Nickname = "Unknown";

                                // 성공 메세지
                                Dictionary<string, string> _signIn_SucceedMessage = new Dictionary<string, string>
                                {
                                    { "Type", "SignIn" },
                                    { "Result", "Succeed" },
                                    { "UserData", JsonConvert.SerializeObject(_userData)}
                                };
                                _ = SendMessageAsync(client, _signIn_SucceedMessage);
                                return;
                            }
                        }
                    }

                    // 실패 메세지
                    Debug.Log("[TCPServer] Client SignIn Failed");

                    Dictionary<string, string> _signIn_FailedMessage = new Dictionary<string, string>
                                {
                                    { "Type", "SignIn" },
                                    { "Result", "Failed" }
                                };
                    _ = SendMessageAsync(client, _signIn_FailedMessage);
                    break;
                case "SignUp":
                    Debug.Log("[TCPServer] Client SignUp requested");
                    if (message.TryGetValue("ID", out string signUpID) && message.TryGetValue("Password", out string signUpPassword))
                    {
                        if (await MongoDBCommunity.Instance.TryInsertAccountDataAsync(signUpID, signUpPassword))
                        {
                            Debug.Log("[TCPServer] Client SignUp Succeed");

                            Dictionary<string, string> _signUp_SucceedMessage = new Dictionary<string, string>
                            {
                                { "Type", "SignUp" },
                                { "Result", "Succeed" }
                            };

                            _ = SendMessageAsync(client, _signUp_SucceedMessage);
                            return;
                        }
                    }
                    // 실패 메세지

                    Debug.LogError("[TCPServer] ID already exists.");

                    Dictionary<string, string> _signUp_FailedMessage = new Dictionary<string, string>
                                {
                                    { "Type", "SignUp" },
                                    { "Result", "Failed" }
                                };

                    _ = SendMessageAsync(client, _signUp_FailedMessage);

                    break;
                case "SetNickname":
                    // 성공 시,
                    if (message.TryGetValue("Nickname", out string nickname) && await MongoDBCommunity.Instance.IsUnique("Nickname", nickname))
                    {
                        if(message.TryGetValue("GUID", out string guid))
                        {
                            await MongoDBCommunity.Instance.UpdateDataAsync(guid, "Nickname", nickname);

                            Dictionary<string, string> _setNickname_SucceedMessage = new Dictionary<string, string>
                                {
                                    { "Type", "SetNickname" },
                                    { "Result", "Succeed" },
                                    { "Nickname", nickname }
                                };

                            _ = SendMessageAsync(client, _setNickname_SucceedMessage);
                            return;
                        }
                    }
                    // 실패 시,
                    Dictionary<string, string> _setNickname_FailedMessage = new Dictionary<string, string>
                    {
                        { "Type", "SetNickname" },
                        { "Result", "Failed" }
                    };

                    _ = SendMessageAsync(client, _setNickname_FailedMessage);
                    break;
                case "Match":
                    if (message.TryGetValue("IPEndPoint", out string ipEndPoint))
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
                    else if (message.TryGetValue("Request", out string value))
                    {
                        if(value == "True")
                        {
                            matchQueue.Enqueue(client);
                            Debug.Log("[TCPServer] Client match requested");
                            Debug.Log($"[TCPServer] Current matching client count: {matchQueue.Count}");
                            // 매칭 요청
                        }
                        else if(value == "False")
                        {
                            DequeueInMatchQueue(client);
                            Debug.Log("[TCPServer] Client match request canceled");
                            // 매칭 요청 취소
                        }
                        else
                        {
                            Debug.LogError("[TCPServer] Request message is not True or False");
                        }
                    }
                    break;
                default:
                    Debug.LogError("[TCPServer] Unknown message type received.");
                    break;
            }
        }
    }
    void DequeueInMatchQueue(TcpClient client)
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
                        { "Type", "Match" },
                        { "Request", "Found" }
                    };

                    // 매칭 그룹
                    ConcurrentQueue<TcpClient> _matchingGroup = new ConcurrentQueue<TcpClient>();

                    // 메세지 전송
                    for (int i = 0; i < playerCount; i++)
                    {
                        if (matchQueue.TryDequeue(out TcpClient client))
                        {
                            _matchingGroup.Enqueue(client);
                            _ = SendMessageAsync(client, _matchMessage);
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
                        { "Type", "Match" },
                        { "Request", "Succeed" },
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
                            _ = SendMessageAsync(client, _matchMessage);
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
                        { "Request", "Failed" }
                    };

                    for (int i = 0; i < _matchedPlayerCount; i++)
                    {
                        if (matchedClients.TryDequeue(out TcpClient client))
                        {
                            // 실패 메세지 전달
                            _ = SendMessageAsync(client, _matchMessage);
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

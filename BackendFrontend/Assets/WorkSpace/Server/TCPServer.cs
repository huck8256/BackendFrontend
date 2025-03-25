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
        // ������Ʈ �ı� ��, Cancel
        token = this.GetCancellationTokenOnDestroy();

        // ���� ����
        tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();

        // Ŭ���̾�Ʈ ����
        _ = UniTask.RunOnThreadPool(() => AcceptClientAsync());

        // ��Ī
        _ = UniTask.RunOnThreadPool(() => MatchMakingAsync());
    }

    private async UniTask AcceptClientAsync()
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                // ���� ���
                TcpClient _client = await tcpListener.AcceptTcpClientAsync();
                Debug.Log($"[TCPServer] Accepted client");

                // Ŭ���̾�Ʈ ����
                ClientInfo _clientInfo = new ClientInfo();
                _clientInfo.Address = null;
                _clientInfo.Port = 0;
                Clients.TryAdd(_client, _clientInfo);

                // �޼��� ����
                _ = UniTask.RunOnThreadPool(() => ReceiveMessageAsync(_client));

                // �޼��� ����
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
                if (bytesRead == 0) break;  // Ŭ���̾�Ʈ ���� ���� ����

                // Byte[] to string
                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                // string to Dictionary<string, string>
                var messageData = JsonConvert.DeserializeObject<Dictionary<string, string>>(receivedMessage);
                if (messageData == null) return;

                Debug.Log($"[TCPServer] Received message: {receivedMessage}");

                // �޼��� ó�� �Լ�
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
            // ��Ī ť���� ����
            DequeueInMatchQueue(client);

            // Ŭ���̾�Ʈ ����
            Clients.TryRemove(client, out _);

            // Ŭ���̾�Ʈ ���� ����
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
    void ReceivedMessageProcess(TcpClient client, Dictionary<string, string> message)
    {
        if (message.TryGetValue("Type", out string type))
        {
            switch (type)
            {
                case "Connect":

                    break;
                case "Match":
                    if (message.TryGetValue("IPEndPoint", out string ipEndPoint))
                    {
                        if (Clients.TryGetValue(client, out ClientInfo info))
                        {
                            string[] _parts = ipEndPoint.Split(':');
                            string _address = _parts[0];
                            int _port = int.Parse(_parts[1]);

                            // UDP IPEndPoint ����
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
                            // ��Ī ��û
                        }
                        else if(value == "False")
                        {
                            DequeueInMatchQueue(client);
                            Debug.Log("[TCPServer] Client match request canceled");
                            // ��Ī ��û ���
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
                    matchQueue.Enqueue(dequeuedClient); // ������ Ŭ���̾�Ʈ�� �ƴϸ� �ٽ� ť�� �߰�
        }
        Debug.Log($"[TCPServer] Current matching client count: {matchQueue.Count}");
    }
    private async UniTask MatchMakingAsync()
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await UniTask.Delay(3000); // ��Ī �ֱ� (3�ʸ��� üũ)

                if (matchQueue.Count >= playerCount) // �ּ� �ο��� �־�� ��Ī
                {
                    // ��Ī �޼���
                    Dictionary<string, string> _matchMessage = new Dictionary<string, string>
                    {
                        { "Type", "Match" },
                        { "Request", "Found" }
                    };

                    // ��Ī �׷�
                    ConcurrentQueue<TcpClient> _matchingGroup = new ConcurrentQueue<TcpClient>();

                    // �޼��� ����
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

                    // �غ� �� Ȯ�� �۾� �� �޼��� ���� (UDP Client IPEndPoint)
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
        int _matchedPlayerCount = matchedClients.Count; // ��Ī �� Ŭ���̾�Ʈ ��
        int _checkCount = 10; // 10�� Ȯ��
        int _checkedClientCount = 0;  // Ȯ�� �� Ŭ���̾�Ʈ
        int _currentCheckCount = 0; // ���� Ȯ�� ����Ŭ

        // ����
        ConcurrentQueue<TcpClient> _temp = new ConcurrentQueue<TcpClient>(matchedClients);

        while (!token.IsCancellationRequested)
        {
            await UniTask.Delay(1000); // 1�ʸ��� üũ
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
                // �ٽ� �Ҵ�
                matchedClients = new ConcurrentQueue<TcpClient>(_temp);

                
                // ��� Ȯ�� ��,
                if (_checkedClientCount == _matchedPlayerCount)
                {
                    Dictionary<string, string> _matchMessage = new Dictionary<string, string>
                    {
                        { "Type", "Match" },
                        { "Request", "Succeed" },
                        { "ClientCount", $"{_checkedClientCount.ToString()}" }
                    };

                    // Client���� IPEndPoint �޼����� ���
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

                    // �޼��� ����
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

                // Ȯ���� �����ɸ� ��,
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
                            // ���� �޼��� ����
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

using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using System.Text;
using UnityEngine.Events;

public class TCPClient : SingletonMonoBehaviour<TCPClient>
{
    [SerializeField] string serverIP;
    [SerializeField] int serverPort;

    TcpClient client;
    NetworkStream stream;
    CancellationToken token;

    public UnityEvent OnServerConnectedEvent { get; private set; } = new UnityEvent();
    public UnityEvent OnServerDisconnectedEvent { get; private set; } = new UnityEvent();
    public UnityEvent OnMatchRequestedEvent { get; private set; } = new UnityEvent();
    public UnityEvent OnMatchRequestCanceledEvent { get; private set; } = new UnityEvent();
    public UnityEvent OnMatchFoundEvent { get; private set; } = new UnityEvent();
    public UnityEvent<List<string>> OnMatchSucceedEvent { get; private set; } = new UnityEvent<List<string>>();
    public UnityEvent OnMatchFailedEvent { get; private set; } = new UnityEvent();

    #region Basic TCP Client
    private void Start()
    {
        // 오브젝트 파괴 시, Cancel
        token = this.GetCancellationTokenOnDestroy();

        // Event Link
        LinkEvent();

        StartClient();
    }
    async void StartClient()
    {
        Dispose();

        client = new TcpClient();

        try
        {
            // 서버 연결 및 스트림 반환
            Debug.Log("[TCPClient] Connecting to server");
            await client.ConnectAsync(serverIP, serverPort);
            Debug.Log("[TCPClient] Connected to server");
            stream = client.GetStream();

            // 서버 메세지 수신
            _ = UniTask.RunOnThreadPool(() => ReceiveMessageAsync());
        }
        catch (Exception e)
        {
            Debug.LogError($"[TCPClient] Connection Error: {e.Message}");
            OnServerDisconnectedEvent.Invoke();
        }
    }
    private async UniTask ReceiveMessageAsync()
    {
        byte[] buffer = new byte[1024];
        int bytesRead = 0;

        try
        {
            while (!token.IsCancellationRequested && client.Connected)
            {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break; // 서버가 연결을 끊었을 때

                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Debug.Log($"[TCPClient] Received message: {receivedMessage}");

                // string to Dictionary<string, string>
                var messageData = JsonConvert.DeserializeObject<Dictionary<string, string>>(receivedMessage);
                if (messageData == null) return;

                // 메세지 처리 함수
                await UniTask.SwitchToMainThread();
                ReceivedMessageProcess(messageData);
                await UniTask.SwitchToTaskPool();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[TCPClient] Error receiving message: {e.Message}");
        }
        finally
        {
            OnServerDisconnectedEvent.Invoke();
        }
    }

    private async UniTask SendMessageAsync(Dictionary<string, string> message)
    {
        // 메시지를 JSON 형식으로 직렬화
        string json = JsonConvert.SerializeObject(message);

        // JSON 데이터를 UTF-8 바이트 배열로 변환
        byte[] data = Encoding.UTF8.GetBytes(json);

        try
        {
            // 서버에 메시지 전송
            await stream.WriteAsync(data, 0, data.Length);
            Debug.Log($"[TCPClient] Sent message: {json}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[TCPClient] Error sending JSON message: {e.Message}");
        }
    }
    #endregion
    private void LinkEvent()
    {
        // UDPClient에서 IPEndPoint를 받으면, TCP 서버에 IPEndPoint 정보 메세지 전송
        UDPClient.Instance.OnIPEndPointReceivedEvent.AddListener(SendUDPIPEndPointMessage);

        // Client에서 매칭 요청 시, TCP 서버에 매칭 요청 메세지 전송
        OnMatchRequestedEvent.AddListener(SendMatchRequestMessage);

        // Client에서 매칭 취소 요청 시, TCP 서버에 매칭 취소 요청 메세지 전송
        OnMatchRequestCanceledEvent.AddListener(SendMatchRequestCancelMessage);

    }
    void ReceivedMessageProcess(Dictionary<string, string> message)
    {
        if (message.TryGetValue("Type", out string type))
        {
            switch (type)
            {
                case "Connect":
                    OnServerConnectedEvent.Invoke();
                    break;
                case "Match":
                    if(message.TryGetValue("Request", out string value))
                    {
                        if (value.Equals("Found"))
                            OnMatchFoundEvent.Invoke();
                        else if(value.Equals("Succeed"))
                        {
                            if(message.TryGetValue("ClientCount", out string count))
                            {
                                // 매칭된 클라이언트 수
                                int _count = int.Parse(count);
                                
                                // 클라이언트 IPEndPoint 담을 리스트
                                List<string> _matchedClientList = new List<string>();

                                // List에 IPEndPoint담기
                                for (int i = 0; i < _count; i++)
                                {
                                    if(message.TryGetValue($"Client{i}", out string iPEndPoint))
                                    {
                                        _matchedClientList.Add(iPEndPoint);
                                    }
                                }

                                // 이벤트 호출
                                OnMatchSucceedEvent.Invoke(_matchedClientList);
                            }
                        }
                        else if(value.Equals("Failed"))
                            OnMatchFailedEvent.Invoke();
                    }
                    break;
                default:
                    Debug.LogError("[TCPServer] Unknown message type received.");
                    break;
            }
        }
    }
    
    private void SendMatchRequestCancelMessage()
    {
        Dictionary<string, string> _message = new Dictionary<string, string>();

        _message.Add("Type", "Match");
        _message.Add("Request", "False");

        _ = SendMessageAsync(_message);
    }
    private void SendMatchRequestMessage()
    {
        Dictionary<string, string> _message = new Dictionary<string, string>();

        _message.Add("Type", "Match");
        _message.Add("Request", "True");

        _ = SendMessageAsync(_message);
    }
    private void SendUDPIPEndPointMessage(string ipEndPoint)
    {
        Dictionary<string, string> _message = new Dictionary<string, string>();

        _message.Add("Type", "Match");
        _message.Add("IPEndPoint", ipEndPoint);

        _ = SendMessageAsync(_message);
    }
    void Dispose()
    {
        client?.Close();
        client?.Dispose();
    }
    private void OnApplicationQuit()
    {
        Dispose();
    }
}

using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using System.Text;
using UnityEngine.Events;
using System.IO;

public class TCPClient : SingletonMonoBehaviour<TCPClient>
{
    [SerializeField] string serverIP;
    [SerializeField] int serverPort;

    TcpClient client;
    NetworkStream stream;
    CancellationToken token;

    public static event UnityAction OnServerConnectedEvent;
    public static event UnityAction OnServerDisconnectedEvent;
    public static event UnityAction<UserData> OnSignInSucceedEvent;
    public static event UnityAction OnSignInFailedEvent;
    public static event UnityAction OnSignUpSucceedEvent;
    public static event UnityAction OnSignUpFailedEvent;
    public static event UnityAction OnCreateNicknameSucceedEvent;
    public static event UnityAction OnCreateNicknameFailedEvent;
    public static event UnityAction OnMatchFoundEvent;
    public static event UnityAction<List<string>> OnMatchSucceedEvent;
    public static event UnityAction OnMatchFailedEvent;

    public UserData UserData { get; private set; }

    #region Basic TCP Client
    private void Start()
    {
        // ������Ʈ �ı� ��, Cancel
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
            // ���� ���� �� ��Ʈ�� ��ȯ
            Debug.Log("[TCPClient] Connecting to server");
            await client.ConnectAsync(serverIP, serverPort);
            Debug.Log("[TCPClient] Connected to server");
            stream = client.GetStream();

            await UniTask.Delay(1000);
            // ���� �޼��� ����
            _ = UniTask.RunOnThreadPool(() => ReceiveMessageAsync());
        }
        catch (Exception e)
        {
            Debug.LogError($"[TCPClient] Connection Error: {e.Message}");
            OnServerDisconnectedEvent?.Invoke();
        }
    }
    private async UniTask ReceiveMessageAsync()
    {
        byte[] buffer = new byte[1024];
        int bytesRead = 0;

        try
        {
            while (!token.IsCancellationRequested && client.Connected && stream.CanRead)
            {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break; // ������ ������ ������ ��

                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Debug.Log($"[TCPClient] Received message: {receivedMessage}");

                // string to Dictionary<string, string>
                var messageData = JsonConvert.DeserializeObject<Dictionary<string, string>>(receivedMessage);
                if (messageData == null) return;

                // �޼��� ó�� �Լ�
                await UniTask.SwitchToMainThread();
                ReceivedMessageProcess(messageData);
                await UniTask.SwitchToTaskPool();
            }
        }
        catch (IOException e)
        {
            Debug.LogError($"[TCPClient] Network error : {e.Message}");
        }
        catch (ObjectDisposedException)
        {
            Debug.LogWarning("[TCPClient] Stream has already been closed");
        }
        catch (Exception e)
        {
            Debug.LogError($"[TCPClient] Unknown error occurred: {e.Message}");
        }
        finally
        {
            OnServerDisconnectedEvent?.Invoke();
        }
    }

    private async UniTask SendMessageAsync(Dictionary<string, string> message)
    {
        // �޽����� JSON �������� ����ȭ
        string json = JsonConvert.SerializeObject(message);

        // JSON �����͸� UTF-8 ����Ʈ �迭�� ��ȯ
        byte[] data = Encoding.UTF8.GetBytes(json);

        try
        {
            // ������ �޽��� ����
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
        // UDPClient���� IPEndPoint�� ������, TCP ������ IPEndPoint ���� �޼��� ����
        UDPClient.OnIPEndPointReceivedEvent += SendUDPIPEndPointMessage;
    }
    void ReceivedMessageProcess(Dictionary<string, string> message)
    {
        if (message.TryGetValue("Type", out string type))
        {
            switch (type)
            {
                case "Connect":
                    OnServerConnectedEvent?.Invoke();
                    break;
                case "SignIn":
                    if (message.TryGetValue("Result", out string signIn_Result))
                    {
                        if (signIn_Result == "Succeed")
                        {
                            Debug.Log("�α��� ����");
                            if(message.TryGetValue("UserData", out string userDataJson))
                            {
                                UserData = new UserData();
                                UserData = JsonConvert.DeserializeObject<UserData>(userDataJson);
                                OnSignInSucceedEvent?.Invoke(UserData);
                            }
                        }
                        else if(signIn_Result == "Failed")
                        {
                            Debug.Log("�α��� ����");
                            OnSignInFailedEvent?.Invoke();
                        }
                    }
                    break;
                case "SignUp":
                    if(message.TryGetValue("Result", out string signUp_result))
                    {
                        if (signUp_result == "Succeed")
                        {
                            Debug.Log("���� ����");
                            OnSignUpSucceedEvent?.Invoke();
                        }
                        else if (signUp_result == "Failed")
                        {
                            Debug.Log("���� ����");
                            OnSignUpFailedEvent?.Invoke();
                        }
                    }
                    break;
                case "SetNickname":
                    if (message.TryGetValue("Result", out string setNickname_result))
                    {
                        if (setNickname_result == "Succeed" && message.TryGetValue("Nickname", out string nickname))
                        {
                            Debug.Log("�г��� ����� ����");
                            UserData.Nickname = nickname;
                            OnCreateNicknameSucceedEvent?.Invoke();
                        }
                        else if (setNickname_result == "Failed")
                        {
                            Debug.Log("�г��� ����� ����");
                            OnCreateNicknameFailedEvent?.Invoke();
                        }
                    }
                    break;
                case "Match":
                    if(message.TryGetValue("Request", out string value))
                    {
                        if (value.Equals("Found"))
                            OnMatchFoundEvent?.Invoke();
                        else if(value.Equals("Succeed"))
                        {
                            if(message.TryGetValue("ClientCount", out string count))
                            {
                                // ��Ī�� Ŭ���̾�Ʈ ��
                                int _count = int.Parse(count);
                                
                                // Ŭ���̾�Ʈ IPEndPoint ���� ����Ʈ
                                List<string> _matchedClientList = new List<string>();

                                // List�� IPEndPoint���
                                for (int i = 0; i < _count; i++)
                                {
                                    if(message.TryGetValue($"Client{i}", out string iPEndPoint))
                                    {
                                        _matchedClientList.Add(iPEndPoint);
                                    }
                                }

                                // �̺�Ʈ ȣ��
                                OnMatchSucceedEvent?.Invoke(_matchedClientList);
                            }
                        }
                        else if(value.Equals("Failed"))
                            OnMatchFailedEvent?.Invoke();
                    }
                    break;
                default:
                    Debug.LogError("[TCPServer] Unknown message type received.");
                    break;
            }
        }
    }
    public void SendSignInRequestMessage(string id, string password)
    {
        Dictionary<string, string> _message = new Dictionary<string, string>
        {
            {"Type", "SignIn" },
            {"ID", id },
            {"Password", password}
        };

        _ = SendMessageAsync(_message);
    }
    public void SendSignUpRequestMessage(string id, string password)
    {
        Dictionary<string, string> _message = new Dictionary<string, string>
        {
            {"Type", "SignUp" },
            {"ID", id },
            {"Password", password}
        };

        _ = SendMessageAsync(_message);
    }
    public void SendCreateNicknameRequestMessage(string nickname)
    {
        Dictionary<string, string> _message = new Dictionary<string, string>
        {
            {"Type", "SetNickname" },
            {"GUID", UserData.GUID},
            {"Nickname", nickname }
        };

        _ = SendMessageAsync(_message);
    }
    public void SendUserDataRequestMessage()
    {
        Dictionary<string, string> _message = new Dictionary<string, string>
        {
            {"Type", "Data" },
            {"GUID", UserData.GUID }
        };

        _ = SendMessageAsync(_message);
    }
    public void SendMatchCancelRequestMessage()
    {
        Dictionary<string, string> _message = new Dictionary<string, string>();

        _message.Add("Type", "Match");
        _message.Add("Request", "False");

        _ = SendMessageAsync(_message);
    }
    public void SendMatchRequestMessage()
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
        stream?.Close();
        stream?.Dispose();

        client?.Close();
        client?.Dispose();
    }
    private void OnDestroy()
    {
        Dispose();
    }
}

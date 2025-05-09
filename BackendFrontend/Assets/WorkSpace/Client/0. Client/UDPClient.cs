using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Net;
using System;
using UnityEngine.Events;
using System.Collections.Generic;

public class UDPClient : SingletonMonoBehaviour<UDPClient>
{
    [SerializeField] string stunServerIP;
    [SerializeField] int stunServerPort;

    public List<IPEndPoint> MatchedClient { get; private set; } = new List<IPEndPoint>();
    public int Order { get; private set; }
    public static event UnityAction<string> OnIPEndPointReceivedEvent;
    public static event UnityAction OnGameStartEvent;
    public static event UnityAction OnGameExitEvent;
    public int HostOrder { get; set; }

    UdpClient udpClient;
    IPEndPoint stunServerIPEndPoint;
    CancellationToken token;
    string m_IPEndPoint;
    bool isPlay;
    void Start()
    {
        // ������Ʈ �ı� ��, Cancel
        token = this.GetCancellationTokenOnDestroy();

        // ���� ����
        stunServerIPEndPoint = new IPEndPoint(IPAddress.Parse(stunServerIP), stunServerPort);

        LinkEvent();
    }
    #region Basic UDP Client
    public async UniTask SendMessageAsync(string message, IPEndPoint ipEndPoint)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        await udpClient.SendAsync(data, data.Length, ipEndPoint);
        Debug.Log($"[UDPClient] Sent Message: {message}");
    }

    async UniTask ReceiveMessageAsync()
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                UdpReceiveResult result = await udpClient.ReceiveAsync();
                string message = Encoding.UTF8.GetString(result.Buffer);

                // �޼��� ó�� �Լ�
                await UniTask.SwitchToMainThread();
                ReceivedMessageProcess(result, message);
                await UniTask.SwitchToTaskPool();
            }
            catch (Exception e)
            {
                Debug.LogError($"[UDPClient] Error receiving message: {e.Message}");
            }
        }
    }
    public void BroadcastMessage_Position(int _index, Vector3 position)
    {
        string _message = $"{_index},{position.x},{position.y},{position.z}";

        _ = BroadcastMessageAsync(_message);
    }
    public async UniTask BroadcastMessageAsync(string message)
    {
        for(int i = 1; i < MatchedClient.Count; i++)
        {
            _ = SendMessageAsync(message, MatchedClient[i]);
        }
    }
    #endregion
    private void LinkEvent()
    {
        // ��Ī�� ã���� ��, UDP Ŭ���̾�Ʈ ����
        //TCPClient.OnMatchFoundEvent += StartClient;

        // ��Ī ���� ��, ���� ����
        //TCPClient.OnMatchSucceedEvent += StartGame;
    }
    void StartClient()
    {
        // ����
        Dispose();

        // ����
        udpClient = new UdpClient();

        // �޼��� ����
        _ = UniTask.RunOnThreadPool(() => ReceiveMessageAsync());

        // ���ϼ����� �޼��� ����
        _ = SendMessageAsync($"Request Address & Port", stunServerIPEndPoint);
    }
    void StartGame(List<string> matchedClient)
    {
        for (int i = 0; i < matchedClient.Count; i++)
        {
            if (matchedClient[i] == m_IPEndPoint)
                Order = i;

            MatchedClient.Add(Global.StringToIPEndPoint(matchedClient[i]));
        }

        OnGameStartEvent?.Invoke();
        isPlay = true;
    }
    void ReceivedMessageProcess(UdpReceiveResult udpReceiveResult, string message)
    {
        // ���� �������� �޾��� ���
        if (udpReceiveResult.RemoteEndPoint.Equals(stunServerIPEndPoint))
        {
            Debug.Log($"[UDPClient] Received message from StunServer: {message}");
            m_IPEndPoint = message;
            OnIPEndPointReceivedEvent?.Invoke(message);
        }
        else
        {
            // �޼��� Type ���� ó�� �߰� �ʿ�
            for(int i = 0; i < MatchedClient.Count; i++)
            {
                // Host���׼� ���� ���
                if (udpReceiveResult.RemoteEndPoint.Equals(MatchedClient[HostOrder]))
                {
                    // �޽��� �Ľ� �� ��ǥ �� ����
                    string[] _values = message.Split(',');
                    if (_values.Length == 4)
                    {
                        Vector3 pos = new Vector3(
                            float.Parse(_values[1]),
                            float.Parse(_values[2]),
                            float.Parse(_values[3])
                        );

                        // ����ȭ
                        Game.Instance.players[int.Parse(_values[0])].GetComponent<PlayerController>().SetPosition(pos);
                    }
                }
                else
                {
                    string[] _values = message.Split(',');
                    int _index = int.Parse(_values[0]);
                    Game.Instance.players[_index].GetComponent<PlayerController>().NewInput(new Vector2(float.Parse(_values[1]), float.Parse(_values[2])));
                }
            }

            Debug.Log($"[UDPClient] Received message: {message}");
        }
    }
    private void Dispose()
    {
        udpClient?.Close();
        udpClient?.Dispose();
    }
    private void OnDestroy()
    {
        Dispose();
    }
}

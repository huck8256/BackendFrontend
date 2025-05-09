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
        // 오브젝트 파괴 시, Cancel
        token = this.GetCancellationTokenOnDestroy();

        // 스턴 서버
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

                // 메세지 처리 함수
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
        // 매칭을 찾았을 시, UDP 클라이언트 실행
        //TCPClient.OnMatchFoundEvent += StartClient;

        // 매칭 성공 시, 게임 시작
        //TCPClient.OnMatchSucceedEvent += StartGame;
    }
    void StartClient()
    {
        // 제거
        Dispose();

        // 생성
        udpClient = new UdpClient();

        // 메세지 수신
        _ = UniTask.RunOnThreadPool(() => ReceiveMessageAsync());

        // 스턴서버에 메세지 전송
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
        // 스턴 서버에서 받았을 경우
        if (udpReceiveResult.RemoteEndPoint.Equals(stunServerIPEndPoint))
        {
            Debug.Log($"[UDPClient] Received message from StunServer: {message}");
            m_IPEndPoint = message;
            OnIPEndPointReceivedEvent?.Invoke(message);
        }
        else
        {
            // 메세지 Type 마다 처리 추가 필요
            for(int i = 0; i < MatchedClient.Count; i++)
            {
                // Host한테서 왔을 경우
                if (udpReceiveResult.RemoteEndPoint.Equals(MatchedClient[HostOrder]))
                {
                    // 메시지 파싱 후 좌표 값 적용
                    string[] _values = message.Split(',');
                    if (_values.Length == 4)
                    {
                        Vector3 pos = new Vector3(
                            float.Parse(_values[1]),
                            float.Parse(_values[2]),
                            float.Parse(_values[3])
                        );

                        // 동기화
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

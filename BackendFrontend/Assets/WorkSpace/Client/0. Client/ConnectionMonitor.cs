using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;

public class ConnectionMonitor : IConnectionMonitor
{
    private readonly ITCPClient tcpClient;
    private CancellationTokenSource token;
    private readonly CompositeDisposable disposables = new CompositeDisposable();
    float lastPongTime;
    readonly float interval;
    readonly float timeout;
    readonly float checkInterval;
    PongHandler pongHandler;
    public ConnectionMonitor(ITCPClient client, PongHandler pongHandler, float interval, float timeout, float checkInterval)
    {
        this.tcpClient = client;
        this.pongHandler = pongHandler;
        this.interval = interval;
        this.timeout = timeout;
        this.checkInterval = checkInterval;
        lastPongTime = Time.time;
    }
    public void StartMonitoring()
    {
        _ = StartPingLoopAsync();

        Observable.Interval(TimeSpan.FromSeconds(checkInterval))
            .Subscribe(_ => CheckConnection()).AddTo(disposables);
    }
    public void Initialize()
    {
        token = new CancellationTokenSource();
        pongHandler.OnPong += UpdatePongTime;
    }
    public void Dispose()
    {
        token?.Cancel();
        token?.Dispose();
        token = null;

        disposables.Clear();
        disposables.Dispose();

        pongHandler.OnPong -= UpdatePongTime;
    }
    private void UpdatePongTime() => lastPongTime = Time.time;
    private async UniTask StartPingLoopAsync()
    {
        while (token != null && !token.IsCancellationRequested)
        {
            try { await SendPingAsync(); }
            catch (Exception e)
            {
                Debug.LogWarning($"[TCPClient] Ping Failed: {e.Message}");
            }

            await UniTask.Delay(TimeSpan.FromSeconds(interval));
        }
    }
    private async UniTask SendPingAsync() => await tcpClient.SendMessageAsync(PacketID.Ping, PacketBuilder.CreateBody(("Timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString())));
    private void CheckConnection()
    {
        if (Time.time - lastPongTime > timeout)
        {
            // 辑滚 立加 谗扁 贸府
            tcpClient?.HeartBeatDisconnection();
        }
    }
}

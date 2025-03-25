using Cysharp.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine;
using System.Threading;
using System;

public class StunServer : MonoBehaviour
{
    [SerializeField] int port;

    UdpClient udpClient;
    CancellationToken token;
    void Awake()
    {
        string localIP = Global.GetLocalIPAddress();
        string publicIP = Global.GetPublicIPAddress();

        Debug.Log($"[StunServer] Running on IP: {publicIP}, Port: {port}");
    }
    void Start()
    {
        // ������Ʈ �ı� ��, Cancel
        token = this.GetCancellationTokenOnDestroy();

        udpClient = new UdpClient(port);

        // �޼��� ����
        _ = UniTask.RunOnThreadPool(() => ReceiveMessageAsync());
    }
    #region Basic StunServer(UDP)
    async UniTask ReceiveMessageAsync()
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                UdpReceiveResult result = await udpClient.ReceiveAsync();
                string message = Encoding.UTF8.GetString(result.Buffer);
                IPEndPoint clientEndPoint = result.RemoteEndPoint;

                Debug.Log($"[StunServer] Received message: {message}");

                // �޼��� ���� (UDP Ȧ��Ī�� ����)
                _ = SendMessageAsync(clientEndPoint.ToString(), clientEndPoint);
            }
            catch (Exception e)
            {
                Debug.LogError($"[StunServer] Error receiving message: {e.Message}");
            }
        }
    }
    async UniTask SendMessageAsync(string message, IPEndPoint iPEndPoint)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        await udpClient.SendAsync(data, data.Length, iPEndPoint);
        Debug.Log($"[StunServer] Sent message: {message}");
    }
    #endregion
    private void Dispose()
    {
        udpClient?.Close();
        udpClient?.Dispose();
    }
    private void OnApplicationQuit()
    {
        Dispose();
    }
}

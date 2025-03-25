using System.Net.Sockets;
using System.Net;
using UnityEngine;

public class IPEndPointGetter : MonoBehaviour
{
    private UdpClient client;

    void Start()
    {
        client = new UdpClient();
        client.Connect("8.8.8.8", 12345); // 구글 DNS 서버와 가짜 연결 (포트는 아무거나)

        IPEndPoint localEndPoint = (IPEndPoint)client.Client.LocalEndPoint;
        Debug.Log($"[내 IP] {localEndPoint.Address} [내 포트] {localEndPoint.Port}");
    }

    void OnApplicationQuit()
    {
        client?.Close();
    }
}

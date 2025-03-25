using System.Net.Sockets;
using System.Net;
using UnityEngine;

public class IPEndPointGetter : MonoBehaviour
{
    private UdpClient client;

    void Start()
    {
        client = new UdpClient();
        client.Connect("8.8.8.8", 12345); // ���� DNS ������ ��¥ ���� (��Ʈ�� �ƹ��ų�)

        IPEndPoint localEndPoint = (IPEndPoint)client.Client.LocalEndPoint;
        Debug.Log($"[�� IP] {localEndPoint.Address} [�� ��Ʈ] {localEndPoint.Port}");
    }

    void OnApplicationQuit()
    {
        client?.Close();
    }
}

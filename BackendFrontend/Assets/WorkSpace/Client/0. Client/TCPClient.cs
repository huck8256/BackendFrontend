using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using System.Text;
using System.IO;

public class TCPClient : ITCPClient
{
    public TCPClient(string ip, int port, PacketRouter packetRouter)
    {
        serverIP = ip;
        serverPort = port;
        this.packetRouter = packetRouter;
    }

    readonly PacketRouter packetRouter;

    public event Action OnConnected;
    public event Action OnDisconnected;

    TcpClient client;
    NetworkStream stream;
    CancellationTokenSource token;

    readonly string serverIP;
    readonly int serverPort;

    public void Initialize()
    {
        token = new CancellationTokenSource();

        // UDPClient에서 IPEndPoint를 받으면, TCP 서버에 IPEndPoint 정보 메세지 전송
        UDPClient.OnIPEndPointReceivedEvent += SendUDPIPEndPointMessage;
    }
    public void Dispose()
    {
        token?.Cancel();
        token?.Dispose();
        token = null;

        stream?.Close();
        stream?.Dispose();
        stream = null;

        client?.Close();
        client?.Dispose();
        client = null;

        UDPClient.OnIPEndPointReceivedEvent -= SendUDPIPEndPointMessage;
    }
    public async UniTask ConnectAsync()
    {
        client = new TcpClient();

        try
        {
            // 서버 연결 및 스트림 반환
            Debug.Log("[TCPClient] Connecting...");
            await client.ConnectAsync(serverIP, serverPort);
            stream = client.GetStream();
            Debug.Log("[TCPClient] Connected!");
            OnConnected?.Invoke();

            // 서버 메세지 수신
            _ = UniTask.RunOnThreadPool(() => StartReceiveMessageAsync());
        }
        catch (Exception e)
        {
            Debug.LogError($"[TCPClient] Connect Error: {e.Message}");
            OnDisconnected?.Invoke();
        }
    }
    public async UniTask SendMessageAsync(PacketID packetID, Dictionary<string, string> body)
    {
        if(body == null)
        {
            Debug.LogError($"[TCPClient] Body is null");
            return;
        }

        // Body 직렬화
        string json = JsonConvert.SerializeObject(body);
        byte[] bodyBytes = Encoding.UTF8.GetBytes(json);

        // Header 생성
        int totalSize = PacketHeader.Size + bodyBytes.Length;
        byte[] packet = new byte[totalSize];

        // Header 삽입
        Array.Copy(BitConverter.GetBytes(totalSize), 0, packet, 0, 4);          // Packet Size
        Array.Copy(BitConverter.GetBytes((ushort)packetID), 0, packet, 4, 2);   // Packet ID

        // Body 삽입
        Array.Copy(bodyBytes, 0, packet, PacketHeader.Size, bodyBytes.Length);

        // 전송
        try
        {
            await stream.WriteAsync(packet, 0, packet.Length);
            Debug.Log($"[TCPClient] Sent message: {packetID}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[TCPClient] Error sending message: {e.Message}");
        }
    }
    public async UniTask StartReceiveMessageAsync()
    {
        byte[] headerBuffer = new byte[PacketHeader.Size];

        try
        {
            while (client.Connected && !token.IsCancellationRequested)
            {
                // Header 수신
                int headerRead = await stream.ReadAsync(headerBuffer, 0, PacketHeader.Size);
                if (headerRead < PacketHeader.Size) break;

                int packetSize = BitConverter.ToInt32(headerBuffer, 0);
                ushort packetID = BitConverter.ToUInt16(headerBuffer, 4);

                int bodySize = packetSize - PacketHeader.Size;

                // Body 수신
                byte[] boddyBuffer = new byte[bodySize];
                // Body Byte데이터를 BodySize만큼 다 읽기 위한 로직
                int totalRead = 0;
                while (totalRead < bodySize)
                {
                    int read = await stream.ReadAsync(boddyBuffer, totalRead, bodySize - totalRead);
                    if (read == 0)
                    {
                        Debug.LogWarning("[TCPClient] Server Disconnected");
                        break;
                    }
                    totalRead += read;
                }

                string json = Encoding.UTF8.GetString(boddyBuffer);
                var body = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                await UniTask.SwitchToMainThread();
                HandlePacket(packetID, body);
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
            OnDisconnected?.Invoke();
        }
    }
    private void HandlePacket(ushort packetID, Dictionary<string, string> body) => packetRouter.Route((PacketID)packetID, body);
    private void SendUDPIPEndPointMessage(string ipEndPoint) => _ = SendMessageAsync(PacketID.Matching, PacketBuilder.CreateBody(("IPEndPoint", ipEndPoint)));
    public void HeartBeatDisconnection() => OnDisconnected?.Invoke();
}
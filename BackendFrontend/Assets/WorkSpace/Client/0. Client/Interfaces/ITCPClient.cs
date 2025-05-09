using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;

public interface ITCPClient : IInitializable, IDisposable
{
    event Action OnConnected;
    event Action OnDisconnected;
    
    UniTask ConnectAsync();
    UniTask StartReceiveMessageAsync();
    UniTask SendMessageAsync(PacketID packetID, Dictionary<string, string> message);

    void HeartBeatDisconnection();
}
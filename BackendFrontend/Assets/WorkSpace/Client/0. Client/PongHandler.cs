using System;
using System.Collections.Generic;
using UnityEngine;

public class PongHandler : IPacketHandler
{
    public event Action OnPong;
    public PacketID ID => PacketID.Pong;
    public void Handle(Dictionary<string, string> body)
    {
        if (body.TryGetValue("Timestamp", out string ts))
        {
            long sentTime = long.Parse(ts);
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Debug.Log($"[TCPClient] 지연 시간: {now - sentTime} ms");
        }

        OnPong?.Invoke();
    }
}

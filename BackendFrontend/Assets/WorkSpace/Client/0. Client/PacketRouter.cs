using System.Collections.Generic;
public class PacketRouter
{
    public readonly Dictionary<PacketID, IPacketHandler> Handlers;

    public PacketRouter(params IPacketHandler[] handlers)
    {
        this.Handlers = new Dictionary<PacketID, IPacketHandler>();
        foreach (var h in handlers)
        {
            this.Handlers[h.ID] = h;
        }
    }
    public void Route(PacketID packetID, Dictionary<string,string> body)
    {
        if (Handlers.TryGetValue(packetID, out var handler))
        {
            handler.Handle(body);
        }
    }
}

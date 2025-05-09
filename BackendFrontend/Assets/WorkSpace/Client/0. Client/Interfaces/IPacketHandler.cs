using System.Collections.Generic;

public interface IPacketHandler
{
    PacketID ID { get; }
    void Handle(Dictionary<string, string> body);
}

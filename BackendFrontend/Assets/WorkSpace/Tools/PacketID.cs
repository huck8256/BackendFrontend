public enum PacketID : ushort
{
    SignIn = 2001,
    SignUp = 2002,
    SetNickname = 2003,
    Matching = 3001,
    Data = 4001,
    GameSync = 5001,
    Ping = 9999,
    Pong = 9998
}
public struct PacketHeader
{
    public int PacketSize;   // 전체 패킷 크기
    public ushort PacketID;     // 패킷 종류

    public const int Size = 6;
}

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
    public int PacketSize;   // ��ü ��Ŷ ũ��
    public ushort PacketID;     // ��Ŷ ����

    public const int Size = 6;
}

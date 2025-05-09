using UnityEngine;

public class NetworkManager : SingletonMonoBehaviour<NetworkManager>
{
    [Header("TCPClient")]
    [SerializeField] string tcpServerIP;
    [SerializeField] int tcpServerPort;

    public TCPClient TCPClient { get; private set; }
    public ConnectionMonitor ConnectionMonitor { get; private set; }
    public UserSession UserSession { get; private set; }
    public PacketRouter PacketRouter { get; private set; }
    public PongHandler PongHandler { get; private set; } = new PongHandler();
    public SignInHandler SignInHandler { get; private set; } = new SignInHandler();
    public SignUpHandler SignUpHandler { get; private set; } = new SignUpHandler();
    public SetNicknameHandler SetNicknameHandler { get; private set; } = new SetNicknameHandler();
    public MatchingHandler MatchingHandler { get; private set; } = new MatchingHandler();

    protected override void Awake()
    {
        base.Awake();

        PacketRouter = new PacketRouter(PongHandler, SignInHandler, SignUpHandler, SetNicknameHandler, MatchingHandler);
        TCPClient = new TCPClient(tcpServerIP, tcpServerPort, PacketRouter);
        ConnectionMonitor = new ConnectionMonitor(TCPClient, PongHandler, 5f, 10f, 1f);
        UserSession = new UserSession();
    }
    void OnEnable()
    {
        TCPClient?.Initialize();
        ConnectionMonitor?.Initialize();

        TCPClient.OnDisconnected += DisposeClientWithMonitor;
        SignInHandler.OnSucceed += OnSignInSucceed;
        SetNicknameHandler.OnSucceed += OnSetNicknameSucceed;
    }
    async void Start()
    {
        await TCPClient.ConnectAsync();
        ConnectionMonitor.StartMonitoring();
    }
    private void OnDisable()
    {
        DisposeClientWithMonitor();

        TCPClient.OnDisconnected -= DisposeClientWithMonitor;
        SignInHandler.OnSucceed -= OnSignInSucceed;
        SetNicknameHandler.OnSucceed -= OnSetNicknameSucceed;
    }
    public void OnSignInSucceed(UserData userData) => UserSession?.SignIn(userData);
    public void OnSetNicknameSucceed(string nickname) => UserSession?.SetNickname(nickname);
    public void SendSignInRequestMessage(string id, string password) => _ = TCPClient.SendMessageAsync(PacketID.SignIn, PacketBuilder.CreateBody(("ID", id), ("Password", password)));
    public void SendSignUpRequestMessage(string id, string password) => _ = TCPClient.SendMessageAsync(PacketID.SignUp, PacketBuilder.CreateBody(("ID", id), ("Password", password)));
    public void SendSetNicknameRequestMessage(string nickname)
    {
        if (string.IsNullOrEmpty(UserSession.UserData.GUID))
            return;

        _ = TCPClient.SendMessageAsync(PacketID.SetNickname, PacketBuilder.CreateBody(("GUID", UserSession.UserData.GUID), ("Nickname", nickname)));
    }
    public void SendUserDataRequestMessage()
    {
        if (string.IsNullOrEmpty(UserSession.UserData.GUID))
            return;

        _ = TCPClient.SendMessageAsync(PacketID.Data, PacketBuilder.CreateBody(("GUID", UserSession.UserData.GUID)));
    }
    public void SendMatchCancelRequestMessage() => _ = TCPClient.SendMessageAsync(PacketID.Matching, PacketBuilder.CreateBody(("Result", "Canceled")));
    public void SendMatchRequestMessage() => _ = TCPClient.SendMessageAsync(PacketID.Matching, PacketBuilder.CreateBody(("Result", "Requested")));
    void DisposeClientWithMonitor()
    {
        Debug.LogError("[TCPClient] ¼­¹ö ¿¬°á ²÷±è!");
        TCPClient?.Dispose();
        ConnectionMonitor?.Dispose();
    }
}

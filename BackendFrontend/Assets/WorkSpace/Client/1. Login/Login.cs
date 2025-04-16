using UnityEngine;

public class Login : SingletonMonoBehaviour<Login>
{
    [SerializeField] LoginUI loginUI;

    public void RequestSignIn(string id, string password) => TCPClient.Instance?.SendSignInRequestMessage(id, password);
    public void RequestSignUp(string id, string password) => TCPClient.Instance?.SendSignUpRequestMessage(id, password);
    public void RequestCreateNickname(string nickname) => TCPClient.Instance?.SendCreateNicknameRequestMessage(nickname);
    private void OnEnable()
    {
        TCPClient.OnServerConnectedEvent += HandleServerConnected;
        TCPClient.OnServerDisconnectedEvent += HandleServerDisconnected;
        TCPClient.OnSignInSucceedEvent += HandleSignInSucceed;
        TCPClient.OnSignInFailedEvent += HandleSignInFailed;
        TCPClient.OnSignUpSucceedEvent += HandleSignUpSucceed;
        TCPClient.OnSignUpFailedEvent += HandleSignUpFailed;
        TCPClient.OnCreateNicknameSucceedEvent += HandleCreateNicknameSucceed;
        TCPClient.OnCreateNicknameFailedEvent += HandleCreateNicknameFailed;
    }
    private void OnDisable()
    {
        TCPClient.OnServerConnectedEvent -= HandleServerConnected;
        TCPClient.OnServerDisconnectedEvent -= HandleServerDisconnected;
        TCPClient.OnSignInSucceedEvent -= HandleSignInSucceed;
        TCPClient.OnSignInFailedEvent -= HandleSignInFailed;
        TCPClient.OnSignUpSucceedEvent -= HandleSignUpSucceed;
        TCPClient.OnSignUpFailedEvent -= HandleSignUpFailed;
        TCPClient.OnCreateNicknameSucceedEvent -= HandleCreateNicknameSucceed;
        TCPClient.OnCreateNicknameFailedEvent -= HandleCreateNicknameFailed;
    }
    private void HandleServerConnected() { }
    private void HandleServerDisconnected() { }
    private void HandleSignInSucceed(UserData userData)
    {
        loginUI.HandleSignInSucceed();

        if (userData.Nickname.Equals("Unknown"))
            loginUI.HandleCheckNickname(false);
        else
        {
            loginUI.HandleCheckNickname(true);
            SceneController.Instance.MoveScene("Lobby");
        }
    }
    private void HandleSignInFailed() => loginUI.HandleSignInFailed();
    private void HandleSignUpSucceed() => loginUI.HandleSignUpSucceed();
    private void HandleSignUpFailed() => loginUI.HandleSignUpFailed();
    private void HandleCreateNicknameSucceed()
    {
        loginUI.HandleCreateNicknameSucceed();
        SceneController.Instance.MoveScene("Lobby");
    }
    private void HandleCreateNicknameFailed() => loginUI.HandleCreateNicknameFailed();
}

using System;
using Unity.VisualScripting;
using UnityEngine.Events;

public class LoginModel : IInitializable, IDisposable, ILoginService
{
    public LoginModel() { }

    #region Public Variable
    public event UnityAction<UserData> OnSignInSucceed;
    public event UnityAction OnSignInFailed;
    public event UnityAction OnSignUpSucceed;
    public event UnityAction OnSignUpFailed;
    public event UnityAction OnCreateNicknameSucceed;
    public event UnityAction OnCreateNicknameFailed;
    #endregion
    #region Private Variable
    #endregion
    public void Initialize()
    {
        TCPClient.OnSignInSucceedEvent += HandleSignInSucceed;
        TCPClient.OnSignInFailedEvent += HandleSignInFailed;
        TCPClient.OnSignUpSucceedEvent += HandleSignUpSucceed;
        TCPClient.OnSignUpFailedEvent += HandleSignUpFailed;
        TCPClient.OnCreateNicknameSucceedEvent += HandleCreateNicknameSucceed;
        TCPClient.OnCreateNicknameFailedEvent += HandleCreateNicknameFailed;
    }
    public void Dispose()
    {
        TCPClient.OnSignInSucceedEvent -= HandleSignInSucceed;
        TCPClient.OnSignInFailedEvent -= HandleSignInFailed;
        TCPClient.OnSignUpSucceedEvent -= HandleSignUpSucceed;
        TCPClient.OnSignUpFailedEvent -= HandleSignUpFailed;
        TCPClient.OnCreateNicknameSucceedEvent -= HandleCreateNicknameSucceed;
        TCPClient.OnCreateNicknameFailedEvent -= HandleCreateNicknameFailed;
    }

    public void RequestSignIn(string id, string pw) => TCPClient.Instance?.SendSignInRequestMessage(id, pw);
    public void RequestSignUp(string id, string pw) => TCPClient.Instance?.SendSignUpRequestMessage(id, pw);
    public void RequestCreateNickname(string nickname) => TCPClient.Instance?.SendCreateNicknameRequestMessage(nickname);

    private void HandleSignInSucceed(UserData userData) => OnSignInSucceed?.Invoke(userData);
    private void HandleSignInFailed() => OnSignInFailed?.Invoke();
    private void HandleSignUpSucceed() => OnSignUpSucceed?.Invoke();
    private void HandleSignUpFailed() => OnSignUpFailed?.Invoke();
    private void HandleCreateNicknameSucceed() => OnCreateNicknameSucceed?.Invoke();
    private void HandleCreateNicknameFailed() => OnCreateNicknameFailed?.Invoke();
}

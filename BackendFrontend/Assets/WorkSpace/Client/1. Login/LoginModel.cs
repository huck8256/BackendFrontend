using System;
public class LoginModel : ILoginService
{
    public LoginModel(SignInHandler signInHandler, SignUpHandler signUpHandler, SetNicknameHandler setNicknameHandler) 
    { 
        this.signInHandler = signInHandler; 
        this.signUpHandler = signUpHandler;
        this.setNicknameHandler = setNicknameHandler;
    }

    #region Public Variable
    public event Action<UserData> OnSignInSucceed;
    public event Action OnSignInFailed;
    public event Action OnSignUpSucceed;
    public event Action OnSignUpFailed;
    public event Action<string> OnSetNicknameSucceed;
    public event Action OnSetNicknameFailed;
    #endregion

    SignInHandler signInHandler;
    SignUpHandler signUpHandler;
    SetNicknameHandler setNicknameHandler;
    public void Initialize()
    {
        signInHandler.OnSucceed += HandleSignInSucceed;
        signInHandler.OnFailed += HandleSignInFailed;
        signUpHandler.OnSucceed += HandleSignUpSucceed;
        signUpHandler.OnFailed += HandleSignUpFailed;
        setNicknameHandler.OnSucceed += HandleSetNicknameSucceed;
        setNicknameHandler.OnFailed += HandleSetNicknameFailed;
    }
    public void Dispose()
    {
        signInHandler.OnSucceed -= HandleSignInSucceed;
        signInHandler.OnFailed -= HandleSignInFailed;
        signUpHandler.OnSucceed -= HandleSignUpSucceed;
        signUpHandler.OnFailed -= HandleSignUpFailed;
        setNicknameHandler.OnSucceed -= HandleSetNicknameSucceed;
        setNicknameHandler.OnFailed -= HandleSetNicknameFailed;
    }
    public void RequestSignIn(string id, string pw) => NetworkManager.Instance?.SendSignInRequestMessage(id, pw);
    public void RequestSignUp(string id, string pw) => NetworkManager.Instance?.SendSignUpRequestMessage(id, pw);
    public void RequestSetNickname(string nickname) => NetworkManager.Instance?.SendSetNicknameRequestMessage(nickname);
    private void HandleSignInSucceed(UserData userData) => OnSignInSucceed?.Invoke(userData);
    private void HandleSignInFailed() => OnSignInFailed?.Invoke();
    private void HandleSignUpSucceed() => OnSignUpSucceed?.Invoke();
    private void HandleSignUpFailed() => OnSignUpFailed?.Invoke();
    private void HandleSetNicknameSucceed(string nickName) => OnSetNicknameSucceed?.Invoke(nickName);
    private void HandleSetNicknameFailed() => OnSetNicknameFailed?.Invoke();
}

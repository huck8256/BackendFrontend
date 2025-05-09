using System;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
using Unit = UniRx.Unit;
public class LoginViewModel : IInitializable, IDisposable 
{
    public LoginViewModel(ILoginService loginService)
    {
        this.loginService = loginService;

        onSignInSucceedHandler = HandleSignInSucceed;
        onSignInFailedHandler = HandleSignInFailed;
        onSignUpSucceedHandler = HandleSignUpSucceed;
        onSignUpFailedHandler = HandleSignUpFailed;
        onSetNicknameSucceedHandler = HandleSetNicknameSucceed;
        onSetNicknameFailedHandler = HandleSetNicknameFailed;
    }
  

    #region Public Variable
    public IReactiveProperty<string> SignInID => signInID;
    public IReactiveProperty<string> SignInPassword => signInPassword;
    public IReactiveProperty<string> SignUpID => signUpID;
    public IReactiveProperty<string> SignUpPassword => signUpPassword;
    public IReactiveProperty<string> Nickname => nickname;
    public IReactiveProperty<LoginPanelState> CurrentPanel => currentPanel;
    public IReactiveCommand<Unit> SignInCommand => signInCommand;
    public IReactiveCommand<Unit> SignUpCommand => signUpCommand;
    public IReactiveCommand<Unit> SetNicknameCommand => setNicknameCommand;
    public IReadOnlyReactiveProperty<string> LogMessage => logMessage;

    #endregion
    #region Private Variable
    event Action <UserData> onSignInSucceedHandler;
    event Action onSignInFailedHandler;
    event Action onSignUpSucceedHandler;
    event Action onSignUpFailedHandler;
    event Action<string> onSetNicknameSucceedHandler;
    event Action onSetNicknameFailedHandler;
    #endregion
    #region Readonly Variable
    readonly ReactiveProperty<string> signInID = new ReactiveProperty<string>();
    readonly ReactiveProperty<string> signInPassword = new ReactiveProperty<string>();

    readonly ReactiveProperty<string> signUpID = new ReactiveProperty<string>();
    readonly ReactiveProperty<string> signUpPassword = new ReactiveProperty<string>();

    readonly ReactiveProperty<string> nickname = new ReactiveProperty<string>();

    readonly ReactiveProperty<string> logMessage = new ReactiveProperty<string>();

    readonly ReactiveProperty<LoginPanelState> currentPanel = new ReactiveProperty<LoginPanelState>(LoginPanelState.SignIn);

    readonly ReactiveCommand signInCommand = new ReactiveCommand();
    readonly ReactiveCommand signUpCommand = new ReactiveCommand();
    readonly ReactiveCommand setNicknameCommand = new ReactiveCommand();

    readonly ILoginService loginService;
    readonly CompositeDisposable disposables = new();
    #endregion

    public void Initialize()
    {
        (loginService as IInitializable)?.Initialize();

        signInCommand.Subscribe(_ => loginService.RequestSignIn(signInID.Value, signInPassword.Value)).AddTo(disposables);
        signUpCommand.Subscribe(_ => loginService.RequestSignUp(signUpID.Value, signUpPassword.Value)).AddTo(disposables);
        setNicknameCommand.Subscribe(_ => loginService.RequestSetNickname(nickname.Value)).AddTo(disposables);

        loginService.OnSignInSucceed += onSignInSucceedHandler;
        loginService.OnSignInFailed += onSignInFailedHandler;
        loginService.OnSignUpSucceed += onSignUpSucceedHandler;
        loginService.OnSignUpFailed += onSignUpFailedHandler;
        loginService.OnSetNicknameSucceed += onSetNicknameSucceedHandler;
        loginService.OnSetNicknameFailed += onSetNicknameFailedHandler;
    }
    public void Dispose()
    {
        disposables.Dispose();
        (loginService as IDisposable)?.Dispose();

        loginService.OnSignInSucceed -= onSignInSucceedHandler;
        loginService.OnSignInFailed -= onSignInFailedHandler;
        loginService.OnSignUpSucceed -= onSignUpSucceedHandler;
        loginService.OnSignUpFailed -= onSignUpFailedHandler;
        loginService.OnSetNicknameSucceed -= onSetNicknameSucceedHandler;
        loginService.OnSetNicknameFailed -= onSetNicknameFailedHandler;
    }

    void HandleSignInSucceed(UserData userData)
    {
        logMessage.Value = "Sign in success";

        if (userData.Nickname == "Unknown")
            CurrentPanel.Value = LoginPanelState.CreateNickname;
        else
            SceneController.Instance.MoveScene("Lobby");
    }
    void HandleSignInFailed() { }
    void HandleSignUpSucceed() 
    {
        logMessage.Value = "Sign Up success";

        CurrentPanel.Value = LoginPanelState.SignIn;
    }
    void HandleSignUpFailed() { }
    void HandleSetNicknameSucceed(string nickname)
    {
        logMessage.Value = $"Set Nickname success: {nickname}";

        SceneController.Instance.MoveScene("Lobby");
    }
    void HandleSetNicknameFailed() { }
}

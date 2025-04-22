using System;
using UniRx;
using Unity.VisualScripting;
using UnityEngine.Events;
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
        onCreateNicknameSucceedHandler = HandleCreateNicknameSucceed;
        onCreateNicknameFailedHandler = HandleCreateNicknameFailed;
    }
    public enum PanelState
    {
        SignIn,
        SignUp,
        CreateNickname
    }

    #region Public Variable
    public IReactiveProperty<string> SignInID => signInID;
    public IReactiveProperty<string> SignInPassword => signInPassword;
    public IReactiveProperty<string> SignUpID => signUpID;
    public IReactiveProperty<string> SignUpPassword => signUpPassword;
    public IReactiveProperty<string> Nickname => nickname;
    public IReactiveProperty<string> LogMessage => logMessage;
    public IReactiveProperty<PanelState> CurrentPanel => currentPanel;
    public IReactiveCommand<Unit> SignInCommand => signInCommand;
    public IReactiveCommand<Unit> SignUpCommand => signUpCommand;
    public IReactiveCommand<Unit> CreateNicknameCommand => createNicknameCommand;
    #endregion
    #region Private Variable
    UnityAction<UserData> onSignInSucceedHandler;
    UnityAction onSignInFailedHandler;
    UnityAction onSignUpSucceedHandler;
    UnityAction onSignUpFailedHandler;
    UnityAction onCreateNicknameSucceedHandler;
    UnityAction onCreateNicknameFailedHandler;
    #endregion
    #region Readonly Variable
    readonly ReactiveProperty<string> signInID = new ReactiveProperty<string>();
    readonly ReactiveProperty<string> signInPassword = new ReactiveProperty<string>();

    readonly ReactiveProperty<string> signUpID = new ReactiveProperty<string>();
    readonly ReactiveProperty<string> signUpPassword = new ReactiveProperty<string>();

    readonly ReactiveProperty<string> nickname = new ReactiveProperty<string>();

    readonly ReactiveProperty<string> logMessage = new ReactiveProperty<string>();

    readonly ReactiveProperty<PanelState> currentPanel = new ReactiveProperty<PanelState>(PanelState.SignIn);

    readonly ReactiveCommand signInCommand = new ReactiveCommand();
    readonly ReactiveCommand signUpCommand = new ReactiveCommand();
    readonly ReactiveCommand createNicknameCommand = new ReactiveCommand();

    readonly ILoginService loginService;
    readonly CompositeDisposable disposables = new();
    #endregion

    public void Initialize()
    {
        (loginService as IInitializable)?.Initialize();

        signInCommand.Subscribe(_ => loginService.RequestSignIn(signInID.Value, signInPassword.Value)).AddTo(disposables);
        signUpCommand.Subscribe(_ => loginService.RequestSignUp(signUpID.Value, signUpPassword.Value)).AddTo(disposables);
        createNicknameCommand.Subscribe(_ => loginService.RequestCreateNickname(nickname.Value)).AddTo(disposables);

        loginService.OnSignInSucceed += onSignInSucceedHandler;
        loginService.OnSignInFailed += onSignInFailedHandler;
        loginService.OnSignUpSucceed += onSignUpSucceedHandler;
        loginService.OnSignUpFailed += onSignUpFailedHandler;
        loginService.OnCreateNicknameSucceed += onCreateNicknameSucceedHandler;
        loginService.OnCreateNicknameFailed += onCreateNicknameFailedHandler;
    }
    public void Dispose()
    {
        disposables.Dispose();
        (loginService as IDisposable)?.Dispose();

        loginService.OnSignInSucceed -= onSignInSucceedHandler;
        loginService.OnSignInFailed -= onSignInFailedHandler;
        loginService.OnSignUpSucceed -= onSignUpSucceedHandler;
        loginService.OnSignUpFailed -= onSignUpFailedHandler;
        loginService.OnCreateNicknameSucceed -= onCreateNicknameSucceedHandler;
        loginService.OnCreateNicknameFailed -= onCreateNicknameFailedHandler;
    }

    void HandleSignInSucceed(UserData userData)
    {
        LogMessage.Value = "Sign in success";

        if (userData.Nickname == "Unknown")
            CurrentPanel.Value = PanelState.CreateNickname;
        else
            SceneController.Instance.MoveScene("Lobby");
    }
    void HandleSignInFailed() { }
    void HandleSignUpSucceed() 
    {
        LogMessage.Value = "Sign Up success";

        CurrentPanel.Value = PanelState.SignIn;
    }
    void HandleSignUpFailed() { }
    void HandleCreateNicknameSucceed()
    {
        LogMessage.Value = "Create Nickname success";

        SceneController.Instance.MoveScene("Lobby");
    }
    void HandleCreateNicknameFailed() { }
}

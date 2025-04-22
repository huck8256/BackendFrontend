using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UniRx;
using Cysharp.Threading.Tasks;
public class LoginView : MonoBehaviour
{
    [SerializeField] TMP_Text log;

    [Header("Sign In")]
    [SerializeField] GameObject signIn;
    [SerializeField] TMP_InputField signIn_ID;
    [SerializeField] TMP_InputField signIn_Password;
    [SerializeField] Button signIn_SignUp;
    [SerializeField] Button signIn_SignIn;

    [Header("Sign Up")]
    [SerializeField] GameObject signUp;
    [SerializeField] TMP_InputField signUp_ID;
    [SerializeField] TMP_InputField signUp_Password;
    [SerializeField] Button signUp_SignUp;
    [SerializeField] Button signUp_Back;

    [Header("Create Nickname")]
    [SerializeField] GameObject createNickname;
    [SerializeField] TMP_InputField createNickname_nickName;
    [SerializeField] Button createNickname_Create;

    LoginViewModel loginViewModel;

    #region Unity LifeCycle
    void Awake() => loginViewModel = new LoginViewModel(new LoginModel());
    void OnEnable() => loginViewModel.Initialize();
    void OnDisable() => loginViewModel.Dispose();
    void Start()
    {
        // Binding
        signIn_ID.onValueChanged.AsObservable().Subscribe(x => loginViewModel.SignInID.Value = x).AddTo(this);
        signIn_Password.onValueChanged.AsObservable().Subscribe(x => loginViewModel.SignInPassword.Value = x).AddTo(this);

        signUp_ID.onValueChanged.AsObservable().Subscribe(x => loginViewModel.SignUpID.Value = x).AddTo(this);
        signUp_Password.onValueChanged.AsObservable().Subscribe(x => loginViewModel.SignUpPassword.Value = x).AddTo(this);

        createNickname_nickName.onValueChanged.AsObservable().Subscribe(x => loginViewModel.Nickname.Value = x).AddTo(this);

        signIn_SignIn.OnClickAsObservable().Subscribe(_ => loginViewModel.SignInCommand.Execute(Unit.Default)).AddTo(this);
        signIn_SignUp.OnClickAsObservable().Subscribe(_ => loginViewModel.CurrentPanel.Value = LoginViewModel.PanelState.SignUp).AddTo(this);

        signUp_Back.OnClickAsObservable().Subscribe(_ => loginViewModel.CurrentPanel.Value = LoginViewModel.PanelState.SignIn).AddTo(this);
        signUp_SignUp.OnClickAsObservable().Subscribe(_ => loginViewModel.SignUpCommand.Execute(Unit.Default)).AddTo(this);

        createNickname_Create.OnClickAsObservable().Subscribe(_ => loginViewModel.CreateNicknameCommand.Execute(Unit.Default)).AddTo(this);

        // Log Message
        loginViewModel.LogMessage.Subscribe(msg => log.text = msg).AddTo(this);

        // Panel switching
        loginViewModel.CurrentPanel.Subscribe(state =>
        {
            signIn.SetActive(state == LoginViewModel.PanelState.SignIn);
            signUp.SetActive(state == LoginViewModel.PanelState.SignUp);
            createNickname.SetActive(state == LoginViewModel.PanelState.CreateNickname);
        }).AddTo(this);
    }
    #endregion
}

using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
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
    private void Start()
    {
        #region Sign In
        // SIgn Up 버튼 클릭 시,
        signIn_SignUp.onClick.AddListener(ShowSignUpPanel);
        // Sign In 버튼 클릭 시,
        signIn_SignIn.onClick.AddListener(HandleSignInButtonClick);
        #endregion

        #region Sign Up
        // Back 버튼 클릭 시,
        signUp_Back.onClick.AddListener(ShowSignInPanel);
        signUp_SignUp.onClick.AddListener(HandleSignUpButtonClick);
        #endregion

        #region Create Nickname
        // Create 버튼 클릭 시,
        createNickname_Create.onClick.AddListener(HandleCreateNicknameButtonClick);
        #endregion
    }
    public void HandleSignInSucceed() => log.text = "Sign In Succeeded";
    public void HandleSignInFailed() => log.text = "Sign In Failed";
    public void HandleSignUpSucceed()
    {
        ShowSignInPanel();
        log.text = $"Sign Up Succeeded";
    }
    public void HandleSignUpFailed() => log.text = "Sign Up Failed";
    public void HandleCreateNicknameSucceed() => log.text = "Nickname Created";
    public void HandleCreateNicknameFailed() => log.text = "Nickname Create Failed";
    public void HandleCheckNickname(bool isCheck)
    {
        if(isCheck)
        {
            Debug.Log("Looby 입장");
        }
        else
        {
            Debug.Log("닉네임 생성");
            ShowCreateNickname();
        }
    }
    // 회원가입 창
    void ShowSignUpPanel()
    {
        signUp.SetActive(true);
        signIn.SetActive(false);
    }
    // 로그인 창
    void ShowSignInPanel()
    {
        signIn.SetActive(true);
        signUp.SetActive(false);
    }
    // 닉네임 생성 창
    void ShowCreateNickname()
    {
        createNickname.SetActive(true);
        signIn.SetActive(false);
    }
    void HandleSignUpButtonClick()
    {
        // 입력값 유효성 검사 로직 추가 필요

        string id = signUp_ID.text;
        string password = signUp_Password.text;

        Login.Instance.RequestSignUp(id, password);
    }
    void HandleSignInButtonClick()
    {
        // 입력값 유효성 검사 로직 추가 필요

        string id = signIn_ID.text;
        string password = signIn_Password.text;

        Login.Instance.RequestSignIn(id, password);
    }
    void HandleCreateNicknameButtonClick()
    {
        // 입력값 유효성 검사 로직 추가 필요
        string nickName = createNickname_nickName.text;

        Login.Instance.RequestCreateNickname(nickName);
    }
}

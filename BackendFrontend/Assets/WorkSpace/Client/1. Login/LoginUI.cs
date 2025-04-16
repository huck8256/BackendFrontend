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
        // SIgn Up ��ư Ŭ�� ��,
        signIn_SignUp.onClick.AddListener(ShowSignUpPanel);
        // Sign In ��ư Ŭ�� ��,
        signIn_SignIn.onClick.AddListener(HandleSignInButtonClick);
        #endregion

        #region Sign Up
        // Back ��ư Ŭ�� ��,
        signUp_Back.onClick.AddListener(ShowSignInPanel);
        signUp_SignUp.onClick.AddListener(HandleSignUpButtonClick);
        #endregion

        #region Create Nickname
        // Create ��ư Ŭ�� ��,
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
            Debug.Log("Looby ����");
        }
        else
        {
            Debug.Log("�г��� ����");
            ShowCreateNickname();
        }
    }
    // ȸ������ â
    void ShowSignUpPanel()
    {
        signUp.SetActive(true);
        signIn.SetActive(false);
    }
    // �α��� â
    void ShowSignInPanel()
    {
        signIn.SetActive(true);
        signUp.SetActive(false);
    }
    // �г��� ���� â
    void ShowCreateNickname()
    {
        createNickname.SetActive(true);
        signIn.SetActive(false);
    }
    void HandleSignUpButtonClick()
    {
        // �Է°� ��ȿ�� �˻� ���� �߰� �ʿ�

        string id = signUp_ID.text;
        string password = signUp_Password.text;

        Login.Instance.RequestSignUp(id, password);
    }
    void HandleSignInButtonClick()
    {
        // �Է°� ��ȿ�� �˻� ���� �߰� �ʿ�

        string id = signIn_ID.text;
        string password = signIn_Password.text;

        Login.Instance.RequestSignIn(id, password);
    }
    void HandleCreateNicknameButtonClick()
    {
        // �Է°� ��ȿ�� �˻� ���� �߰� �ʿ�
        string nickName = createNickname_nickName.text;

        Login.Instance.RequestCreateNickname(nickName);
    }
}

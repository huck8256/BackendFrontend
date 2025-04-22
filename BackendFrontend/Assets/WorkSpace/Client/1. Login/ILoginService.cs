using UnityEngine.Events;

public interface ILoginService
{
    event UnityAction<UserData> OnSignInSucceed;
    event UnityAction OnSignInFailed;
    event UnityAction OnSignUpSucceed;
    event UnityAction OnSignUpFailed;
    event UnityAction OnCreateNicknameSucceed;
    event UnityAction OnCreateNicknameFailed;
    void RequestSignIn(string id, string pw);
    void RequestSignUp(string id, string pw);
    void RequestCreateNickname(string nickname);
}
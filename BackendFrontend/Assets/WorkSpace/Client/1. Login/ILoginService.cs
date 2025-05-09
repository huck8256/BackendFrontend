using System;
using Unity.VisualScripting;
public interface ILoginService : IInitializable, IDisposable
{
    event Action<UserData> OnSignInSucceed;
    event Action OnSignInFailed;
    event Action OnSignUpSucceed;
    event Action OnSignUpFailed;
    event Action<string> OnSetNicknameSucceed;
    event Action OnSetNicknameFailed;
    void RequestSignIn(string id, string pw);
    void RequestSignUp(string id, string pw);
    void RequestSetNickname(string nickname);
}
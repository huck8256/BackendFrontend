public class UserSession
{
    public bool IsSignedIn { get; private set; }
    public UserData UserData { get; private set; } = new UserData();
    public void SignIn(UserData data)
    {
        UserData = data;
        IsSignedIn = true;
    }

    public void SignOut()
    {
        UserData = new UserData();
        IsSignedIn = false;
    }

    public void SetNickname(string nickname)
    {
        if (IsSignedIn)
            UserData.Nickname = nickname;
    }
}

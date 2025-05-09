using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class SignInHandler : IPacketHandler
{
    public event Action<UserData> OnSucceed;
    public event Action OnFailed;

    public PacketID ID => PacketID.SignIn;

    public void Handle(Dictionary<string, string> body)
    {
        if (body.TryGetValue("Result", out string result))
        {
            if (result.Equals("Succeed"))
            {
                Debug.Log("로그인 성공");
                if (body.TryGetValue("UserData", out string userDataJson))
                {
                    UserData userData = new UserData();
                    userData = JsonConvert.DeserializeObject<UserData>(userDataJson);
                    OnSucceed?.Invoke(userData);
                }
            }
            else if (result.Equals("Failed"))
            {
                Debug.Log("로그인 실패");
                OnFailed?.Invoke();
            }
        }
    }
}

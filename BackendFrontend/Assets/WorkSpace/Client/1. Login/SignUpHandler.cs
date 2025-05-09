using System;
using System.Collections.Generic;
using UnityEngine;

public class SignUpHandler : IPacketHandler
{
    public event Action OnSucceed;
    public event Action OnFailed;

    public PacketID ID => PacketID.SignUp;

    public void Handle(Dictionary<string, string> body)
    {
        if (body.TryGetValue("Result", out string result))
        {
            if (result.Equals("Succeed"))
            {
                Debug.Log("가입 성공");
                OnSucceed?.Invoke();
            }
            else if (result.Equals("Failed"))
            {
                Debug.Log("가입 실패");
                OnFailed?.Invoke();
            }
        }
    }
}

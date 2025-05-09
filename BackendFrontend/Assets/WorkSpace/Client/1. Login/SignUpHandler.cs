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
                Debug.Log("���� ����");
                OnSucceed?.Invoke();
            }
            else if (result.Equals("Failed"))
            {
                Debug.Log("���� ����");
                OnFailed?.Invoke();
            }
        }
    }
}

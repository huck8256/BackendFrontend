using System;
using System.Collections.Generic;
using UnityEngine;

public class SetNicknameHandler : IPacketHandler
{
    
    public event Action<string> OnSucceed;
    public event Action OnFailed;
    public PacketID ID => PacketID.SetNickname;

    public void Handle(Dictionary<string, string> body)
    {
        if (body.TryGetValue("Result", out string result))
        {
            if (result.Equals("Succeed") && body.TryGetValue("Nickname", out string nickname))
            {
                Debug.Log("닉네임 만들기 성공");
                OnSucceed?.Invoke(nickname);
            }
            else if (result.Equals("Failed"))
            {
                Debug.Log("닉네임 만들기 실패");
                OnFailed?.Invoke();
            }
        }
    }
}

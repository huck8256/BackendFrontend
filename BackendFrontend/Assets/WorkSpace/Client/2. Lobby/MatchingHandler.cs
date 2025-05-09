using System;
using System.Collections.Generic;

public class MatchingHandler : IPacketHandler
{
    public event Action OnRequested;
    public event Action OnCanceled;
    public event Action OnFound;
    public event Action<List<string>> OnSucceed;
    public event Action OnFailed;

    public PacketID ID => PacketID.Matching;

    public void Handle(Dictionary<string, string> body)
    {
        if (body.TryGetValue("Result", out string result))
        {
            if (result.Equals("Requested"))
            {
                OnRequested?.Invoke();
            }
            else if (result.Equals("Canceled"))
            {
                OnCanceled?.Invoke();
            }
            else if (result.Equals("Found"))
                OnFound?.Invoke();
            else if (result.Equals("Succeed"))
            {
                if (body.TryGetValue("ClientCount", out string count))
                {
                    // 매칭된 클라이언트 수
                    int _count = int.Parse(count);

                    // 클라이언트 IPEndPoint 담을 리스트
                    List<string> _matchedClientList = new List<string>();

                    // List에 IPEndPoint담기
                    for (int i = 0; i < _count; i++)
                    {
                        if (body.TryGetValue($"Client{i}", out string iPEndPoint))
                        {
                            _matchedClientList.Add(iPEndPoint);
                        }
                    }

                    // 이벤트 호출
                    OnSucceed?.Invoke(_matchedClientList);
                }
            }
            else if (result.Equals("Failed"))
                OnFailed?.Invoke();
        }
    }
}

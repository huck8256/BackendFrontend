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
                    // ��Ī�� Ŭ���̾�Ʈ ��
                    int _count = int.Parse(count);

                    // Ŭ���̾�Ʈ IPEndPoint ���� ����Ʈ
                    List<string> _matchedClientList = new List<string>();

                    // List�� IPEndPoint���
                    for (int i = 0; i < _count; i++)
                    {
                        if (body.TryGetValue($"Client{i}", out string iPEndPoint))
                        {
                            _matchedClientList.Add(iPEndPoint);
                        }
                    }

                    // �̺�Ʈ ȣ��
                    OnSucceed?.Invoke(_matchedClientList);
                }
            }
            else if (result.Equals("Failed"))
                OnFailed?.Invoke();
        }
    }
}

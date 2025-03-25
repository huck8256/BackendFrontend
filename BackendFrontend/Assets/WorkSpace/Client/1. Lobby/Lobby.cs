using UnityEngine;
using UniRx;
using System.Collections.Generic;
public class Lobby : SingletonMonoBehaviour<Lobby>
{
    public IReadOnlyReactiveProperty<bool> IsMatching => isMatching;
    ReactiveProperty<bool> isMatching = new ReactiveProperty<bool>();

    private void Start()
    {
        TCPClient.Instance.OnMatchFoundEvent.AddListener(MatchFound);
        TCPClient.Instance.OnMatchSucceedEvent.AddListener(MatchSucceed);
        TCPClient.Instance.OnMatchFailedEvent.AddListener(MatchFailed);
    }
    private void OnDestroy()
    {
        TCPClient.Instance.OnMatchFoundEvent.RemoveListener(MatchFound);
        TCPClient.Instance.OnMatchSucceedEvent.RemoveListener(MatchSucceed);
        TCPClient.Instance.OnMatchFailedEvent.RemoveListener(MatchFailed);
    }
    // ��ġ ��û
    public void MatchRequest()
    {
        isMatching.Value = true;
        TCPClient.Instance.OnMatchRequestedEvent.Invoke();
    }
    // ��ġ ��û ���
    public void MatchRequestCancel()
    {
        isMatching.Value = false;
        TCPClient.Instance.OnMatchRequestCanceledEvent.Invoke();
    }
    // ��Ī ��� ã��
    private void MatchFound()
    {
        Debug.Log("Match Found");
    }
    // ��Ī �Ϸ�
    private void MatchSucceed(List<string> matchedClient)
    {
        SceneController.Instance.MoveScene("Game");
    }
    // ��Ī ����
    private void MatchFailed()
    {
    }
}

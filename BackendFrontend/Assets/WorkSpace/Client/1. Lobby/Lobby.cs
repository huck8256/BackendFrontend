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
    // 매치 요청
    public void MatchRequest()
    {
        isMatching.Value = true;
        TCPClient.Instance.OnMatchRequestedEvent.Invoke();
    }
    // 매치 요청 취소
    public void MatchRequestCancel()
    {
        isMatching.Value = false;
        TCPClient.Instance.OnMatchRequestCanceledEvent.Invoke();
    }
    // 매칭 상대 찾음
    private void MatchFound()
    {
        Debug.Log("Match Found");
    }
    // 매칭 완료
    private void MatchSucceed(List<string> matchedClient)
    {
        SceneController.Instance.MoveScene("Game");
    }
    // 매칭 실패
    private void MatchFailed()
    {
    }
}

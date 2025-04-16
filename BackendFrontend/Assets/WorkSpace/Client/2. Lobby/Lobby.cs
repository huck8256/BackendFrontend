using UnityEngine;
using UniRx;
using System.Collections.Generic;
public class Lobby : SingletonMonoBehaviour<Lobby>
{
    [SerializeField] LobbyUI lobbyUI;
    public IReadOnlyReactiveProperty<bool> IsMatching => isMatching;
    ReactiveProperty<bool> isMatching = new ReactiveProperty<bool>();

    private void Start()
    {
        isMatching.Subscribe(value =>
        {
            if (value)
                TCPClient.Instance.SendMatchRequestMessage();
            else
                TCPClient.Instance.SendMatchCancelRequestMessage();
        }).AddTo(this);
    }
    private void OnEnable()
    {
        TCPClient.OnMatchFoundEvent += HandleMatchFound;
        TCPClient.OnMatchSucceedEvent += HandleMatchSucceed;
        TCPClient.OnMatchFailedEvent += HandleMatchFailed;
    }
    private void OnDisable()
    {
        TCPClient.OnMatchFoundEvent -= HandleMatchFound;
        TCPClient.OnMatchSucceedEvent -= HandleMatchSucceed;
        TCPClient.OnMatchFailedEvent -= HandleMatchFailed;
    }
    // 매치 요청
    public void MatchRequest() => isMatching.Value = true;
    // 매치 요청 취소
    public void MatchCancelRequest() => isMatching.Value = false;
    // 매칭 상대 찾음
    private void HandleMatchFound() => lobbyUI.HandleMatchFound();
    // 매칭 완료
    private void HandleMatchSucceed(List<string> matchedClient)
    {
        lobbyUI.HandleMatchSucceed(matchedClient);

        SceneController.Instance.MoveScene("Game");
    }
    // 매칭 실패
    private void HandleMatchFailed() => lobbyUI.HandleMatchFailed();
}

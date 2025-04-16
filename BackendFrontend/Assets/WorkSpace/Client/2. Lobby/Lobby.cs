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
    // ��ġ ��û
    public void MatchRequest() => isMatching.Value = true;
    // ��ġ ��û ���
    public void MatchCancelRequest() => isMatching.Value = false;
    // ��Ī ��� ã��
    private void HandleMatchFound() => lobbyUI.HandleMatchFound();
    // ��Ī �Ϸ�
    private void HandleMatchSucceed(List<string> matchedClient)
    {
        lobbyUI.HandleMatchSucceed(matchedClient);

        SceneController.Instance.MoveScene("Game");
    }
    // ��Ī ����
    private void HandleMatchFailed() => lobbyUI.HandleMatchFailed();
}

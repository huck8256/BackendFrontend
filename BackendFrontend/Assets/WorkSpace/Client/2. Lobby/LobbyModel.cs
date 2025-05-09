using System.Collections.Generic;
using System;
public class LobbyModel : IMatchService
{
    public LobbyModel(MatchingHandler matchingHandler) { this.matchingHandler = matchingHandler; }
    #region Public Variable
    public event Action OnMatchRequested;
    public event Action OnMatchCanceled;
    public event Action OnMatchFound;
    public event Action<List<string>> OnMatchSucceed;
    public event Action OnMatchFailed;
    #endregion

    MatchingHandler matchingHandler;
    public void Initialize()
    {
        matchingHandler.OnRequested += HandleMatchRequested;
        matchingHandler.OnCanceled += HandleMatchCanceled;
        matchingHandler.OnFound += HandleMatchFound;
        matchingHandler.OnSucceed += HandleMatchSucceed;
        matchingHandler.OnFailed += HandleMatchFailed;
    }

    public void Dispose()
    {
        matchingHandler.OnRequested -= HandleMatchRequested;
        matchingHandler.OnCanceled -= HandleMatchCanceled;
        matchingHandler.OnFound -= HandleMatchFound;
        matchingHandler.OnSucceed -= HandleMatchSucceed;
        matchingHandler.OnFailed -= HandleMatchFailed;
    }
    // ��ġ ��û
    public void RequestMatch() => NetworkManager.Instance?.SendMatchRequestMessage();
    // ��ġ ��û ���
    public void RequestMatchCancel() => NetworkManager.Instance?.SendMatchCancelRequestMessage();
    // ��Ī ã�� ��
    private void HandleMatchRequested() => OnMatchRequested?.Invoke();
    // ��Ī �� ã�� ��
    private void HandleMatchCanceled() => OnMatchCanceled?.Invoke();
    // ��Ī ��� ã��
    private void HandleMatchFound() => OnMatchFound?.Invoke();
    // ��Ī �Ϸ�
    private void HandleMatchSucceed(List<string> matchedClient) => OnMatchSucceed?.Invoke(matchedClient);
    // ��Ī ����
    private void HandleMatchFailed() => OnMatchFailed?.Invoke();
}

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
    // 매치 요청
    public void RequestMatch() => NetworkManager.Instance?.SendMatchRequestMessage();
    // 매치 요청 취소
    public void RequestMatchCancel() => NetworkManager.Instance?.SendMatchCancelRequestMessage();
    // 매칭 찾는 중
    private void HandleMatchRequested() => OnMatchRequested?.Invoke();
    // 매칭 안 찾는 중
    private void HandleMatchCanceled() => OnMatchCanceled?.Invoke();
    // 매칭 상대 찾음
    private void HandleMatchFound() => OnMatchFound?.Invoke();
    // 매칭 완료
    private void HandleMatchSucceed(List<string> matchedClient) => OnMatchSucceed?.Invoke(matchedClient);
    // 매칭 실패
    private void HandleMatchFailed() => OnMatchFailed?.Invoke();
}

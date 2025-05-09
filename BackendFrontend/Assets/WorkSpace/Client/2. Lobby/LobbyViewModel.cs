using System;
using System.Collections.Generic;
using UniRx;
using Unity.VisualScripting;
using Unit = UniRx.Unit;
public class LobbyViewModel : IInitializable, IDisposable
{
    public LobbyViewModel(IMatchService matchService) 
    {
        this.matchService = matchService;

        onMatchRequestedHandler = HandleMatchRequested;
        onMatchCanceledHandler = HandleMatchCanceled;
        onMatchFoundHandler = HandleMatchFound;
        onMatchSucceedHandler = HandleMatchSucceed;
        onMatchFailedHandler = HandleMatchFailed;
    }
    public void Initialize()
    {
        (matchService as IInitializable)?.Initialize();

        matchRequestCommand.Subscribe(_ => matchService.RequestMatch()).AddTo(disposables);
        matchCancelCommand.Subscribe(_ => matchService.RequestMatchCancel()).AddTo(disposables);

        matchService.OnMatchRequested += onMatchRequestedHandler;
        matchService.OnMatchCanceled += onMatchCanceledHandler;
        matchService.OnMatchFound += onMatchFoundHandler;
        matchService.OnMatchSucceed += onMatchSucceedHandler;
        matchService.OnMatchFailed += onMatchFailedHandler;
    }

    public void Dispose()
    {
        disposables.Dispose();
        (matchService as IDisposable)?.Dispose();

        matchService.OnMatchRequested -= onMatchRequestedHandler;
        matchService.OnMatchCanceled -= onMatchCanceledHandler;
        matchService.OnMatchFound -= onMatchFoundHandler;
        matchService.OnMatchSucceed -= onMatchSucceedHandler;
        matchService.OnMatchFailed -= onMatchFailedHandler;
    }

    #region Public Variable
    public IReactiveProperty<MatchingPanelState> CurrentPanel => currentPanel;
    public IReactiveCommand<Unit> MatchRequestCommand => matchRequestCommand;
    public IReactiveCommand<Unit> MatchCancelCommand => matchCancelCommand;
    public IReadOnlyReactiveProperty<string> LogMessage => logMessage;
    #endregion

    #region Private Variable
    Action onMatchRequestedHandler;
    Action onMatchCanceledHandler;
    Action onMatchFoundHandler;
    Action<List<string>> onMatchSucceedHandler;
    Action onMatchFailedHandler;

    readonly ReactiveProperty<string> logMessage = new ReactiveProperty<string>();

    readonly ReactiveProperty<MatchingPanelState> currentPanel = new ReactiveProperty<MatchingPanelState>(MatchingPanelState.Normal);

    readonly ReactiveCommand matchRequestCommand = new ReactiveCommand();
    readonly ReactiveCommand matchCancelCommand = new ReactiveCommand();

    readonly IMatchService matchService;
    readonly CompositeDisposable disposables = new();
    #endregion
    void HandleMatchRequested() => CurrentPanel.Value = MatchingPanelState.Matching;
    void HandleMatchCanceled() => CurrentPanel.Value = MatchingPanelState.Normal;
    void HandleMatchFound() => CurrentPanel.Value = MatchingPanelState.MatchFound;
    void HandleMatchSucceed(List<string> matchedClient) => CurrentPanel.Value = MatchingPanelState.MatchSucceed;
    void HandleMatchFailed() => CurrentPanel.Value = MatchingPanelState.Normal;
}

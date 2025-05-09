using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class LobbyView : MonoBehaviour
{
    [SerializeField] TMP_Text log;
    [SerializeField] Button button_RequestMatch;
    [SerializeField] Button button_RequestMatchCancel;

    LobbyViewModel lobbyViewModel;
    void Awake() => lobbyViewModel = new LobbyViewModel(new LobbyModel(NetworkManager.Instance?.MatchingHandler));
    void OnEnable() => lobbyViewModel?.Initialize();
    void Start()
    {
        // Binding
        button_RequestMatch.OnClickAsObservable()
            .Where(_ => lobbyViewModel.CurrentPanel.Value == MatchingPanelState.Normal)
            .Subscribe(_ => lobbyViewModel.MatchRequestCommand.Execute(Unit.Default))
            .AddTo(this);
        button_RequestMatchCancel.OnClickAsObservable()
            .Where(_ => lobbyViewModel.CurrentPanel.Value == MatchingPanelState.Matching)
            .Subscribe(_ => lobbyViewModel.MatchCancelCommand.Execute(Unit.Default))
            .AddTo(this);

        // Log Message
        lobbyViewModel.LogMessage.Subscribe(msg => log.text = msg).AddTo(this);

        // Panel switching
        lobbyViewModel.CurrentPanel.Subscribe(state =>
        {
            button_RequestMatch.gameObject.SetActive(state == MatchingPanelState.Normal);
            button_RequestMatchCancel.gameObject.SetActive(state == MatchingPanelState.Matching);
        }).AddTo(this);
    }
    void OnDisable() => lobbyViewModel?.Dispose();
}

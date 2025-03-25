using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Collections.Generic;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] TMP_Text log;
    [SerializeField] Button button_Start;
    [SerializeField] TMP_Text button_Start_Text;

    private void Start()
    {
        button_Start.onClick.AddListener(() =>
        {
            if(Lobby.Instance.IsMatching.Value)
                Lobby.Instance.MatchRequestCancel();
            else
                Lobby.Instance.MatchRequest();
        });

        Lobby.Instance.IsMatching.Subscribe(isMatching =>
        {
            button_Start_Text.text = isMatching ? "Cancel" : "Start";
        }).AddTo(this);

        TCPClient.Instance.OnMatchFoundEvent.AddListener(MatchFound);
        TCPClient.Instance.OnMatchRequestedEvent.AddListener(MatchRequest);
        TCPClient.Instance.OnMatchRequestCanceledEvent.AddListener(MatchRequestCancel);
        TCPClient.Instance.OnMatchSucceedEvent.AddListener(MatchSucceed);
        TCPClient.Instance.OnMatchFailedEvent.AddListener(MatchFailed);
    }
    // 매치 요청
    public void MatchRequest()
    {
        log.text = "Serching...";
    }
    // 매치 요청 취소
    public void MatchRequestCancel()
    {
        log.text = "";
    }
    // 매칭 상대 찾음
    private void MatchFound()
    {
        button_Start.interactable = false;
        log.text = "Connecting...";
    }
    // 매칭 완료
    private void MatchSucceed(List<string> matchedClient)
    {
        log.text = "Match Succeed";
    }
    // 매칭 실패
    private void MatchFailed()
    {
        button_Start.interactable = true;
        log.text = "Match Failed";
    }
    
    private void OnDestroy()
    {
        TCPClient.Instance.OnMatchFoundEvent.RemoveListener(MatchFound);
        TCPClient.Instance.OnMatchRequestedEvent.RemoveListener(MatchRequest);
        TCPClient.Instance.OnMatchRequestCanceledEvent.RemoveListener(MatchRequestCancel);
        TCPClient.Instance.OnMatchSucceedEvent.RemoveListener(MatchSucceed);
        TCPClient.Instance.OnMatchFailedEvent.RemoveListener(MatchFailed);
    }
}

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
            if (Lobby.Instance.IsMatching.Value)
                MatchCancelRequest();
            else
                MatchRequest();
        });

        Lobby.Instance.IsMatching.Subscribe(isMatching => button_Start_Text.text = isMatching ? "Cancel" : "Start").AddTo(this);
    }
    // 매칭 상대 찾음
    public void HandleMatchFound()
    {
        button_Start.interactable = false;
        log.text = "Connecting...";
    }
    // 매칭 완료
    public void HandleMatchSucceed(List<string> matchedClient) => log.text = "Match Succeed";
    // 매칭 실패
    public void HandleMatchFailed()
    {
        button_Start.interactable = true;
        log.text = "Match Failed";
    }
    // 매치 요청
    private void MatchRequest()
    {
        Lobby.Instance.MatchRequest();
        log.text = "Serching...";
    }
    // 매치 요청 취소
    private void MatchCancelRequest()
    {
        Lobby.Instance.MatchCancelRequest();
        log.text = "";
    }
}

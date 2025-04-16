using UnityEngine;

public class Game : SingletonMonoBehaviour<Game>
{
    [SerializeField] GameObject playerPrefab;

    public GameObject[] players { get; private set; }

    public bool isPlay { get; private set; }

    public void Start()
    {
        SpawnPlayer();
    }
    public void SpawnPlayer()
    {
        players = new GameObject[UDPClient.Instance.MatchedClient.Count];

        for (int i = 0; i < UDPClient.Instance.MatchedClient.Count; i++)
        {
            players[i] = Instantiate(playerPrefab);
            players[i].name = $"Player {i.ToString()}";
            players[i].GetComponent<PlayerController>().Order = i;
        }

        isPlay = true;
    }
}

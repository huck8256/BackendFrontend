using System;
using UnityEngine;

public class Login : SingletonMonoBehaviour<Login>
{
    private void Start()
    {
        TCPClient.Instance.OnServerConnectedEvent.AddListener(OnServerConnected);
        TCPClient.Instance.OnServerDisconnectedEvent.AddListener(OnServerDisconnected);
    }
    private void OnDestroy()
    {
        TCPClient.Instance.OnServerConnectedEvent.RemoveListener(OnServerConnected);
        TCPClient.Instance.OnServerDisconnectedEvent.RemoveListener(OnServerDisconnected);
    }
    private void OnServerConnected()
    {
        SceneController.Instance.MoveScene("Lobby");
    }
    private void OnServerDisconnected()
    {

    }
}

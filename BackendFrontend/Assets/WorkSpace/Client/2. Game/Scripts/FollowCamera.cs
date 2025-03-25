using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Vector3 offset = new Vector3(0, 5, -5); // 카메라 위치 오프셋

    void LateUpdate()
    {
        if(Game.Instance.isPlay)
        {
            transform.position = Game.Instance.players[UDPClient.Instance.Order].transform.position + offset;
            transform.LookAt(Game.Instance.players[UDPClient.Instance.Order].transform.position);
        }
    }
}

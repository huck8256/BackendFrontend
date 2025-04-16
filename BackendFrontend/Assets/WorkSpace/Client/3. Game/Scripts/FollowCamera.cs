using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Vector3 offset = new Vector3(0, 5, -5); // ī�޶� ��ġ ������

    void LateUpdate()
    {
        if(Game.Instance.isPlay)
        {
            transform.position = Game.Instance.players[UDPClient.Instance.Order].transform.position + offset;
            transform.LookAt(Game.Instance.players[UDPClient.Instance.Order].transform.position);
        }
    }
}

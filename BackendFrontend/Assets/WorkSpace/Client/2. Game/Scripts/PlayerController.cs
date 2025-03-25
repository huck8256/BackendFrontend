using UnityEngine;
public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] bool isMine;
    public int Order { get => order; set => order = value; }

    int order;  // 몇 번째 오브젝트인지 확인

    private Vector2 lastInput = Vector2.zero;
    Vector2 inputVector = Vector2.zero;

    private void Start()
    {
        if (order == UDPClient.Instance.Order)
            isMine = true;
        else
            isMine = false;
    }
    void Update()
    {
        if(isMine)
        {
            Vector2 _inputVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            // 입력이 변경되었을 때만 전송
            if (_inputVector != lastInput)
            {
                // Host Client가 아닐 시,
                if (UDPClient.Instance.Order != UDPClient.Instance.HostOrder)
                    _ = UDPClient.Instance.SendMessageAsync($"{UDPClient.Instance.Order},{_inputVector.x},{_inputVector.y}", UDPClient.Instance.MatchedClient[UDPClient.Instance.HostOrder]);    // 호스트에게 입력값 전달
                else
                    NewInput(_inputVector);

                lastInput = _inputVector;
            }
        }
    }
    private void FixedUpdate()
    {
        // 호스트일 경우
        if (UDPClient.Instance.Order == UDPClient.Instance.HostOrder)
        {
            transform.Translate(new Vector3(inputVector.x, 0f, inputVector.y) * moveSpeed * Time.deltaTime);
            UDPClient.Instance.BroadcastMessage_Position(order, transform.position);
        }
    }
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }
    public void NewInput(Vector2 input)
    {
        inputVector = input;
    }
}

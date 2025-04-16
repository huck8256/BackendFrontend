using UnityEngine;
public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] bool isMine;
    public int Order { get => order; set => order = value; }

    int order;  // �� ��° ������Ʈ���� Ȯ��

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

            // �Է��� ����Ǿ��� ���� ����
            if (_inputVector != lastInput)
            {
                // Host Client�� �ƴ� ��,
                if (UDPClient.Instance.Order != UDPClient.Instance.HostOrder)
                    _ = UDPClient.Instance.SendMessageAsync($"{UDPClient.Instance.Order},{_inputVector.x},{_inputVector.y}", UDPClient.Instance.MatchedClient[UDPClient.Instance.HostOrder]);    // ȣ��Ʈ���� �Է°� ����
                else
                    NewInput(_inputVector);

                lastInput = _inputVector;
            }
        }
    }
    private void FixedUpdate()
    {
        // ȣ��Ʈ�� ���
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

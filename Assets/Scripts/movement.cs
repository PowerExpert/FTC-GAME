using UnityEngine;

public class movement : MonoBehaviour
{
    private Vector3 Velocity;
    private Vector3 PlayerMovementInput;

    public string joystickRightAxisXName;

    public string joystickLeftAxisYName;

    [Header("Components Needed")]
    [SerializeField] private CharacterController Controller;

    [Header("Movement")]
    [SerializeField] private float Speed = 6f;
    [SerializeField] private float Gravity = 9.81f;

    [Header("Rotation")]
    [SerializeField] private float RotationSpeed = 120f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float keyboardX = Input.GetAxisRaw("Horizontal");
        float keyboardZ = Input.GetAxisRaw("Vertical");

        float joystickZ = Input.GetAxisRaw(joystickLeftAxisYName);

        Vector3 input = new Vector3(
            0f,
            0f,
            keyboardZ + joystickZ
        );

        PlayerMovementInput = Vector3.ClampMagnitude(input, 1f);

        RotatePlayer();
        MovePlayer();
    }

    private void MovePlayer()
    {
        Vector3 move = transform.TransformDirection(PlayerMovementInput);

        if (Controller.isGrounded && Velocity.y < 0f)
            Velocity.y = -2f;
        else
            Velocity.y -= Gravity * Time.deltaTime;

        Controller.Move(move * Speed * Time.deltaTime);
        Controller.Move(Velocity * Time.deltaTime);
    }

    private void RotatePlayer()
    {
        float rotation = Input.GetAxis(joystickRightAxisXName);

        if (Input.GetKey(KeyCode.O))
        {
            rotation = -1f;
        }
        else if (Input.GetKey(KeyCode.P))
        {
            rotation = 1f;
        }

        transform.Rotate(0f, rotation * RotationSpeed * Time.deltaTime, 0f, Space.Self);
    }
}

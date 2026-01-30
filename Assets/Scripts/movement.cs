using UnityEngine;

public class movement : MonoBehaviour
{
    private Vector3 Velocity;
    private Vector3 PlayerMovementInput;

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
        PlayerMovementInput = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0f,
            Input.GetAxisRaw("Vertical")
        );

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
        float rotation = 0f;

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

using UnityEngine;

public class movementVR : MonoBehaviour
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

    [Header("VR")]
    [SerializeField] private Transform VRCamera;

    public string VR_JLX;
    public string VR_JLY;
    public string VR_JRX;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float moveX = Input.GetAxis(VR_JLX);
        float moveZ = Input.GetAxis(VR_JLY);

        Vector3 input = new Vector3(moveX, 0f, moveZ);
        PlayerMovementInput = Vector3.ClampMagnitude(input, 1f);

        RotatePlayerVR();
        MovePlayerVR();
    }

    private void MovePlayerVR()
    {
        Vector3 move = transform.TransformDirection(PlayerMovementInput);

        if (Controller.isGrounded && Velocity.y < 0f)
            Velocity.y = -2f;
        else
            Velocity.y -= Gravity * Time.deltaTime;

        Controller.Move(move * Speed * Time.deltaTime);
        Controller.Move(Velocity * Time.deltaTime);
    }

    private void RotatePlayerVR()
    {
        float rotation = Input.GetAxis(VR_JRX);
        transform.Rotate(0f, rotation * RotationSpeed * Time.deltaTime, 0f);
    }
}

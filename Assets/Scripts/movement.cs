using UnityEngine;
using UnityEngine.InputSystem;


//DONT TOUCH THIS SCRIPT, IT IS FOR TESTING PURPOSES ONLY. IT IS NOT USED IN THE FINAL GAME
/*
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

    private PlayerInput _playerInput;
    private InputDevice _playerDevice;

    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (_playerInput != null && _playerInput.devices.Count > 0)
            _playerDevice = _playerInput.devices[0];
    }

    void Update()
    {
        Vector2 moveInput = Vector2.zero;
        float rotateInput = 0f;

        if (_playerInput != null)
        {
            InputAction moveAction = _playerInput.actions["Move"];
            InputAction lookAction = _playerInput.actions["Look"];

            if (moveAction != null)
            {
                moveInput = _playerDevice != null
                    ? moveAction.ReadValue<Vector2>(_playerDevice)
                    : moveAction.ReadValue<Vector2>();
            }

            if (lookAction != null)
            {
                rotateInput = _playerDevice != null
                    ? lookAction.ReadValue<Vector2>(_playerDevice).x
                    : lookAction.ReadValue<Vector2>().x;
            }
        }

        if (Input.GetKey(KeyCode.O))
            rotateInput = -1f;
        else if (Input.GetKey(KeyCode.P))
            rotateInput = 1f;

        Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y);

        PlayerMovementInput = Vector3.ClampMagnitude(input, 1f);

        RotatePlayer(rotateInput);
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

    private void RotatePlayer(float rotation)
    {
        transform.Rotate(0f, rotation * RotationSpeed * Time.deltaTime, 0f, Space.Self);
    }
}
*/
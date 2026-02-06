using UnityEngine;
using UnityEngine.InputSystem;

public class moveTEST : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController controller;

    [Header("Movement")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float gravity = 9.81f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 120f;

    private Vector3 velocity;
    private Vector2 moveInput;
    private Vector2 lookInput;

    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        moveInput = playerInput.actions["Move"].ReadValue<Vector2>();
        lookInput = playerInput.actions["Look"].ReadValue<Vector2>();

        MovePlayer();
        RotatePlayer();
    }

    private void MovePlayer()
    {
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        move = transform.TransformDirection(move);

        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;
        else
            velocity.y -= gravity * Time.deltaTime;

        controller.Move(move * speed * Time.deltaTime);
        controller.Move(velocity * Time.deltaTime);
    }

    private void RotatePlayer()
    {
        float rotationY = lookInput.x * rotationSpeed * Time.deltaTime;
        transform.Rotate(0f, rotationY, 0f);
    }
}

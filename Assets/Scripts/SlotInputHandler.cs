using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attach this to the same GameObject as PlayerInput (the spawned player object).
/// It reads "Navigate" / "Move" / "Submit" actions and forwards them to the
/// PlayerSlotUI that this player is assigned to.
///
/// Unity's PlayerInputManager spawns this with the player prefab, so the
/// CharacterSelectionManager should assign the slot reference after spawning.
/// </summary>
public class SlotInputHandler : MonoBehaviour
{
    // Set by CharacterSelectionManager after the player joins
    [HideInInspector] public PlayerSlotUI assignedSlot;

    private PlayerInput playerInput;

    // Edge-detect for joystick so one hold = one step
    private float navCooldown = 0f;
    private const float NAV_COOLDOWN_TIME = 0.25f;
    private float prevNavX = 0f;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        if (playerInput == null) return;

        // Subscribe to Submit (A / Cross / Enter) for ready toggle
        var submit = playerInput.actions["Submit"];
        if (submit != null) submit.performed += OnSubmit;

        // Subscribe to Cancel (B / Circle) for unready
        var cancel = playerInput.actions["Cancel"];
        if (cancel != null) cancel.performed += OnCancel;
    }

    private void OnDisable()
    {
        if (playerInput == null) return;
        var submit = playerInput.actions["Submit"];
        if (submit != null) submit.performed -= OnSubmit;
        var cancel = playerInput.actions["Cancel"];
        if (cancel != null) cancel.performed -= OnCancel;
    }

    private void Update()
    {
        if (assignedSlot == null || playerInput == null) return;

        navCooldown -= Time.deltaTime;

        // Read left stick or D-pad horizontal from "Move" action
        var moveAction = playerInput.actions["Move"];
        float navX = (moveAction != null) ? moveAction.ReadValue<Vector2>().x : 0f;

        // Navigate left
        if (navX < -0.5f && prevNavX >= -0.5f && navCooldown <= 0f)
        {
            assignedSlot.OnPreviousCharacter();
            navCooldown = NAV_COOLDOWN_TIME;
        }
        // Navigate right
        else if (navX > 0.5f && prevNavX <= 0.5f && navCooldown <= 0f)
        {
            assignedSlot.OnNextCharacter();
            navCooldown = NAV_COOLDOWN_TIME;
        }

        prevNavX = navX;
    }

    private void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (assignedSlot != null)
            assignedSlot.OnReadyToggle();
    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        // Un-ready if already ready
        if (assignedSlot != null && assignedSlot.IsReady())
            assignedSlot.OnReadyToggle();
    }
}

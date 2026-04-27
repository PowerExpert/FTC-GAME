using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles character selection input during lobby phase.
/// Must be on player prefab alongside PlayerInput.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class SlotInputHandler : MonoBehaviour
{
    public PlayerSlotUI AssignedSlot { get; set; }

    private PlayerInput _playerInput;
    private InputAction _navigateAction;
    private InputAction _submitAction;
    private InputAction _cancelAction;

    private float _navTimer;
    private float _prevNavX;
    private const float NavCooldown = 0.25f;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;

        // Disable by default - only enabled during lobby
        enabled = false;
    }

    private void OnEnable()
    {
        if (_playerInput == null) return;

        // Find actions from UI action map
        var uiMap = _playerInput.actions.FindActionMap("UI");
        if (uiMap != null) uiMap.Enable();

        _navigateAction = _playerInput.actions.FindAction("Navigate");
        _submitAction = _playerInput.actions.FindAction("Submit");
        _cancelAction = _playerInput.actions.FindAction("Cancel");

        if (_navigateAction != null) _navigateAction.performed += OnNavigate;
        if (_submitAction != null) _submitAction.performed += OnSubmit;
        if (_cancelAction != null) _cancelAction.performed += OnCancel;
    }

    private void OnDisable()
    {
        if (_navigateAction != null) _navigateAction.performed -= OnNavigate;
        if (_submitAction != null) _submitAction.performed -= OnSubmit;
        if (_cancelAction != null) _cancelAction.performed -= OnCancel;

        var uiMap = _playerInput?.actions.FindActionMap("UI");
        if (uiMap != null) uiMap.Disable();
    }

    private void OnNavigate(InputAction.CallbackContext ctx)
    {
        if (AssignedSlot == null || !AssignedSlot.IsOccupied) return;

        _navTimer -= Time.deltaTime;

        float navX = ctx.ReadValue<Vector2>().x;
        bool goLeft = navX < -0.5f;
        bool goRight = navX > 0.5f;
        bool wasLeft = _prevNavX < -0.5f;
        bool wasRight = _prevNavX > 0.5f;

        if (goLeft && (!wasLeft || _navTimer <= 0f))
        {
            AssignedSlot.SelectPrevious();
            _navTimer = NavCooldown;
        }
        else if (goRight && (!wasRight || _navTimer <= 0f))
        {
            AssignedSlot.SelectNext();
            _navTimer = NavCooldown;
        }

        _prevNavX = navX;
    }

    private void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && AssignedSlot != null)
        {
            AssignedSlot.ToggleReady();
        }
    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && AssignedSlot != null && AssignedSlot.IsReady)
        {
            AssignedSlot.ToggleReady();
        }
    }
}

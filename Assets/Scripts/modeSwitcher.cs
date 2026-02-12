using UnityEngine;
using UnityEngine.InputSystem;

public class modeSwitcher : MonoBehaviour
{
    public GameObject near;
    public GameObject far;

    public bool isNearMode;

    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        near.SetActive(isNearMode);
        far.SetActive(!isNearMode);
        if (playerInput.actions["Switch"].triggered)
        {
            isNearMode = !isNearMode;
        }
    }
}

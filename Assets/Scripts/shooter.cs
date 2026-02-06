using UnityEngine;
using UnityEngine.InputSystem;

public class Shooter : MonoBehaviour
{
    public float shootingPower;
    public float shootingAngle;

    public intake intakeScript;
    public GameObject shootingArea;

    private GameObject projectilePrefab;
    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        if (playerInput != null)
        {
            var shootAction = playerInput.actions["Fire"]; // Make sure this action exists
            shootAction.performed += OnShoot;
        }
    }

    private void OnDisable()
    {
        if (playerInput != null)
        {
            var shootAction = playerInput.actions["Fire"];
            shootAction.performed -= OnShoot;
        }
    }

    private void OnShoot(InputAction.CallbackContext context)
    {
        if (intakeScript != null && intakeScript.c.Count > 0)
        {
            ShootProjectile();
        }
    }

    private void ShootProjectile()
    {
        Vector3 direction = Quaternion.AngleAxis(shootingAngle, transform.right) * transform.forward;

        projectilePrefab = intakeScript.c[0];
        GameObject obj = Instantiate(projectilePrefab, shootingArea.transform.position, Quaternion.LookRotation(direction));

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        rb.AddForce(direction.normalized * shootingPower, ForceMode.Impulse);

        intakeScript.c.RemoveAt(0);
    }
}

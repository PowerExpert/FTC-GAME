using UnityEngine;

public class shooter : MonoBehaviour
{
    public float shootingPower = 10f;
    public float shootingAngle = 30f;
    public float fireRate = 0.3f;

    public intake intakeScript;
    public GameObject shootingArea;

    private GameObject projectilePrefab;
    private float nextFireTime = 0f;

    void Update()
    {
        if (Input.GetKey(KeyCode.JoystickButton0)
            && intakeScript != null
            && intakeScript.c.Count > 0
            && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;

            ShootProjectile();
        }
    }

    void ShootProjectile()
    {
        Vector3 direction = Quaternion.AngleAxis(shootingAngle, transform.right) * transform.forward;

        projectilePrefab = intakeScript.c[0];
        GameObject obj = Instantiate(projectilePrefab, shootingArea.transform.position, Quaternion.LookRotation(direction));

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        rb.AddForce(direction.normalized * shootingPower, ForceMode.Impulse);

        intakeScript.c.RemoveAt(0);
    }
}

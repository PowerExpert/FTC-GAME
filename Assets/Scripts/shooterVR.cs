using UnityEngine;

public class shooterVR : MonoBehaviour
{
    private float timer;
    public float fire_rate = 0.3f;

    public string VR_JBB;

    public float shootingPower;
    public float shootingAngle;

    public intake intakeScript;
    public GameObject shootingArea;

    private GameObject projectilePrefab;

    void Update()
    {
        if (Input.GetAxis(VR_JBB) != 0 && intakeScript.c.Count > 0 && timer >= fire_rate)
        {
            ShootProjectile();
            timer = 0f;
        }
        timer += Time.deltaTime;
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

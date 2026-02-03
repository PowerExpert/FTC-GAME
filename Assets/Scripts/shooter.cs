using UnityEngine;

public class shooter : MonoBehaviour
{
    public float shootingPower;
    public float shootingAngle;

    public intake intakeScript;
    private GameObject projectilePrefab;
    public GameObject shootingArea;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton2) && intakeScript != null && intakeScript.c.Count != 0)
        {
            Vector3 direction = Quaternion.AngleAxis(shootingAngle, transform.right) * transform.forward;

            projectilePrefab = intakeScript.c[0];
            GameObject obj = Instantiate(projectilePrefab, shootingArea.transform.position, Quaternion.LookRotation(direction));
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            rb.AddForce(direction.normalized * shootingPower, ForceMode.Impulse);
            
            intakeScript.c.RemoveAt(0);
        }
    }
}

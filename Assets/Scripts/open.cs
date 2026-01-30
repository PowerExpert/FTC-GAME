using UnityEngine;

public class open : MonoBehaviour
{
    public GameObject door;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Robot") && door != null)
        {
            door.SetActive(false);
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Robot") && door != null)
        {
            door.SetActive(true);
        }
    }
}

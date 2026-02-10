using UnityEngine;

public class open : MonoBehaviour
{
    public string requiredTag;

    public GameObject door;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(requiredTag) && door != null)
        {
            door.SetActive(false);
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(requiredTag) && door != null)
        {
            door.SetActive(true);
        }
    }
}

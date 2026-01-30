using UnityEngine;

public class respawn : MonoBehaviour
{
    public GameObject respawnArea;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("purpleOne") || other.CompareTag("greenOne"))
        {
            other.transform.position = respawnArea.transform.position;
        }
    }
}

using UnityEngine;

public class TriggerRead : MonoBehaviour
{
    public bool isTouching = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RedRobot") || other.CompareTag("BlueRobot"))
        {
            isTouching = true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("RedRobot") || other.CompareTag("BlueRobot"))
        {
            isTouching = false;
        }
    }
}

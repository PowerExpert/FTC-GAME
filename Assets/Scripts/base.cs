using UnityEngine;

public class base : MonoBehaviour
{
    public timer timerScript;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Robot") && timerScript.tempTime <= 0)
        {
            
        }
    }
}

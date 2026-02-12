using UnityEngine;

public class ___Trigger : MonoBehaviour
{
    public bool isTouching = false;

    void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("purpleOne") || other.CompareTag("greenOne"))
        {
            isTouching = true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        isTouching = false;
    }
}

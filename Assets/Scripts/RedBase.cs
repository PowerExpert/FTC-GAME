using UnityEngine;
using System.Collections.Generic;

public class RedBase : MonoBehaviour
{
    public bool isActivated = false;

    public timer timerScript;
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("RedRobot") && timerScript.tempTime <= 0)
        {
            isActivated = true;
        }
    }
}

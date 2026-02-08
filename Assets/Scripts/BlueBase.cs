using UnityEngine;
using System.Collections.Generic;

public class BlueBase : MonoBehaviour
{
    public bool isActivated = false;

    public timer timerScript;
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("BlueRobot") && timerScript.tempTime <= 0)
        {
            isActivated = true;
        }
    }
}

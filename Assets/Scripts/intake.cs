using System.Collections.Generic;
using UnityEngine;

public class intake : MonoBehaviour
{
    public List<GameObject> c = new List<GameObject>();

    public GameObject purple;
    public GameObject green;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("purpleOne") && c.Count < 3)
        {
            c.Add(purple);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("greenOne") && c.Count < 3)
        {
            c.Add(green);
            Destroy(other.gameObject);
        }
    }
}

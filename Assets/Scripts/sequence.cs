using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class sequence : MonoBehaviour
{
    public List<GameObject> objectsInSequence = new List<GameObject>();
    public List<char> colorSequance = new List<char>();

    public Transform parent;
    public GameObject P;
    public GameObject G;

    private void Start()
    {
        create_sequence();
    }

    public void create_sequence()
    {
        for (int i = 0; i <= 9; i+=3)
        {
            int rand = Random.Range(1, 4);
            if (rand == 1)
            {
                Instantiate(P, objectsInSequence[i].transform.position, Quaternion.identity, parent);
                Instantiate(P, objectsInSequence[i+1].transform.position, Quaternion.identity, parent);
                Instantiate(G, objectsInSequence[i+2].transform.position, Quaternion.identity, parent);
                colorSequance.Add('P');
                colorSequance.Add('P');
                colorSequance.Add('G');
            }
            else if (rand == 2)
            {
                Instantiate(P, objectsInSequence[i].transform.position, Quaternion.identity, parent);
                Instantiate(G, objectsInSequence[i + 1].transform.position, Quaternion.identity, parent);
                Instantiate(P, objectsInSequence[i + 2].transform.position, Quaternion.identity, parent);
                colorSequance.Add('P');
                colorSequance.Add('G');
                colorSequance.Add('P');
            }
            else if (rand == 3)
            {
                Instantiate(G, objectsInSequence[i].transform.position, Quaternion.identity, parent);
                Instantiate(P, objectsInSequence[i + 1].transform.position, Quaternion.identity, parent);
                Instantiate(P, objectsInSequence[i + 2].transform.position, Quaternion.identity, parent); 
                colorSequance.Add('G');
                colorSequance.Add('P');
                colorSequance.Add('P');
            }
        }
    }
}

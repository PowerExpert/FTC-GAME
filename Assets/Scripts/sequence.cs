using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class sequence : MonoBehaviour
{
    public List<GameObject> objectsInSequence = new List<GameObject>();
    public List<char> colorSequance = new List<char>();
    public List<GameObject> createdSequence = new List<GameObject>();

    public Transform parent;
    public GameObject P;
    public GameObject G;

    private void Start()
    {
        create_sequence();
    }

    public void create_sequence()
    {
        colorSequance.Clear();
        foreach (GameObject obj in createdSequence) Destroy(obj);
        createdSequence.Clear();
        for (int i = 0; i < 9; i+=3)
        {
            int rand = Random.Range(1, 4);
            if (rand == 1)
            {
                createdSequence.Add(Instantiate(P, objectsInSequence[i].transform.position, Quaternion.identity, parent));
                createdSequence.Add(Instantiate(P, objectsInSequence[i+1].transform.position, Quaternion.identity, parent));
                createdSequence.Add(Instantiate(G, objectsInSequence[i+2].transform.position, Quaternion.identity, parent));
                colorSequance.Add('P');
                colorSequance.Add('P');
                colorSequance.Add('G');
            }
            else if (rand == 2)
            {
                createdSequence.Add(Instantiate(P, objectsInSequence[i].transform.position, Quaternion.identity, parent));
                createdSequence.Add(Instantiate(G, objectsInSequence[i + 1].transform.position, Quaternion.identity, parent));
                createdSequence.Add(Instantiate(P, objectsInSequence[i + 2].transform.position, Quaternion.identity, parent));
                colorSequance.Add('P');
                colorSequance.Add('G');
                colorSequance.Add('P');
            }
            else if (rand == 3)
            {
                createdSequence.Add(Instantiate(G, objectsInSequence[i].transform.position, Quaternion.identity, parent));
                createdSequence.Add(Instantiate(P, objectsInSequence[i + 1].transform.position, Quaternion.identity, parent));
                createdSequence.Add(Instantiate(P, objectsInSequence[i + 2].transform.position, Quaternion.identity, parent)); 
                colorSequance.Add('G');
                colorSequance.Add('P');
                colorSequance.Add('P');
            }
        }
    }
}

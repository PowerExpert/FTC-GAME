using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameObjectPair
{
    public GameObject prefab;
    public GameObject spawnPoint;
}

public class spawner : MonoBehaviour
{
    public List<GameObjectPair> objs = new List<GameObjectPair>();

    public Transform parent;

    public void respawn(Transform parent)
    {
        for (int i = 0; i < objs.Count; i++)
        {
            GameObject obj = Instantiate(objs[i].prefab, objs[i].spawnPoint.transform.position, Quaternion.identity, parent);
        }
    }
    public void Start()
    {
        respawn(parent);
    }
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            GameObject[] green;
            GameObject[] purple;
            green = GameObject.FindGameObjectsWithTag("greenOne");
            purple = GameObject.FindGameObjectsWithTag("purpleOne");

            foreach (GameObject obj in green) Destroy(obj);
            foreach (GameObject obj in purple) Destroy(obj);
            respawn(parent);
        }
    }
}

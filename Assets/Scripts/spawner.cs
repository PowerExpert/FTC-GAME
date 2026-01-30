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

    public void respawn()
    {
        for (int i = 0; i < objs.Count; i++)
        {
            Instantiate(objs[i].prefab, objs[i].spawnPoint.transform.position, Quaternion.identity);
        }
    }
    public void Start()
    {
        respawn();
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
            respawn();
        }
    }
}

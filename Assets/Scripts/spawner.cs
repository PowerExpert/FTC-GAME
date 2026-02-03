using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class GameObjectPair
{
    public GameObject prefab;
    public GameObject spawnPoint;
}

public class spawner : MonoBehaviour
{
    public List<GameObjectPair> objsToRespawn = new List<GameObjectPair>();
    public List<GameObjectPair> objsToTeleport = new List<GameObjectPair>();

    public Text blueScore;
    public Text redScore;

    public Transform parent;

    public void respawn(Transform parent)
    {
        GameObject[] green;
        GameObject[] purple;
        green = GameObject.FindGameObjectsWithTag("greenOne");
        purple = GameObject.FindGameObjectsWithTag("purpleOne");

        foreach (GameObject obj in green) Destroy(obj);
        foreach (GameObject obj in purple) Destroy(obj);
        for (int i = 0; i < objsToRespawn.Count; i++)
        {
            GameObject obj = Instantiate(objsToRespawn[i].prefab, objsToRespawn[i].spawnPoint.transform.position, Quaternion.identity, parent);
        }
        for (int i = 0; i < objsToTeleport.Count; i++)
        {
            objsToTeleport[i].prefab.transform.position = objsToTeleport[i].spawnPoint.transform.position;
        }
        blueScore.text = "0";
        redScore.text = "0";
    }
    public void Start()
    {
        respawn(parent);
    }
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            respawn(parent);
        }
    }
}

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
    [Header("Objects")]
    public List<GameObjectPair> objsToRespawn = new();
    public List<GameObjectPair> objsToTeleport = new();
    public Transform parent;

    [Header("Players")]
    public List<GameObject> playerObjects = new();
    public List<Transform> playerSpawns = new();

    [Header("Score")]
    public Text blueScore;
    public Text redScore;

    // ?????????????????????????????????????????????????????????????????????????

    private void Start() => respawn(parent);
    private void Update() { if (Input.GetKeyDown(KeyCode.R)) respawn(parent); }

    // ?????????????????????????????????????????????????????????????????????????

    public void respawn(Transform spawnParent)
    {
        // Destroy old tagged objects
        foreach (var obj in GameObject.FindGameObjectsWithTag("greenOne")) Destroy(obj);
        foreach (var obj in GameObject.FindGameObjectsWithTag("purpleOne")) Destroy(obj);

        // Re-instantiate world objects
        foreach (var pair in objsToRespawn)
        {
            if (pair.prefab == null || pair.spawnPoint == null) continue;
            Instantiate(pair.prefab, pair.spawnPoint.transform.position,
                        Quaternion.identity, spawnParent);
        }

        // Teleport non-player objects
        foreach (var pair in objsToTeleport)
        {
            if (pair.prefab == null || pair.spawnPoint == null) continue;
            pair.prefab.transform.position = pair.spawnPoint.transform.position;
        }

        // Teleport players back to their spawn points
        for (int i = 0; i < playerObjects.Count; i++)
        {
            if (playerObjects[i] == null) continue;
            if (i >= playerSpawns.Count || playerSpawns[i] == null) continue;

            // CharacterController blocks Transform.position — disable briefly
            var cc = playerObjects[i].GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            playerObjects[i].transform.SetPositionAndRotation(
                playerSpawns[i].position,
                playerSpawns[i].rotation);

            if (cc != null) cc.enabled = true;
        }

        // Reset scores
        if (redScore != null) redScore.text = "0";
        if (blueScore != null) blueScore.text = "0";
    }

    /// <summary>
    /// Called by MultiplayerJoinManager.BeginGame() for each spawned player.
    /// </summary>
    public void RegisterPlayer(GameObject playerObj, Transform spawnPoint)
    {
        playerObjects.Add(playerObj);
        playerSpawns.Add(spawnPoint);
    }
}
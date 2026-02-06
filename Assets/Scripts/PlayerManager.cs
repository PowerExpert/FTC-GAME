using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    public List<GameObject> players = new List<GameObject>();
    public PlayerInputManager manager;

    public int playerCount;
    void Update()
    {
        manager.onPlayerJoined += OnPlayerJoined;
    }
    private void OnPlayerJoined(PlayerInput playerInput)
    {
        manager.playerPrefab = players[playerCount];
        playerCount++;
    }
}

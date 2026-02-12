using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    public GameObject tutorText;
    public GameObject timerText;

    public timer timerScript;

    public List<GameObject> players = new List<GameObject>();
    public PlayerInputManager manager;

    public int playerCount;

    void Start()
    {
        timerText.SetActive(false);
        tutorText.SetActive(true);
    }
    void Update()
    {
        manager.onPlayerJoined += OnPlayerJoined;
    }
    private void OnPlayerJoined(PlayerInput playerInput)
    {
        manager.playerPrefab = players[playerCount];
        tutorText.SetActive(false);
        timerText.SetActive(true);
        timerScript.tick = true;
        playerCount++;
    }
}

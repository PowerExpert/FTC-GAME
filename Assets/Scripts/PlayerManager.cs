using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public struct TextComponents
{
    public GameObject firstText;
    public GameObject secondText;
}

public class PlayerManager : MonoBehaviour
{
    public GameObject canvasJoin;

    public GameObject tutorText;
    public GameObject timerText;
    public List<TextComponents> texts = new List<TextComponents>();
    public timer timerScript;
    public List<GameObject> playerPrefabs = new List<GameObject>();
    public PlayerInputManager manager;

    private int playerCount = 0;

    void Start()
    {
        timerText.SetActive(false);
        tutorText.SetActive(true);

        if (playerPrefabs.Count > 0)
        {
            manager.playerPrefab = playerPrefabs[0];
        }
    }

    void OnEnable()
    {
        manager.onPlayerJoined += OnPlayerJoined;
    }

    void OnDisable()
    {
        manager.onPlayerJoined -= OnPlayerJoined;
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        tutorText.SetActive(false);
        timerText.SetActive(true);

        if (timerScript != null) timerScript.tick = true;

        if (playerCount < texts.Count)
        {
            GameObject firstObj = texts[playerCount].firstText;
            if (firstObj != null)
            {
                var textComponent = firstObj.GetComponent<Graphic>();
                if (textComponent != null)
                {
                    if(playerCount % 2 != 0)
                        textComponent.color = Color.blue;
                    else
                        textComponent.color = Color.red;
                }
            }

            GameObject secondObj = texts[playerCount].secondText;
            if (secondObj != null)
            {
                secondObj.SetActive(true);
            }
        }

        playerCount++;

        if (playerCount < playerPrefabs.Count)
        {
            manager.playerPrefab = playerPrefabs[playerCount];
        }
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnFinishPressed();
            manager.DisableJoining();
        }
    }
    public void OnFinishPressed()
    {
        canvasJoin.SetActive(false);
        manager.DisableJoining();
    }
}
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

/// <summary>
/// PlayerManager — keeps your existing join-screen text logic,
/// now also integrates with CharacterSelectionManager.
///
/// If you are using CharacterSelectionManager as your primary join screen,
/// you can attach BOTH scripts to the same GameObject, or keep this one
/// for the legacy text-only flow and use CharacterSelectionManager for the new UI.
/// </summary>
public class PlayerManager1 : MonoBehaviour
{
    [Header("Canvas / Panels")]
    public GameObject canvasJoin;
    public GameObject tutorText;
    public GameObject timerText;

    [Header("Per-Player Join Text (legacy)")]
    public List<TextComponents> texts = new List<TextComponents>();

    [Header("Timer")]
    public timer timerScript;

    [Header("Player Prefabs (fallback if no CharacterData)")]
    public List<GameObject> playerPrefabs = new List<GameObject>();

    [Header("Input Manager")]
    public PlayerInputManager manager;

    [Header("Optional: Link to character selection system")]
    [Tooltip("If assigned, PlayerManager defers prefab logic to CharacterSelectionManager.")]
    public CharacterSelectionManager charSelectionManager;

    private int playerCount = 0;

    // ---------------------------------------------------------------

    void Start()
    {
        if (timerText != null) timerText.SetActive(false);
        if (tutorText != null) tutorText.SetActive(true);

        // Set first prefab only if we are NOT using the character selection manager
        if (charSelectionManager == null && manager != null && playerPrefabs.Count > 0)
        {
            manager.playerPrefab = playerPrefabs[0];
        }
    }

    void OnEnable()
    {
        if (manager != null)
            manager.onPlayerJoined += OnPlayerJoined;
    }

    void OnDisable()
    {
        if (manager != null)
            manager.onPlayerJoined -= OnPlayerJoined;
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        if (tutorText != null) tutorText.SetActive(false);
        if (timerText != null) timerText.SetActive(true);

        // Legacy text coloring — still works alongside new system
        if (playerCount < texts.Count)
        {
            GameObject firstObj = texts[playerCount].firstText;
            if (firstObj != null)
            {
                var textComponent = firstObj.GetComponent<Graphic>();
                if (textComponent != null)
                    textComponent.color = (playerCount % 2 != 0) ? Color.blue : Color.red;
            }

            GameObject secondObj = texts[playerCount].secondText;
            if (secondObj != null)
                secondObj.SetActive(true);
        }

        playerCount++;

        // Advance prefab only when NOT using CharacterSelectionManager
        if (charSelectionManager == null && manager != null && playerCount < playerPrefabs.Count)
        {
            manager.playerPrefab = playerPrefabs[playerCount];
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnFinishPressed();
        }
    }

    public void OnFinishPressed()
    {
        if (canvasJoin != null) canvasJoin.SetActive(false);
        if (manager != null) manager.DisableJoining();

        // If timer should start here (not handled by CharacterSelectionManager)
        if (charSelectionManager == null && timerScript != null)
            timerScript.tick = true;
    }
}

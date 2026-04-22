using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Central manager for the multiplayer join + character selection screen.
///
/// SETUP:
///  1. Create a Canvas with a full-screen background image.
///  2. Create up to 4 "PlayerSlot" panel objects (see PlayerSlotUI).
///     - Each panel should be ~1/3 screen width, vertically centered.
///     - Assign each to the slots[] list below.
///  3. Assign the PlayerInputManager from your scene.
///  4. Set the characters list — or assign them on each PlayerSlotUI directly.
///  5. Optionally set a startButton that only activates once all joined players are ready.
/// </summary>
public class CharacterSelectionManager : MonoBehaviour
{
    [Header("Player Input")]
    public PlayerInputManager inputManager;

    [Header("Slot UI Panels (up to 4)")]
    public List<PlayerSlotUI> slots = new List<PlayerSlotUI>();   // Assign all slot panels here

    [Header("Shared Character List")]
    [Tooltip("If set, these characters are pushed to every slot on Start.")]
    public List<CharacterData> sharedCharacters = new List<CharacterData>();

    [Header("Screen UI")]
    public Text instructionText;          // e.g. "Press any button to join!"
    public Button startButton;            // Becomes interactable when all joined players are ready
    public GameObject joinCanvas;         // The whole join/select canvas — hide when done
    public Text countdownText;            // Optional "Starting in 3..." countdown

    [Header("Timer Integration")]
    public timer timerScript;             // Your existing timer.cs reference

    // Runtime
    private int joinedCount = 0;
    private float countdownTimer = 0f;
    private bool countingDown = false;
    private const float COUNTDOWN_DURATION = 3f;

    // ---------------------------------------------------------------
    // Unity lifecycle
    // ---------------------------------------------------------------

    private void Start()
    {
        // Push shared character list to every slot
        if (sharedCharacters.Count > 0)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].playerIndex = i;
                slots[i].characters = sharedCharacters;
            }
        }
        else
        {
            // Just set indices
            for (int i = 0; i < slots.Count; i++)
                slots[i].playerIndex = i;
        }

        if (startButton != null)
        {
            startButton.interactable = false;
            startButton.onClick.AddListener(OnStartPressed);
        }

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        UpdateInstructionText();
    }

    private void OnEnable()
    {
        if (inputManager != null)
        {
            inputManager.onPlayerJoined += OnPlayerJoined;
            inputManager.onPlayerLeft   += OnPlayerLeft;
        }
    }

    private void OnDisable()
    {
        if (inputManager != null)
        {
            inputManager.onPlayerJoined -= OnPlayerJoined;
            inputManager.onPlayerLeft   -= OnPlayerLeft;
        }
    }

    private void Update()
    {
        // Keyboard shortcut: Enter = start (same as your existing code)
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnStartPressed();
        }

        // Countdown logic
        if (countingDown)
        {
            countdownTimer -= Time.deltaTime;
            if (countdownText != null)
                countdownText.text = "Starting in " + Mathf.CeilToInt(countdownTimer) + "...";

            if (countdownTimer <= 0f)
            {
                countingDown = false;
                BeginGame();
            }
        }

        // Keep start button state synced
        if (startButton != null)
            startButton.interactable = (joinedCount > 0 && AllJoinedPlayersReady());
    }

    // ---------------------------------------------------------------
    // Player join / leave callbacks
    // ---------------------------------------------------------------

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        if (joinedCount >= slots.Count)
        {
            Debug.LogWarning("CharacterSelectionManager: No free slot for new player.");
            return;
        }

        PlayerSlotUI slot = slots[joinedCount];
        slot.AssignPlayer(playerInput);
        joinedCount++;

        UpdateInstructionText();
        Debug.Log($"Player {joinedCount} joined → slot {joinedCount - 1}");
    }

    private void OnPlayerLeft(PlayerInput playerInput)
    {
        // Find which slot this player was in and free it
        // Simple approach: free the last occupied slot
        if (joinedCount <= 0) return;
        joinedCount--;
        slots[joinedCount].UnassignPlayer();
        UpdateInstructionText();
    }

    // ---------------------------------------------------------------
    // Ready / Start
    // ---------------------------------------------------------------

    private bool AllJoinedPlayersReady()
    {
        for (int i = 0; i < joinedCount; i++)
        {
            if (!slots[i].IsReady()) return false;
        }
        return true;
    }

    public void OnStartPressed()
    {
        if (joinedCount == 0) return;

        // Stop more players from joining
        if (inputManager != null)
            inputManager.DisableJoining();

        // Start countdown
        if (!countingDown)
        {
            countingDown = true;
            countdownTimer = COUNTDOWN_DURATION;

            if (countdownText != null)
                countdownText.gameObject.SetActive(true);
        }
    }

    private void BeginGame()
    {
        // Hide the join/select canvas
        if (joinCanvas != null)
            joinCanvas.SetActive(false);

        // Start the in-game timer
        if (timerScript != null)
            timerScript.tick = true;

        Debug.Log("Game started! Selected characters:");
        for (int i = 0; i < joinedCount; i++)
        {
            CharacterData cd = slots[i].GetSelectedCharacter();
            Debug.Log($"  Player {i + 1}: {(cd != null ? cd.characterName : "default")}");
        }
    }

    // ---------------------------------------------------------------
    // UI helpers
    // ---------------------------------------------------------------

    private void UpdateInstructionText()
    {
        if (instructionText == null) return;

        if (joinedCount == 0)
            instructionText.text = "PRESS ANY BUTTON TO JOIN";
        else if (joinedCount < slots.Count)
            instructionText.text = joinedCount + " PLAYER(S) JOINED  •  PRESS ANY BUTTON TO JOIN";
        else
            instructionText.text = "MAX PLAYERS REACHED";
    }
}

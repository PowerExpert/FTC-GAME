using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Handles one player's slot panel on the join screen.
/// Attach this to each "PlayerSlot" panel prefab/object.
/// </summary>
public class PlayerSlotUI : MonoBehaviour
{
    [Header("Slot State")]
    public int playerIndex = 0;          // 0-based (Player 1 = 0)
    public bool isOccupied = false;

    [Header("UI References")]
    [SerializeField] private GameObject waitingPanel;        // "Press any button to join" state
    [SerializeField] private GameObject selectionPanel;      // Character selection state
    [SerializeField] private Text playerLabel;               // "PLAYER 1", "PLAYER 2", etc.
    [SerializeField] private Text characterNameText;         // Current character name
    [SerializeField] private Image characterPreviewImage;    // Character portrait/preview
    [SerializeField] private Text readyText;                 // "READY" label (shown when locked in)
    [SerializeField] private Button leftArrowButton;         // Previous character
    [SerializeField] private Button rightArrowButton;        // Next character
    [SerializeField] private Button readyButton;             // Lock in selection

    [Header("Team Color")]
    [SerializeField] private Image panelBackground;          // The grey pane background
    [SerializeField] private Image headerBar;                // Colored header bar per team
    private static readonly Color[] teamColors = new Color[]
    {
        new Color(0.85f, 0.15f, 0.15f, 1f),  // Red  - Player 1
        new Color(0.10f, 0.35f, 0.85f, 1f),  // Blue - Player 2
        new Color(0.10f, 0.75f, 0.20f, 1f),  // Green- Player 3
        new Color(0.85f, 0.65f, 0.05f, 1f),  // Gold - Player 4
    };

    [Header("Characters")]
    public List<CharacterData> characters = new List<CharacterData>();

    // Runtime state
    private int selectedCharacterIndex = 0;
    private bool isReady = false;
    private PlayerInput assignedInput = null;

    // ---------------------------------------------------------------
    // Public API
    // ---------------------------------------------------------------

    /// <summary>Called by CharacterSelectionManager when a player joins and is assigned to this slot.</summary>
    public void AssignPlayer(PlayerInput input)
    {
        assignedInput = input;
        isOccupied = true;
        isReady = false;
        selectedCharacterIndex = 0;

        // Apply team color
        Color col = teamColors[Mathf.Clamp(playerIndex, 0, teamColors.Length - 1)];
        if (headerBar != null) headerBar.color = col;
        if (playerLabel != null)
        {
            playerLabel.text = "PLAYER " + (playerIndex + 1);
            playerLabel.color = col;
        }

        ShowSelectionPanel();
        RefreshCharacterDisplay();
    }

    public void UnassignPlayer()
    {
        assignedInput = null;
        isOccupied = false;
        isReady = false;
        ShowWaitingPanel();
    }

    public bool IsReady() => isReady;
    public int GetSelectedCharacterIndex() => selectedCharacterIndex;
    public CharacterData GetSelectedCharacter()
    {
        if (characters == null || characters.Count == 0) return null;
        return characters[selectedCharacterIndex];
    }

    public void SetWaitingPanel(GameObject wp) { waitingPanel = wp; }
    public void SetSelectionPanel(GameObject sp) { selectionPanel = sp; }
    public void SetPlayerLabel(Text t) { playerLabel = t; }
    public void SetCharacterNameText(Text t) { characterNameText = t; }
    public void SetCharacterPreviewImage(Image i) { characterPreviewImage = i; }
    public void SetReadyText(Text t) { readyText = t; }
    public void SetLeftArrow(Button b) { leftArrowButton = b; if (b != null) b.onClick.AddListener(OnPreviousCharacter); }
    public void SetRightArrow(Button b) { rightArrowButton = b; if (b != null) b.onClick.AddListener(OnNextCharacter); }
    public void SetReadyButton(Button b) { readyButton = b; if (b != null) b.onClick.AddListener(OnReadyToggle); }
    public void SetBackgroundImage(Image i) { panelBackground = i; }
    public void SetHeaderBar(Image i) { headerBar = i; }

    // ---------------------------------------------------------------
    // Unity lifecycle
    // ---------------------------------------------------------------

    private void Start()
    {
        // Wire up arrow buttons
        if (leftArrowButton != null)
            leftArrowButton.onClick.AddListener(OnPreviousCharacter);
        if (rightArrowButton != null)
            rightArrowButton.onClick.AddListener(OnNextCharacter);
        if (readyButton != null)
            readyButton.onClick.AddListener(OnReadyToggle);

        ShowWaitingPanel();
    }

    private void Update()
    {
        if (!isOccupied || assignedInput == null) return;

        // Joystick / gamepad navigation for character selection
        // Left stick or D-pad horizontal
        float nav = assignedInput.actions["Move"]?.ReadValue<Vector2>().x ?? 0f;

        // Edge-detect so we don't spin through characters every frame
        // (handled via button presses; joystick support is additive via threshold)
    }

    // ---------------------------------------------------------------
    // Button callbacks
    // ---------------------------------------------------------------

    public void OnPreviousCharacter()
    {
        if (!isOccupied || isReady || characters.Count == 0) return;
        selectedCharacterIndex = (selectedCharacterIndex - 1 + characters.Count) % characters.Count;
        RefreshCharacterDisplay();
    }

    public void OnNextCharacter()
    {
        if (!isOccupied || isReady || characters.Count == 0) return;
        selectedCharacterIndex = (selectedCharacterIndex + 1) % characters.Count;
        RefreshCharacterDisplay();
    }

    public void OnReadyToggle()
    {
        if (!isOccupied) return;
        isReady = !isReady;
        RefreshReadyState();
    }

    // ---------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------

    private void ShowWaitingPanel()
    {
        if (playerLabel != null)
        {
            playerLabel.text = "WAITING...";
            playerLabel.color = teamColors[Mathf.Clamp(playerIndex, 0, teamColors.Length - 1)];
        }
        if (characterNameText != null)
            characterNameText.text = "Press button to join";
    }

    private void ShowSelectionPanel()
    {
        if (playerLabel != null)
        {
            playerLabel.text = "PLAYER " + (playerIndex + 1);
            playerLabel.color = teamColors[Mathf.Clamp(playerIndex, 0, teamColors.Length - 1)];
        }
    }

    private void RefreshCharacterDisplay()
    {
        if (characters == null || characters.Count == 0) return;
        CharacterData data = characters[selectedCharacterIndex];

        if (characterNameText != null)
            characterNameText.text = data.characterName;

        if (characterPreviewImage != null && data.previewSprite != null)
            characterPreviewImage.sprite = data.previewSprite;
    }

    private void RefreshReadyState()
    {
        if (readyText != null) readyText.gameObject.SetActive(isReady);

        // Dim arrows when ready (locked in)
        if (leftArrowButton != null) leftArrowButton.interactable = !isReady;
        if (rightArrowButton != null) rightArrowButton.interactable = !isReady;
    }
}

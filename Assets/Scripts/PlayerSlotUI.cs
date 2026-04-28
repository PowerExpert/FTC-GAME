using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// UI slot for one player in the lobby.
/// Driven entirely by MultiplayerJoinManager — no PlayerInput reference needed.
/// </summary>
public class PlayerSlotUI : MonoBehaviour
{
    [Header("Config")]
    public int PlayerIndex;
    public List<CharacterData> Characters;

    [Header("UI References")]
    public Text PlayerLabel;
    public Text CharacterNameText;
    public Text DescriptionText;
    public Text ReadyIndicator;
    public Image HeaderBar;
    public Image PreviewImage;
    public Button LeftArrow;
    public Button RightArrow;
    public Button ReadyButton;

    // ?? Public state ??????????????????????????????????????????????????????????
    public bool IsOccupied { get; private set; }
    public bool IsReady { get; private set; }
    public CharacterData SelectedCharacter { get; private set; }

    private int _charIndex;

    private static readonly Color[] TeamColors =
    {
        new(0.85f, 0.15f, 0.15f),
        new(0.15f, 0.45f, 0.85f),
        new(0.15f, 0.75f, 0.25f),
        new(0.85f, 0.65f, 0.10f)
    };

    // ?????????????????????????????????????????????????????????????????????????

    /// <summary>
    /// Called when a device joins. Shows the slot as occupied.
    /// </summary>
    public void AssignPlayerDevice(int characterIndex, List<CharacterData> chars)
    {
        Characters = chars;
        IsOccupied = true;
        IsReady = false;
        _charIndex = 0;

        Color col = TeamColors[Mathf.Clamp(PlayerIndex, 0, TeamColors.Length - 1)];
        if (HeaderBar != null) HeaderBar.color = col;
        if (PlayerLabel != null)
        {
            PlayerLabel.text = $"PLAYER {PlayerIndex + 1}";
            PlayerLabel.color = col;
        }

        if (ReadyButton != null) ReadyButton.gameObject.SetActive(true);
        if (ReadyIndicator != null) ReadyIndicator.gameObject.SetActive(false);
        if (PreviewImage != null) PreviewImage.gameObject.SetActive(true);
        SetArrowsActive(true);

        SetCharacterIndex(characterIndex);
    }

    /// <summary>
    /// Called when a player leaves the lobby.
    /// </summary>
    public void UnassignPlayer()
    {
        IsOccupied = false;
        IsReady = false;
        SelectedCharacter = null;
        _charIndex = 0;

        if (PlayerLabel != null) PlayerLabel.text = "WAITING...";
        if (CharacterNameText != null) CharacterNameText.text = "Press button to join";
        if (DescriptionText != null) DescriptionText.text = "";
        if (ReadyButton != null) ReadyButton.gameObject.SetActive(false);
        if (ReadyIndicator != null) ReadyIndicator.gameObject.SetActive(false);
        if (PreviewImage != null) PreviewImage.gameObject.SetActive(false);
        SetArrowsActive(false);
    }

    /// <summary>
    /// Updates which character is displayed. Called by MJM when player navigates.
    /// </summary>
    public void SetCharacterIndex(int index)
    {
        if (Characters == null || Characters.Count == 0) return;

        _charIndex = ((index % Characters.Count) + Characters.Count) % Characters.Count;
        SelectedCharacter = Characters[_charIndex];

        if (CharacterNameText != null)
            CharacterNameText.text = SelectedCharacter?.characterName ?? "";

        if (DescriptionText != null)
            DescriptionText.text = SelectedCharacter?.description ?? "";

        if (PreviewImage != null)
        {
            Sprite portrait = SelectedCharacter?.previewSprite;
            if (portrait != null)
            {
                PreviewImage.sprite = portrait;
                PreviewImage.gameObject.SetActive(true);
            }
            else
            {
                PreviewImage.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Syncs ready state and updates UI. Called by MJM on submit/cancel.
    /// </summary>
    public void SetReady(bool ready)
    {
        if (!IsOccupied) return;
        IsReady = ready;

        if (ReadyIndicator != null) ReadyIndicator.gameObject.SetActive(ready);
        if (ReadyButton != null) ReadyButton.gameObject.SetActive(!ready);
        SetArrowsActive(!ready);
    }

    // ?? Still available for button onClick listeners in the UI ????????????????

    public void SelectNext()
    {
        if (!IsOccupied || IsReady) return;
        SetCharacterIndex(_charIndex + 1);
    }

    public void SelectPrevious()
    {
        if (!IsOccupied || IsReady) return;
        SetCharacterIndex(_charIndex - 1);
    }

    public void ToggleReady()
    {
        SetReady(!IsReady);
    }

    // ?? Helpers ???????????????????????????????????????????????????????????????

    private void SetArrowsActive(bool value)
    {
        if (LeftArrow != null) { LeftArrow.gameObject.SetActive(value); LeftArrow.interactable = value; }
        if (RightArrow != null) { RightArrow.gameObject.SetActive(value); RightArrow.interactable = value; }
    }

    // Legacy — kept so existing code that calls AssignPlayer(PlayerInput) still compiles.
    // MJM no longer uses this path.
    public void AssignPlayer(PlayerInput input)
    {
        AssignPlayerDevice(0, Characters);
    }

    public PlayerInput GetAssignedPlayerInput() => null;
}
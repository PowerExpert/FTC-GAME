using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// UI slot for one player. Shows character selection and ready state.
/// </summary>
public class PlayerSlotUI : MonoBehaviour
{
    [Header("UI References")]
    public int PlayerIndex = 0;
    public List<CharacterData> Characters;

    public Text PlayerLabel;
    public Text CharacterNameText;
    public Text DescriptionText;
    public Text ReadyIndicator;
    public Image HeaderBar;
    public Image PreviewImage;
    public Button LeftArrow;
    public Button RightArrow;
    public Button ReadyButton;

    public bool IsReady { get; private set; }
    public bool IsOccupied { get; private set; }
    public CharacterData SelectedCharacter =>
        (Characters != null && Characters.Count > 0) ? Characters[_charIndex] : null;

    private int _charIndex;
    private PlayerInput _input;

    private static readonly Color[] TeamColors =
    {
        new(0.85f, 0.15f, 0.15f), new(0.15f, 0.45f, 0.85f),
        new(0.15f, 0.75f, 0.25f), new(0.85f, 0.65f, 0.10f)
    };

    public void AssignPlayer(PlayerInput input)
    {
        _input = input;
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

        SetArrowsInteractable(true);
        if (ReadyButton != null) ReadyButton.gameObject.SetActive(true);
        if (ReadyIndicator != null) ReadyIndicator.gameObject.SetActive(false);
        if (PreviewImage != null) PreviewImage.gameObject.SetActive(true);

        RefreshDisplay();
    }

    public void UnassignPlayer()
    {
        _input = null;
        IsOccupied = false;
        IsReady = false;
        _charIndex = 0;

        if (PlayerLabel != null) PlayerLabel.text = "WAITING...";
        if (CharacterNameText != null) CharacterNameText.text = "Press button to join";
        if (DescriptionText != null) DescriptionText.text = "";
        if (ReadyButton != null) ReadyButton.gameObject.SetActive(false);
        if (ReadyIndicator != null) ReadyIndicator.gameObject.SetActive(false);
        if (PreviewImage != null) PreviewImage.gameObject.SetActive(false);

        SetArrowsInteractable(false);
    }

    public void SelectNext()
    {
        if (!IsOccupied || IsReady || Characters == null || Characters.Count == 0) return;
        _charIndex = (_charIndex + 1) % Characters.Count;
        RefreshDisplay();
    }

    public void SelectPrevious()
    {
        if (!IsOccupied || IsReady || Characters == null || Characters.Count == 0) return;
        _charIndex = (_charIndex - 1 + Characters.Count) % Characters.Count;
        RefreshDisplay();
    }

    public void ToggleReady()
    {
        if (!IsOccupied) return;
        IsReady = !IsReady;
        SetArrowsInteractable(!IsReady);
        if (ReadyButton != null) ReadyButton.gameObject.SetActive(!IsReady);
        if (ReadyIndicator != null) ReadyIndicator.gameObject.SetActive(IsReady);
    }

    private void RefreshDisplay()
    {
        var selected = SelectedCharacter;
        if (selected == null) return;

        if (CharacterNameText != null) CharacterNameText.text = selected.characterName;
        if (DescriptionText != null) DescriptionText.text = selected.description;
        if (PreviewImage != null) PreviewImage.sprite = selected.previewSprite;
    }

    private void SetArrowsInteractable(bool value)
    {
        if (LeftArrow != null) LeftArrow.interactable = value;
        if (RightArrow != null) RightArrow.interactable = value;
    }
}

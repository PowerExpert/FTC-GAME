using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Multiplayer lobby: JOIN → CHARACTER SELECT → GAME START
/// </summary>
public class MultiplayerJoinManager : MonoBehaviour
{
    [Header("Required")]
    public List<CharacterData> characters;
    public PlayerInputManager inputManager;
    public timer timerScript;

    [Header("Spawn Points")]
    public List<Transform> spawnPoints;

    [Header("Settings")]
    public int maxPlayers = 4;
    public float countdownTime = 3f;

    private enum GameState { Joining, CharacterSelect, Countdown, Playing }
    private GameState state = GameState.Joining;

    private readonly List<PlayerSlotUI> slots = new();
    private readonly List<PlayerInput> players = new();
    private readonly HashSet<InputDevice> joinedDevices = new();

    private Text instructionText;
    private Text countdownText;
    private Button startButton;
    private GameObject joinCanvas;

    private float countdownTimer;
    private float joinDebounceTimer;

    private InputAction _joinInputAction;

    private static readonly Color[] TeamColors =
    {
        new(0.85f, 0.15f, 0.15f), new(0.15f, 0.45f, 0.85f),
        new(0.15f, 0.75f, 0.25f), new(0.85f, 0.65f, 0.10f)
    };

    private static readonly Vector2[] SlotPositions =
    {
        new(-400, 200), new(400, 200), new(-400, -200), new(400, -200)
    };

    private void Start()
    {
        if (characters == null || characters.Count == 0)
        {
            Debug.LogError("[MJM] No characters assigned!");
            return;
        }

        if (inputManager == null)
        {
            Debug.LogError("[MJM] PlayerInputManager not assigned!");
            return;
        }

        inputManager.onPlayerJoined += OnPlayerJoined;
        inputManager.onPlayerLeft += OnPlayerLeft;

        BuildUI();
        SetupJoinInput();
        UpdateInstructionText();
    }

    private void OnDestroy()
    {
        if (inputManager != null)
        {
            inputManager.onPlayerJoined -= OnPlayerJoined;
            inputManager.onPlayerLeft -= OnPlayerLeft;
        }
        _joinInputAction?.Disable();
        _joinInputAction?.Dispose();
    }

    private void Update()
    {
        if (joinDebounceTimer > 0f)
            joinDebounceTimer -= Time.deltaTime;

        // Auto-start countdown when all ready
        if ((state == GameState.Joining || state == GameState.CharacterSelect) && AllPlayersReady() && players.Count > 0)
        {
            StartCountdown();
        }

        if (state == GameState.Countdown)
        {
            countdownTimer -= Time.deltaTime;
            if (countdownText != null)
                countdownText.text = $"Starting in {Mathf.CeilToInt(countdownTimer)}...";

            if (countdownTimer <= 0f)
                BeginGame();
        }

        if (startButton != null)
            startButton.interactable = AllPlayersReady() && players.Count > 0;
    }

    #region Join Handling

    private void SetupJoinInput()
    {
        _joinInputAction = new InputAction("Join", binding: "*/<Button>");
        _joinInputAction.performed += OnJoinInput;
        _joinInputAction.Enable();
    }

    private void OnJoinInput(InputAction.CallbackContext ctx)
    {
        if (state != GameState.Joining && state != GameState.CharacterSelect)
            return;

        if (players.Count >= maxPlayers || joinDebounceTimer > 0f)
            return;

        InputDevice device = ctx.control.device;

        // Check if device already joined
        if (joinedDevices.Contains(device))
            return;

        inputManager.JoinPlayer(-1, -1, null, device);
    }

    private void OnPlayerJoined(PlayerInput pi)
    {
        if (players.Count >= maxPlayers || joinDebounceTimer > 0f)
        {
            Destroy(pi.gameObject);
            return;
        }

        // Get the device that triggered this join
        InputDevice device = null;
        if (pi.devices.Count > 0)
            device = pi.devices[0];

        // Check if this device already joined
        if (device != null && joinedDevices.Contains(device))
        {
            Destroy(pi.gameObject);
            return;
        }

        // Track this device
        if (device != null)
            joinedDevices.Add(device);

        players.Add(pi);
        int slotIndex = players.Count - 1;

        PlayerSlotUI slot = slots[slotIndex];
        slot.AssignPlayer(pi);

        if (pi.TryGetComponent<SlotInputHandler>(out var handler))
        {
            handler.AssignedSlot = slot;
            handler.enabled = true;
        }

        state = GameState.CharacterSelect;
        joinDebounceTimer = 0.5f;

        UpdateInstructionText();

        string deviceName = device != null ? device.displayName : "unknown";
        Debug.Log($"[MJM] Player {players.Count} joined on {deviceName}");
    }

    private void OnPlayerLeft(PlayerInput pi)
    {
        int idx = players.IndexOf(pi);
        if (idx < 0) return;

        // Remove device from joined set
        if (pi.devices.Count > 0)
            joinedDevices.Remove(pi.devices[0]);

        slots[idx].UnassignPlayer();
        players.RemoveAt(idx);

        // Shift remaining players down
        for (int i = idx; i < players.Count; i++)
        {
            slots[i].AssignPlayer(players[i]);
            if (players[i].TryGetComponent<SlotInputHandler>(out var h))
                h.AssignedSlot = slots[i];
        }

        if (players.Count == 0)
            state = GameState.Joining;

        UpdateInstructionText();
    }

    #endregion

    #region Game Start

    private bool AllPlayersReady()
    {
        if (players.Count == 0) return false;
        for (int i = 0; i < players.Count; i++)
        {
            if (!slots[i].IsReady)
                return false;
        }
        return true;
    }

    private void StartCountdown()
    {
        state = GameState.Countdown;
        countdownTimer = countdownTime;

        _joinInputAction?.Disable();
        inputManager.DisableJoining();

        if (countdownText != null)
            countdownText.gameObject.SetActive(true);
    }

    private void BeginGame()
    {
        state = GameState.Playing;

        if (joinCanvas != null)
            joinCanvas.SetActive(false);

        if (timerScript != null)
            timerScript.tick = true;

        for (int i = 0; i < players.Count; i++)
        {
            PlayerInput pi = players[i];
            CharacterData chosenChar = slots[i].SelectedCharacter;

            if (pi == null) continue;

            // Teleport to spawn
            if (spawnPoints != null && i < spawnPoints.Count && spawnPoints[i] != null)
            {
                pi.transform.SetPositionAndRotation(
                    spawnPoints[i].position,
                    spawnPoints[i].rotation
                );
            }

            // Show player model
            if (pi.TryGetComponent<HideUntilGameStart>(out var hider))
                hider.Show();

            // Disable lobby input, enable gameplay input
            if (pi.TryGetComponent<SlotInputHandler>(out var slotHandler))
                slotHandler.enabled = false;

            if (pi.TryGetComponent<moveTEST>(out var mover))
                mover.enabled = true;

            // Spawn character visual
            if (chosenChar != null && chosenChar.playerPrefab != null)
            {
                GameObject skin = Instantiate(
                    chosenChar.playerPrefab,
                    pi.transform
                );
                skin.transform.localPosition = Vector3.zero;
                skin.transform.localRotation = Quaternion.identity;
            }

            Debug.Log($"[MJM] Player {i + 1} started as {chosenChar?.characterName ?? "unknown"}");
        }
    }

    #endregion

    #region UI

    private void UpdateInstructionText()
    {
        if (instructionText == null) return;

        if (state == GameState.Joining)
        {
            instructionText.text = players.Count switch
            {
                0 => "PRESS ANY BUTTON TO JOIN",
                int n when n < maxPlayers => $"{n} PLAYER(S) JOINED — PRESS A BUTTON TO JOIN",
                _ => "MAX PLAYERS — GET READY"
            };
        }
        else if (state == GameState.CharacterSelect)
        {
            instructionText.text = "SELECT CHARACTER — PRESS SUBMIT TO READY";
        }
        else
        {
            instructionText.text = "";
        }
    }

    private void BuildUI()
    {
        var canvasObj = new GameObject("JoinCanvas");
        canvasObj.transform.SetParent(transform, false);
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        joinCanvas = canvasObj;

        // Background
        CreateStretchedImage(canvasObj.transform, "Background", new Color(0.08f, 0.08f, 0.08f))
            .rectTransform.SetAsFirstSibling();

        // Player slots
        for (int i = 0; i < maxPlayers; i++)
            slots.Add(BuildSlot(canvasObj.transform, i));

        // Instruction text
        instructionText = CreateText(
            canvasObj.transform, "InstructionText",
            "PRESS ANY BUTTON TO JOIN", 36, Color.white, TextAnchor.UpperCenter
        );
        SetAnchors(instructionText.rectTransform,
            new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, new Vector2(0, -80));

        // Start button (manual start if auto fails)
        startButton = CreateButton(
            canvasObj.transform, "StartButton", "START", 20,
            new Color(0.15f, 0.65f, 0.20f)
        );
        var sRT = startButton.GetComponent<RectTransform>();
        sRT.anchorMin = new Vector2(0.5f, 0);
        sRT.anchorMax = new Vector2(0.5f, 0);
        sRT.pivot = new Vector2(0.5f, 0);
        sRT.anchoredPosition = new Vector2(0, 40);
        sRT.sizeDelta = new Vector2(180, 50);
        startButton.interactable = false;
        startButton.onClick.AddListener(StartCountdown);

        // Countdown text
        countdownText = CreateText(
            canvasObj.transform, "CountdownText", "", 60,
            Color.yellow, TextAnchor.MiddleCenter
        );
        var cRT = countdownText.rectTransform;
        cRT.anchorMin = new Vector2(0.5f, 0.5f);
        cRT.anchorMax = new Vector2(0.5f, 0.5f);
        cRT.pivot = new Vector2(0.5f, 0.5f);
        cRT.anchoredPosition = Vector2.zero;
        cRT.sizeDelta = new Vector2(600, 100);
        countdownText.gameObject.SetActive(false);
    }

    private PlayerSlotUI BuildSlot(Transform parent, int index)
    {
        Color teamColor = TeamColors[index];

        var slotObj = new GameObject($"PlayerSlot_{index}");
        slotObj.transform.SetParent(parent, false);

        var bg = slotObj.AddComponent<Image>();
        bg.color = new Color(0.18f, 0.18f, 0.18f);

        var slotRT = slotObj.GetComponent<RectTransform>();
        slotRT.anchorMin = new Vector2(0.5f, 0.5f);
        slotRT.anchorMax = new Vector2(0.5f, 0.5f);
        slotRT.pivot = new Vector2(0.5f, 0.5f);
        slotRT.anchoredPosition = SlotPositions[index];
        slotRT.sizeDelta = new Vector2(380, 310);

        var slot = slotObj.AddComponent<PlayerSlotUI>();
        slot.PlayerIndex = index;
        slot.Characters = new List<CharacterData>(characters);

        // Header bar
        var headerObj = new GameObject("HeaderBar");
        headerObj.transform.SetParent(slotObj.transform, false);
        var headerImg = headerObj.AddComponent<Image>();
        headerImg.color = teamColor;
        var hRT = headerObj.GetComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0, 1);
        hRT.anchorMax = new Vector2(1, 1);
        hRT.pivot = new Vector2(0.5f, 1);
        hRT.anchoredPosition = Vector2.zero;
        hRT.sizeDelta = new Vector2(0, 8);
        slot.HeaderBar = headerImg;

        // Player label
        var lbl = CreateText(
            slotObj.transform, "PlayerLabel", "WAITING...",
            16, teamColor, TextAnchor.MiddleLeft
        );
        var lblRT = lbl.rectTransform;
        lblRT.anchorMin = new Vector2(0, 1);
        lblRT.anchorMax = new Vector2(1, 1);
        lblRT.pivot = new Vector2(0, 1);
        lblRT.anchoredPosition = new Vector2(12, -10);
        lblRT.sizeDelta = new Vector2(-12, 28);
        slot.PlayerLabel = lbl;

        // Preview image
        var previewObj = new GameObject("PreviewImage");
        previewObj.transform.SetParent(slotObj.transform, false);
        var previewImg = previewObj.AddComponent<Image>();
        previewImg.color = new Color(0.12f, 0.12f, 0.12f);
        previewImg.preserveAspect = true;
        var pRT = previewObj.GetComponent<RectTransform>();
        pRT.anchorMin = new Vector2(0.5f, 1);
        pRT.anchorMax = new Vector2(0.5f, 1);
        pRT.pivot = new Vector2(0.5f, 1);
        pRT.anchoredPosition = new Vector2(0, -46);
        pRT.sizeDelta = new Vector2(160, 160);
        previewImg.gameObject.SetActive(false);
        slot.PreviewImage = previewImg;

        // Character name
        var charName = CreateText(
            slotObj.transform, "CharacterName", "Press button to join",
            20, Color.white, TextAnchor.MiddleCenter
        );
        var cnRT = charName.rectTransform;
        cnRT.anchorMin = new Vector2(0, 0.5f);
        cnRT.anchorMax = new Vector2(1, 0.5f);
        cnRT.pivot = new Vector2(0.5f, 0.5f);
        cnRT.anchoredPosition = new Vector2(0, 22);
        cnRT.sizeDelta = new Vector2(-20, 32);
        slot.CharacterNameText = charName;

        // Left arrow
        var leftBtn = CreateArrowButton(
            slotObj.transform, "LeftArrow", "<",
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 0.5f), new Vector2(40, 70)
        );
        slot.LeftArrow = leftBtn;
        leftBtn.onClick.AddListener(slot.SelectPrevious);

        // Right arrow
        var rightBtn = CreateArrowButton(
            slotObj.transform, "RightArrow", ">",
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(0.5f, 0.5f), new Vector2(-40, 70)
        );
        slot.RightArrow = rightBtn;
        rightBtn.onClick.AddListener(slot.SelectNext);

        // Ready button
        var readyBtn = CreateButton(
            slotObj.transform, "ReadyButton", "READY", 16, teamColor
        );
        var rdyRT = readyBtn.GetComponent<RectTransform>();
        rdyRT.anchorMin = new Vector2(0, 0);
        rdyRT.anchorMax = new Vector2(1, 0);
        rdyRT.pivot = new Vector2(0.5f, 0);
        rdyRT.anchoredPosition = new Vector2(0, 52);
        rdyRT.sizeDelta = new Vector2(-200, 36);
        readyBtn.onClick.AddListener(slot.ToggleReady);
        readyBtn.gameObject.SetActive(false);
        slot.ReadyButton = readyBtn;

        // Ready indicator
        var readyInd = CreateText(
            slotObj.transform, "ReadyIndicator", "READY", 18,
            Color.green, TextAnchor.MiddleCenter
        );
        var riRT = readyInd.rectTransform;
        riRT.anchorMin = new Vector2(0, 0);
        riRT.anchorMax = new Vector2(1, 0);
        riRT.pivot = new Vector2(0.5f, 0.5f);
        riRT.anchoredPosition = new Vector2(0, 69);
        riRT.sizeDelta = new Vector2(0, 30);
        readyInd.gameObject.SetActive(false);
        slot.ReadyIndicator = readyInd;

        return slot;
    }

    private static Image CreateStretchedImage(Transform parent, string name, Color color)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var img = obj.AddComponent<Image>();
        img.color = color;
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return img;
    }

    private static Text CreateText(Transform parent, string name, string content,
        int fontSize, Color color, TextAnchor alignment)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var txt = obj.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.text = content;
        txt.fontSize = fontSize;
        txt.color = color;
        txt.alignment = alignment;
        return txt;
    }

    private static Button CreateButton(Transform parent, string name, string label,
        int fontSize, Color bgColor)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var img = obj.AddComponent<Image>();
        img.color = bgColor;
        var btn = obj.AddComponent<Button>();
        btn.targetGraphic = img;

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(obj.transform, false);
        var txt = textObj.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.text = label;
        txt.fontSize = fontSize;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;

        var txtRT = txt.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;

        return btn;
    }

    private static Button CreateArrowButton(Transform parent, string name, string symbol,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position)
    {
        var btn = CreateButton(parent, name, symbol, 22, new Color(0.35f, 0.35f, 0.35f));
        var rt = btn.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(44, 44);
        return btn;
    }

    private static void SetAnchors(RectTransform rt,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }

    #endregion
}

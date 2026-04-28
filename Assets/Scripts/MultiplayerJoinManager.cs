using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Lobby manager — no PlayerInputManager, no placeholder spawning.
/// Tracks devices only during lobby. Spawns real player prefabs at game start.
///
/// SETUP:
///  • No PlayerInputManager needed in the scene
///  • Assign: playerPrefab, spawnPoints, characters, timerScript
///  • Player prefab needs: PlayerInput, moveTEST, CharacterController
/// </summary>
public class MultiplayerJoinManager : MonoBehaviour
{
    [Header("Required")]
    public List<CharacterData> characters;
    public timer timerScript;

    [Header("Spawn Points")]
    public List<Transform> spawnPoints;

    [Header("Settings")]
    public int maxPlayers = 4;
    public float countdownTime = 3f;

    // ── Internal lobby record — one per joined device ─────────────────────────
    private class LobbyPlayer
    {
        public InputDevice device;
        public int characterIndex;
        public bool isReady;
    }

    private readonly List<LobbyPlayer> _lobbyPlayers = new();
    private readonly List<PlayerSlotUI> _slots = new();

    private readonly Dictionary<InputDevice, float> _deviceCooldowns = new();
    private const float DeviceCooldown = 0.8f;
    private const float JoinDebounceTime = 0.4f;
    private float _joinDebounce;

    private enum LobbyState { Joining, CharacterSelect, Countdown, Playing }
    private LobbyState _state = LobbyState.Joining;

    private float _countdownTimer;
    private Text _instructionText;
    private Text _countdownText;
    private Button _startButton;
    private GameObject _joinCanvas;

    private InputAction _joinAction;

    private readonly float[] _navTimers = new float[4];

    private static readonly Color[] TeamColors =
    {
        new(0.85f, 0.15f, 0.15f), new(0.15f, 0.45f, 0.85f),
        new(0.15f, 0.75f, 0.25f), new(0.85f, 0.65f, 0.10f)
    };

    private static readonly Vector2[] SlotPositions =
    {
        new(-400, 200), new(400, 200), new(-400, -200), new(400, -200)
    };

    // ─────────────────────────────────────────────────────────────────────────
    #region Unity lifecycle

    private void Start()
    {
        if (characters == null || characters.Count == 0)
        { Debug.LogError("[MJM] No characters assigned!"); return; }

        BuildUI();
        SetupJoinAction();
        RefreshInstructionText();
    }

    private void OnDestroy()
    {
        _joinAction?.Disable();
        _joinAction?.Dispose();
    }

    private void Update()
    {
        // Tick per-device cooldowns
        var keys = new List<InputDevice>(_deviceCooldowns.Keys);
        foreach (var d in keys)
        {
            _deviceCooldowns[d] -= Time.deltaTime;
            if (_deviceCooldowns[d] <= 0f) _deviceCooldowns.Remove(d);
        }

        if (_joinDebounce > 0f) _joinDebounce -= Time.deltaTime;

        if (_state == LobbyState.CharacterSelect)
            PollLobbyInput();

        // Auto-start when all players ready
        if (_state == LobbyState.CharacterSelect && AllPlayersReady())
            StartCountdown();

        if (_state == LobbyState.Countdown)
        {
            _countdownTimer -= Time.deltaTime;
            if (_countdownText != null)
                _countdownText.text = $"Starting in {Mathf.CeilToInt(_countdownTimer)}...";
            if (_countdownTimer <= 0f)
                BeginGame();
        }

        if (_startButton != null)
            _startButton.interactable = AllPlayersReady();
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Join handling

    private void SetupJoinAction()
    {
        // Only gamepad buttons and keyboard keys — avoids phantom joins from
        // mouse clicks and UI pointer events that "*/<Button>" would catch
        _joinAction = new InputAction("Join");
        _joinAction.AddBinding("<Gamepad>/<Button>");
        _joinAction.AddBinding("<Keyboard>/<Key>");
        _joinAction.performed += OnJoinPressed;
        _joinAction.Enable();
    }

    private void OnJoinPressed(InputAction.CallbackContext ctx)
    {
        if (_state == LobbyState.Countdown || _state == LobbyState.Playing) return;
        if (_lobbyPlayers.Count >= maxPlayers) return;
        if (_joinDebounce > 0f) return;

        InputDevice device = ctx.control.device;

        // Already joined?
        foreach (var p in _lobbyPlayers)
            if (p.device == device) return;

        // Held-button spam guard
        if (_deviceCooldowns.ContainsKey(device)) return;
        _deviceCooldowns[device] = DeviceCooldown;

        // Register device — no prefab spawned yet
        var lp = new LobbyPlayer { device = device, characterIndex = 0, isReady = false };
        _lobbyPlayers.Add(lp);

        _slots[_lobbyPlayers.Count - 1].AssignPlayerDevice(0, characters);

        _state = LobbyState.CharacterSelect;
        _joinDebounce = JoinDebounceTime;

        RefreshInstructionText();
        Debug.Log($"[MJM] Player {_lobbyPlayers.Count} joined — {device.displayName}");
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Lobby input polling

    private void PollLobbyInput()
    {
        for (int i = 0; i < _lobbyPlayers.Count; i++)
        {
            LobbyPlayer lp = _lobbyPlayers[i];
            PlayerSlotUI slot = _slots[i];

            _navTimers[i] -= Time.deltaTime;

            float navX = 0f;
            bool submit = false;
            bool cancel = false;

            if (lp.device is Gamepad gp)
            {
                navX = gp.leftStick.x.ReadValue();
                if (Mathf.Abs(navX) < 0.3f) navX = gp.dpad.x.ReadValue();
                submit = gp.buttonSouth.wasPressedThisFrame;
                cancel = gp.buttonEast.wasPressedThisFrame;
            }
            else if (lp.device is Keyboard kb)
            {
                if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) navX = -1f;
                if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) navX = 1f;
                submit = kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame;
                cancel = kb.escapeKey.wasPressedThisFrame;
            }

            // Navigate character select
            if (navX < -0.5f && _navTimers[i] <= 0f)
            {
                lp.characterIndex = Mod(lp.characterIndex - 1, characters.Count);
                slot.SetCharacterIndex(lp.characterIndex);
                _navTimers[i] = 0.25f;
            }
            else if (navX > 0.5f && _navTimers[i] <= 0f)
            {
                lp.characterIndex = Mod(lp.characterIndex + 1, characters.Count);
                slot.SetCharacterIndex(lp.characterIndex);
                _navTimers[i] = 0.25f;
            }
            else if (Mathf.Abs(navX) < 0.3f)
            {
                _navTimers[i] = 0f; // reset so next press fires immediately
            }

            // Ready toggle
            if (submit)
            {
                lp.isReady = !lp.isReady;
                slot.SetReady(lp.isReady);
            }

            // Un-ready only
            if (cancel && lp.isReady)
            {
                lp.isReady = false;
                slot.SetReady(false);
            }
        }
    }

    private static int Mod(int x, int m) => (x % m + m) % m;

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Game start

    private bool AllPlayersReady()
    {
        if (_lobbyPlayers.Count == 0) return false;
        foreach (var p in _lobbyPlayers)
            if (!p.isReady) return false;
        return true;
    }

    private void StartCountdown()
    {
        if (_state == LobbyState.Countdown || _state == LobbyState.Playing) return;

        _state = LobbyState.Countdown;
        _countdownTimer = countdownTime;
        _joinAction?.Disable();

        if (_countdownText != null) _countdownText.gameObject.SetActive(true);
    }

    private void BeginGame()
    {
        _state = LobbyState.Playing;
        if (_joinCanvas != null) _joinCanvas.SetActive(false);
        if (timerScript != null) timerScript.tick = true;

        for (int i = 0; i < _lobbyPlayers.Count; i++)
        {
            LobbyPlayer lp = _lobbyPlayers[i];
            CharacterData cd = characters[lp.characterIndex];

            Vector3 pos = spawnPoints != null && i < spawnPoints.Count && spawnPoints[i] != null
                             ? spawnPoints[i].position : Vector3.zero;
            Quaternion rot = spawnPoints != null && i < spawnPoints.Count && spawnPoints[i] != null
                             ? spawnPoints[i].rotation : Quaternion.identity;

            // Spawn the selected character prefab directly — it already has
            // PlayerInput, moveTEST, CharacterController etc.
            GameObject playerObj = Instantiate(cd.playerPrefab, pos, rot);

            // Bind this player's device to their PlayerInput
            if (playerObj.TryGetComponent<PlayerInput>(out var pi))
                pi.SwitchCurrentControlScheme(lp.device);
            else
                Debug.LogWarning($"[MJM] Character prefab missing PlayerInput.");

            Debug.Log($"[MJM] Player {i + 1} spawned as {cd.characterName} at {pos}");
        }
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region UI

    private void RefreshInstructionText()
    {
        if (_instructionText == null) return;
        _instructionText.text = _state switch
        {
            LobbyState.Joining => _lobbyPlayers.Count == 0
                                          ? "PRESS ANY BUTTON TO JOIN"
                                          : $"{_lobbyPlayers.Count} PLAYER(S) — PRESS TO JOIN",
            LobbyState.CharacterSelect => "SELECT CHARACTER — PRESS SUBMIT TO READY UP",
            _ => ""
        };
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
        _joinCanvas = canvasObj;

        CreateStretchedImage(canvasObj.transform, "Background", new Color(0.08f, 0.08f, 0.08f))
            .rectTransform.SetAsFirstSibling();

        for (int i = 0; i < maxPlayers; i++)
            _slots.Add(BuildSlot(canvasObj.transform, i));

        _instructionText = CreateText(canvasObj.transform, "InstructionText",
            "PRESS ANY BUTTON TO JOIN", 36, Color.white, TextAnchor.UpperCenter);
        SetAnchors(_instructionText.rectTransform,
            new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, new Vector2(0, -80));

        _startButton = CreateButton(canvasObj.transform, "StartButton",
            "START", 20, new Color(0.15f, 0.65f, 0.20f));
        var sRT = _startButton.GetComponent<RectTransform>();
        sRT.anchorMin = new Vector2(0.5f, 0); sRT.anchorMax = new Vector2(0.5f, 0);
        sRT.pivot = new Vector2(0.5f, 0); sRT.anchoredPosition = new Vector2(0, 40);
        sRT.sizeDelta = new Vector2(180, 50);
        _startButton.interactable = false;
        _startButton.onClick.AddListener(StartCountdown);

        _countdownText = CreateText(canvasObj.transform, "CountdownText",
            "", 60, Color.yellow, TextAnchor.MiddleCenter);
        var cRT = _countdownText.rectTransform;
        cRT.anchorMin = new Vector2(0.5f, 0.5f); cRT.anchorMax = new Vector2(0.5f, 0.5f);
        cRT.pivot = new Vector2(0.5f, 0.5f); cRT.anchoredPosition = Vector2.zero;
        cRT.sizeDelta = new Vector2(600, 100);
        _countdownText.gameObject.SetActive(false);
    }

    private PlayerSlotUI BuildSlot(Transform parent, int index)
    {
        Color tc = TeamColors[index];

        var slotObj = new GameObject($"PlayerSlot_{index}");
        slotObj.transform.SetParent(parent, false);
        var bg = slotObj.AddComponent<Image>(); bg.color = new Color(0.18f, 0.18f, 0.18f);
        var slotRT = slotObj.GetComponent<RectTransform>();
        slotRT.anchorMin = new Vector2(0.5f, 0.5f); slotRT.anchorMax = new Vector2(0.5f, 0.5f);
        slotRT.pivot = new Vector2(0.5f, 0.5f);
        slotRT.anchoredPosition = SlotPositions[index]; slotRT.sizeDelta = new Vector2(380, 310);

        var slot = slotObj.AddComponent<PlayerSlotUI>();
        slot.PlayerIndex = index;
        slot.Characters = new List<CharacterData>(characters);

        // Header bar
        var hObj = new GameObject("HeaderBar"); hObj.transform.SetParent(slotObj.transform, false);
        var hImg = hObj.AddComponent<Image>(); hImg.color = tc;
        var hRT = hObj.GetComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0, 1); hRT.anchorMax = new Vector2(1, 1);
        hRT.pivot = new Vector2(0.5f, 1); hRT.anchoredPosition = Vector2.zero; hRT.sizeDelta = new Vector2(0, 8);
        slot.HeaderBar = hImg;

        // Player label
        var lbl = CreateText(slotObj.transform, "PlayerLabel", "WAITING...", 16, tc, TextAnchor.MiddleLeft);
        var lblRT = lbl.rectTransform;
        lblRT.anchorMin = new Vector2(0, 1); lblRT.anchorMax = new Vector2(1, 1);
        lblRT.pivot = new Vector2(0, 1); lblRT.anchoredPosition = new Vector2(12, -10); lblRT.sizeDelta = new Vector2(-12, 28);
        slot.PlayerLabel = lbl;

        // Portrait
        var pObj = new GameObject("PortraitImage"); pObj.transform.SetParent(slotObj.transform, false);
        var pImg = pObj.AddComponent<Image>();
        pImg.color = new Color(0.12f, 0.12f, 0.12f); pImg.preserveAspect = true;
        var pRT = pObj.GetComponent<RectTransform>();
        pRT.anchorMin = new Vector2(0.5f, 1); pRT.anchorMax = new Vector2(0.5f, 1);
        pRT.pivot = new Vector2(0.5f, 1); pRT.anchoredPosition = new Vector2(0, -46); pRT.sizeDelta = new Vector2(160, 160);
        pImg.gameObject.SetActive(false); slot.PreviewImage = pImg;

        // Character name
        var cn = CreateText(slotObj.transform, "CharacterName", "Press button to join", 20, Color.white, TextAnchor.MiddleCenter);
        var cnRT = cn.rectTransform;
        cnRT.anchorMin = new Vector2(0, 0.5f); cnRT.anchorMax = new Vector2(1, 0.5f);
        cnRT.pivot = new Vector2(0.5f, 0.5f); cnRT.anchoredPosition = new Vector2(0, 22); cnRT.sizeDelta = new Vector2(-20, 32);
        slot.CharacterNameText = cn;

        // Arrows
        var lb = CreateArrowButton(slotObj.transform, "LeftArrow", "<",
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 0.5f), new Vector2(40, 70));
        slot.LeftArrow = lb; lb.onClick.AddListener(slot.SelectPrevious);

        var rb = CreateArrowButton(slotObj.transform, "RightArrow", ">",
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(0.5f, 0.5f), new Vector2(-40, 70));
        slot.RightArrow = rb; rb.onClick.AddListener(slot.SelectNext);

        // Ready button
        var rdy = CreateButton(slotObj.transform, "ReadyButton", "READY", 16, tc);
        var rdyRT = rdy.GetComponent<RectTransform>();
        rdyRT.anchorMin = new Vector2(0, 0); rdyRT.anchorMax = new Vector2(1, 0);
        rdyRT.pivot = new Vector2(0.5f, 0); rdyRT.anchoredPosition = new Vector2(0, 52); rdyRT.sizeDelta = new Vector2(-200, 36);
        rdy.onClick.AddListener(slot.ToggleReady); rdy.gameObject.SetActive(false);
        slot.ReadyButton = rdy;

        // Ready indicator
        var ri = CreateText(slotObj.transform, "ReadyIndicator", "✔ READY", 18, Color.green, TextAnchor.MiddleCenter);
        var riRT = ri.rectTransform;
        riRT.anchorMin = new Vector2(0, 0); riRT.anchorMax = new Vector2(1, 0);
        riRT.pivot = new Vector2(0.5f, 0); riRT.anchoredPosition = new Vector2(0, 69); riRT.sizeDelta = new Vector2(0, 30);
        ri.gameObject.SetActive(false); slot.ReadyIndicator = ri;

        return slot;
    }

    // ── Factory helpers ───────────────────────────────────────────────────────

    private static Image CreateStretchedImage(Transform p, string n, Color c)
    {
        var o = new GameObject(n); o.transform.SetParent(p, false);
        var img = o.AddComponent<Image>(); img.color = c;
        var rt = o.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return img;
    }

    private static Text CreateText(Transform p, string n, string content, int size, Color c, TextAnchor a)
    {
        var o = new GameObject(n); o.transform.SetParent(p, false);
        var t = o.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text = content; t.fontSize = size; t.color = c; t.alignment = a;
        return t;
    }

    private static Button CreateButton(Transform p, string n, string label, int size, Color bg)
    {
        var o = new GameObject(n); o.transform.SetParent(p, false);
        var img = o.AddComponent<Image>(); img.color = bg;
        var btn = o.AddComponent<Button>(); btn.targetGraphic = img;
        var to = new GameObject("Text"); to.transform.SetParent(o.transform, false);
        var t = to.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text = label; t.fontSize = size; t.color = Color.white; t.alignment = TextAnchor.MiddleCenter;
        var trt = to.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
        return btn;
    }

    private static Button CreateArrowButton(Transform p, string n, string sym,
        Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos)
    {
        var btn = CreateButton(p, n, sym, 22, new Color(0.35f, 0.35f, 0.35f));
        var rt = btn.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = pivot;
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(44, 44);
        return btn;
    }

    private static void SetAnchors(RectTransform rt,
        Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax)
    { rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = oMin; rt.offsetMax = oMax; }

    #endregion
}
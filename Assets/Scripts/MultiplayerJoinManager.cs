using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class MultiplayerJoinManager : MonoBehaviour
{
    [Header("Setup (fill these in Inspector)")]
    public List<CharacterData> characters;
    public PlayerInputManager inputManager;
    public timer timerScript;

    [Header("Player Prefab")]
    public GameObject playerPrefab;

    [Header("Settings")]
    public int maxPlayers = 4;
    public float countdownTime = 3f;

    private Canvas canvas;
    private List<PlayerSlotUI> slots = new List<PlayerSlotUI>();
    private Text instructionText;
    private Button startButton;
    private Text countdownText;

    private int joinedCount = 0;
    private float countdownTimer = 0f;
    private bool countingDown = false;
    private GameObject joinCanvas;

    public void Start()
    {
        if (characters == null || characters.Count == 0)
        {
            Debug.LogError("MultiplayerJoinManager: No characters assigned!");
            return;
        }

        CreateJoinScreen();

        if (inputManager != null)
        {
            inputManager.onPlayerJoined += OnPlayerJoined;
            inputManager.onPlayerLeft += OnPlayerLeft;
        }
    }

    private void OnDestroy()
    {
        if (inputManager != null)
        {
            inputManager.onPlayerJoined -= OnPlayerJoined;
            inputManager.onPlayerLeft -= OnPlayerLeft;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            OnStartPressed();

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

        if (startButton != null)
            startButton.interactable = (joinedCount > 0 && AllReady());
    }

    private void CreateJoinScreen()
    {
        GameObject canvasObj = new GameObject("JoinCanvas");
        canvasObj.transform.SetParent(transform, false);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        joinCanvas = canvasObj;

        Color[] teamColors = new Color[]
        {
            new Color(0.85f, 0.15f, 0.15f),
            new Color(0.15f, 0.45f, 0.85f),
            new Color(0.15f, 0.75f, 0.25f),
            new Color(0.85f, 0.65f, 0.10f)
        };

        Vector2[] positions = new Vector2[]
        {
            new Vector2(-320, 100),
            new Vector2(320, 100),
            new Vector2(-320, -150),
            new Vector2(320, -150)
        };

        for (int i = 0; i < maxPlayers; i++)
        {
            GameObject slotObj = new GameObject("PlayerSlot_" + i);
            slotObj.transform.SetParent(canvasObj.transform, false);

            Image bg = slotObj.AddComponent<Image>();
            bg.color = new Color(0.25f, 0.25f, 0.25f, 1f);

            RectTransform slotRT = slotObj.AddComponent<RectTransform>();
            slotRT.anchorMin = new Vector2(0.5f, 0.5f);
            slotRT.anchorMax = new Vector2(0.5f, 0.5f);
            slotRT.pivot = new Vector2(0.5f, 0.5f);
            slotRT.anchoredPosition = positions[i];
            slotRT.sizeDelta = new Vector2(350, 220);

            PlayerSlotUI slot = slotObj.AddComponent<PlayerSlotUI>();
            slot.playerIndex = i;
            slot.characters = new List<CharacterData>(characters);
            slots.Add(slot);

            GameObject header = new GameObject("HeaderBar");
            header.transform.SetParent(slotObj.transform, false);
            Image headerImg = header.AddComponent<Image>();
            headerImg.color = teamColors[i];
            slot.SetHeaderBar(headerImg);

            RectTransform headerRT = header.GetComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0, 1);
            headerRT.anchorMax = new Vector2(1, 1);
            headerRT.offsetMin = Vector2.zero;
            headerRT.offsetMax = Vector2.zero;
            headerRT.sizeDelta = new Vector2(0, 6);

            GameObject labelObj = new GameObject("PlayerLabel");
            labelObj.transform.SetParent(slotObj.transform, false);
            Text label = labelObj.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text = "WAITING...";
            label.alignment = TextAnchor.UpperLeft;
            label.color = teamColors[i];
            label.fontSize = 18;
            slot.SetPlayerLabel(label);

            RectTransform labelRT = labelObj.GetComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 1);
            labelRT.anchorMax = new Vector2(1, 1);
            labelRT.offsetMin = new Vector2(15, -30);
            labelRT.offsetMax = new Vector2(-15, -5);
            labelRT.sizeDelta = new Vector2(0, 25);

            GameObject charNameObj = new GameObject("CharacterName");
            charNameObj.transform.SetParent(slotObj.transform, false);
            Text charName = charNameObj.AddComponent<Text>();
            charName.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            charName.text = "Press button to join";
            charName.alignment = TextAnchor.MiddleCenter;
            charName.color = Color.white;
            charName.fontSize = 22;
            slot.SetCharacterNameText(charName);

            RectTransform charRT = charNameObj.GetComponent<RectTransform>();
            charRT.anchorMin = new Vector2(0, 0.5f);
            charRT.anchorMax = new Vector2(1, 0.5f);
            charRT.offsetMin = new Vector2(20, -20);
            charRT.offsetMax = new Vector2(-20, 20);
            charRT.sizeDelta = new Vector2(0, 40);

            GameObject leftBtn = CreateArrowButton(slotObj, "LeftArrow", new Vector2(-120, 0), "<");
            slot.SetLeftArrow(leftBtn.GetComponent<Button>());

            GameObject rightBtn = CreateArrowButton(slotObj, "RightArrow", new Vector2(120, 0), ">");
            slot.SetRightArrow(rightBtn.GetComponent<Button>());

            GameObject readyObj = new GameObject("ReadyButton");
            readyObj.transform.SetParent(slotObj.transform, false);
            Image readyImg = readyObj.AddComponent<Image>();
            readyImg.color = teamColors[i];
            Button readyB = readyObj.AddComponent<Button>();
            readyB.targetGraphic = readyImg;
            slot.SetReadyButton(readyB);

            RectTransform readyRT = readyObj.GetComponent<RectTransform>();
            readyRT.anchorMin = new Vector2(0, 0);
            readyRT.anchorMax = new Vector2(1, 0);
            readyRT.offsetMin = new Vector2(30, 15);
            readyRT.offsetMax = new Vector2(-30, 40);
            readyRT.sizeDelta = new Vector2(0, 25);

            GameObject readyTextObj = new GameObject("ReadyText");
            readyTextObj.transform.SetParent(readyObj.transform, false);
            Text readyT = readyTextObj.AddComponent<Text>();
            readyT.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            readyT.text = "READY";
            readyT.alignment = TextAnchor.MiddleCenter;
            readyT.color = Color.white;
            readyT.fontSize = 16;
            slot.SetReadyText(readyT);

            RectTransform readyTRT = readyTextObj.GetComponent<RectTransform>();
            readyTRT.anchorMin = Vector2.zero;
            readyTRT.anchorMax = Vector2.one;
            readyTRT.offsetMin = Vector2.zero;
            readyTRT.offsetMax = Vector2.zero;
        }

        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.08f, 0.08f, 0.08f, 1f);
        RectTransform bgRT = bgObj.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        bgRT.SetAsFirstSibling();

        GameObject instrObj = new GameObject("InstructionText");
        instrObj.transform.SetParent(canvasObj.transform, false);
        instructionText = instrObj.AddComponent<Text>();
        instructionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        instructionText.text = "PRESS ANY BUTTON TO JOIN";
        instructionText.alignment = TextAnchor.UpperCenter;
        instructionText.color = Color.white;
        instructionText.fontSize = 36;

        RectTransform instrRT = instrObj.GetComponent<RectTransform>();
        instrRT.anchorMin = new Vector2(0, 1);
        instrRT.anchorMax = new Vector2(1, 1);
        instrRT.pivot = new Vector2(0.5f, 1);
        instrRT.anchoredPosition = Vector2.zero;
        instrRT.sizeDelta = new Vector2(0, 80);

        GameObject startObj = new GameObject("StartButton");
        startObj.transform.SetParent(canvasObj.transform, false);
        Image startImg = startObj.AddComponent<Image>();
        startImg.color = new Color(0.15f, 0.65f, 0.20f, 1f);
        startButton = startObj.AddComponent<Button>();
        startButton.targetGraphic = startImg;
        startButton.interactable = false;

        RectTransform startRT = startObj.GetComponent<RectTransform>();
        startRT.anchorMin = new Vector2(0.5f, 0);
        startRT.anchorMax = new Vector2(0.5f, 0);
        startRT.pivot = new Vector2(0.5f, 0);
        startRT.anchoredPosition = new Vector2(0, 40);
        startRT.sizeDelta = new Vector2(180, 50);

        GameObject startTextObj = new GameObject("Text");
        startTextObj.transform.SetParent(startObj.transform, false);
        Text startT = startTextObj.AddComponent<Text>();
        startT.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        startT.text = "START";
        startT.alignment = TextAnchor.MiddleCenter;
        startT.color = Color.white;
        startT.fontSize = 20;

        RectTransform startTRT = startTextObj.GetComponent<RectTransform>();
        startTRT.anchorMin = Vector2.zero;
        startTRT.anchorMax = Vector2.one;
        startTRT.offsetMin = Vector2.zero;
        startTRT.offsetMax = Vector2.zero;

        GameObject countObj = new GameObject("CountdownText");
        countObj.transform.SetParent(canvasObj.transform, false);
        countdownText = countObj.AddComponent<Text>();
        countdownText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        countdownText.text = "STARTING...";
        countdownText.alignment = TextAnchor.MiddleCenter;
        countdownText.color = Color.yellow;
        countdownText.fontSize = 60;

        RectTransform countRT = countObj.GetComponent<RectTransform>();
        countRT.anchorMin = new Vector2(0.5f, 0.5f);
        countRT.anchorMax = new Vector2(0.5f, 0.5f);
        countRT.pivot = new Vector2(0.5f, 0.5f);
        countRT.anchoredPosition = Vector2.zero;
        countRT.sizeDelta = new Vector2(400, 80);
        countdownText.gameObject.SetActive(false);
    }

    private GameObject CreateArrowButton(Transform parent, string name, Vector2 pos, string symbol)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.45f, 0.45f, 0.45f, 1f);
        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(45, 45);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        Text txt = textObj.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.text = symbol;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.fontSize = 22;

        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        return btnObj;
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        if (joinedCount >= maxPlayers) return;

        PlayerSlotUI slot = slots[joinedCount];
        slot.AssignPlayer(playerInput);

        SlotInputHandler handler = playerInput.GetComponent<SlotInputHandler>();
        if (handler != null)
        {
            handler.assignedSlot = slot;
        }
        else
        {
            Debug.LogWarning("Player prefab missing SlotInputHandler component!");
        }

        joinedCount++;
        UpdateInstruction();

        Debug.Log("Player " + joinedCount + " joined (slot " + joinedCount + ")");
    }

    private void OnPlayerLeft(PlayerInput playerInput)
    {
        if (joinedCount <= 0) return;
        joinedCount--;
        slots[joinedCount].UnassignPlayer();
        UpdateInstruction();
    }

    private bool AllReady()
    {
        for (int i = 0; i < joinedCount; i++)
        {
            if (!slots[i].IsReady()) return false;
        }
        return true;
    }

    private void OnStartPressed()
    {
        if (joinedCount == 0) return;

        if (inputManager != null)
            inputManager.DisableJoining();

        if (!countingDown)
        {
            countingDown = true;
            countdownTimer = countdownTime;
            if (countdownText != null)
                countdownText.gameObject.SetActive(true);
        }
    }

    private void BeginGame()
    {
        if (joinCanvas != null)
            joinCanvas.SetActive(false);

        if (timerScript != null)
            timerScript.tick = true;

        Debug.Log("Game started! Players:");
        for (int i = 0; i < joinedCount; i++)
        {
            CharacterData cd = slots[i].GetSelectedCharacter();
            Debug.Log("  Player " + (i + 1) + ": " + (cd != null ? cd.characterName : "default"));
        }
    }

    private void UpdateInstruction()
    {
        if (instructionText == null) return;

        if (joinedCount == 0)
            instructionText.text = "PRESS ANY BUTTON TO JOIN";
        else if (joinedCount < maxPlayers)
            instructionText.text = joinedCount + " PLAYER(S) JOINED - PRESS ANY BUTTON TO JOIN";
        else
            instructionText.text = "MAX PLAYERS REACHED";
    }
}

public class PlayerSlotUI : MonoBehaviour
{
    public int playerIndex = 0;
    public bool isOccupied = false;
    public List<CharacterData> characters = new List<CharacterData>();

    [SerializeField] private Text playerLabel;
    [SerializeField] private Text characterNameText;
    [SerializeField] private Text readyText;
    [SerializeField] private Image characterPreviewImage;
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button rightArrowButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Image headerBar;

    private int selectedCharacterIndex = 0;
    private bool isReady = false;
    private PlayerInput assignedInput = null;

    private Color[] teamColors = new Color[]
    {
        new Color(0.85f, 0.15f, 0.15f),
        new Color(0.15f, 0.45f, 0.85f),
        new Color(0.15f, 0.75f, 0.25f),
        new Color(0.85f, 0.65f, 0.10f)
    };

    private bool lastFrameWasLeft = false;
    private bool lastFrameWasRight = false;

    public void AssignPlayer(PlayerInput input)
    {
        assignedInput = input;
        isOccupied = true;
        isReady = false;
        selectedCharacterIndex = 0;

        Color col = teamColors[Mathf.Clamp(playerIndex, 0, teamColors.Length - 1)];
        if (headerBar != null) headerBar.color = col;
        if (playerLabel != null)
        {
            playerLabel.text = "PLAYER " + (playerIndex + 1);
            playerLabel.color = col;
        }

        RefreshCharacterDisplay();
    }

    public void UnassignPlayer()
    {
        assignedInput = null;
        isOccupied = false;
        isReady = false;

        if (playerLabel != null)
        {
            playerLabel.text = "WAITING...";
            playerLabel.color = teamColors[Mathf.Clamp(playerIndex, 0, teamColors.Length - 1)];
        }
        if (characterNameText != null)
            characterNameText.text = "Press button to join";
    }

    public bool IsReady() => isReady;

    public CharacterData GetSelectedCharacter()
    {
        if (characters == null || characters.Count == 0) return null;
        return characters[selectedCharacterIndex];
    }

    public void SetPlayerLabel(Text t) { playerLabel = t; }
    public void SetCharacterNameText(Text t) { characterNameText = t; }
    public void SetReadyText(Text t) { readyText = t; }
    public void SetCharacterPreviewImage(Image i) { characterPreviewImage = i; }
    public void SetLeftArrow(Button b) { leftArrowButton = b; if (b != null) b.onClick.AddListener(OnPreviousCharacter); }
    public void SetRightArrow(Button b) { rightArrowButton = b; if (b != null) b.onClick.AddListener(OnNextCharacter); }
    public void SetReadyButton(Button b) { readyButton = b; if (b != null) b.onClick.AddListener(OnReadyToggle); }
    public void SetHeaderBar(Image i) { headerBar = i; }

    private void Update()
    {
        if (!isOccupied || assignedInput == null || isReady) return;

        SlotInputHandler handler = assignedInput.GetComponent<SlotInputHandler>();
        if (handler != null) return;

        var moveAction = assignedInput.actions["Move"];
        if (moveAction == null) return;

        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        float horizontal = moveInput.x;

        if (horizontal > 0.5f)
        {
            if (!lastFrameWasRight)
            {
                OnNextCharacter();
                lastFrameWasRight = true;
            }
        }
        else
        {
            lastFrameWasRight = false;
        }

        if (horizontal < -0.5f)
        {
            if (!lastFrameWasLeft)
            {
                OnPreviousCharacter();
                lastFrameWasLeft = true;
            }
        }
        else
        {
            lastFrameWasLeft = false;
        }
    }

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
        if (leftArrowButton != null) leftArrowButton.interactable = !isReady;
        if (rightArrowButton != null) rightArrowButton.interactable = !isReady;
    }
}

public class SlotInputHandler : MonoBehaviour
{
    [HideInInspector] public PlayerSlotUI assignedSlot;
    private PlayerInput playerInput;

    private float navCooldown = 0f;
    private const float NAV_COOLDOWN_TIME = 0.25f;
    private float prevNavX = 0f;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        if (playerInput == null) return;

        var submit = playerInput.actions["Submit"];
        if (submit != null) submit.performed += OnSubmit;

        var cancel = playerInput.actions["Cancel"];
        if (cancel != null) cancel.performed += OnCancel;
    }

    private void OnDisable()
    {
        if (playerInput == null) return;

        var submit = playerInput.actions["Submit"];
        if (submit != null) submit.performed -= OnSubmit;

        var cancel = playerInput.actions["Cancel"];
        if (cancel != null) cancel.performed -= OnCancel;
    }

    private void Update()
    {
        if (assignedSlot == null || playerInput == null) return;

        navCooldown -= Time.deltaTime;

        float navX = 0f;

        var moveAction = playerInput.actions["Move"];
        if (moveAction != null)
        {
            navX = moveAction.ReadValue<Vector2>().x;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            navX = -1f;
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            navX = 1f;

        if (navX < -0.5f && prevNavX >= -0.5f && navCooldown <= 0f)
        {
            assignedSlot.OnPreviousCharacter();
            navCooldown = NAV_COOLDOWN_TIME;
        }
        else if (navX > 0.5f && prevNavX <= 0.5f && navCooldown <= 0f)
        {
            assignedSlot.OnNextCharacter();
            navCooldown = NAV_COOLDOWN_TIME;
        }

        prevNavX = navX;
    }

    private void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (assignedSlot != null)
            assignedSlot.OnReadyToggle();
    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        if (assignedSlot != null && assignedSlot.IsReady())
            assignedSlot.OnReadyToggle();
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class JoinScreenSetup : MonoBehaviour
{
    [Header("Setup")]
    public List<CharacterData> characters;
    public PlayerInputManager inputManager;
    public timer timerScript;

    [Header("Generated")]
    public Canvas canvas;
    public CharacterSelectionManager manager;
    public List<PlayerSlotUI> slots = new List<PlayerSlotUI>();

    public void RunSetup()
    {
        if (characters == null || characters.Count == 0)
        {
            Debug.LogError("JoinScreenSetup: No characters assigned!");
            return;
        }

        CreateCanvas();
        CreateBackground();
        CreateInstructionText();
        CreatePlayerSlots();
        CreateStartButton();
        CreateCountdownText();
        SetupManager();

        Debug.Log("Join screen setup complete!");
    }

    private void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("JoinCanvas");
        canvasObj.transform.SetParent(transform, false);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
    }

    private void CreateBackground()
    {
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(canvas.transform, false);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.08f, 0.08f, 0.08f, 1f);

        RectTransform rt = bg.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void CreateInstructionText()
    {
        GameObject textObj = new GameObject("InstructionText");
        textObj.transform.SetParent(canvas.transform, false);

        Text instructionText = textObj.AddComponent<Text>();
        instructionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        instructionText.text = "PRESS ANY BUTTON TO JOIN";
        instructionText.alignment = TextAnchor.UpperCenter;
        instructionText.color = Color.white;
        instructionText.fontSize = 36;

        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(0, 80);

        manager = gameObject.AddComponent<CharacterSelectionManager>();
        manager.instructionText = instructionText;
    }

    private void CreatePlayerSlots()
    {
        string[] labels = { "PLAYER 1", "PLAYER 2", "PLAYER 3", "PLAYER 4" };
        Color[] teamColors = new Color[]
        {
            new Color(0.85f, 0.15f, 0.15f),
            new Color(0.15f, 0.45f, 0.85f),
            new Color(0.15f, 0.75f, 0.25f),
            new Color(0.85f, 0.65f, 0.10f)
        };

        Vector2[] positions = new Vector2[]
        {
            new Vector2(-200, 80),
            new Vector2(200, 80),
            new Vector2(-200, -180),
            new Vector2(200, -180)
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject slotObj = new GameObject($"PlayerSlot_{i}");
            slotObj.transform.SetParent(canvas.transform, false);

            Image bg = slotObj.AddComponent<Image>();
            bg.color = new Color(0.25f, 0.25f, 0.25f, 1f);

            RectTransform rt = slotObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = positions[i];
            rt.sizeDelta = new Vector2(350, 220);

            PlayerSlotUI slotUI = slotObj.AddComponent<PlayerSlotUI>();
            slotUI.playerIndex = i;
            slotUI.characters = new List<CharacterData>(characters);
            slots.Add(slotUI);

            GameObject headerBar = new GameObject("HeaderBar");
            headerBar.transform.SetParent(slotObj.transform, false);
            Image headerImg = headerBar.AddComponent<Image>();
            headerImg.color = teamColors[i];
            slotUI.SetHeaderBar(headerImg);

            RectTransform headerRT = headerBar.GetComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0, 1);
            headerRT.anchorMax = new Vector2(1, 1);
            headerRT.offsetMin = Vector2.zero;
            headerRT.offsetMax = Vector2.zero;
            headerRT.sizeDelta = new Vector2(0, 6);

            GameObject labelObj = new GameObject("PlayerLabel");
            labelObj.transform.SetParent(slotObj.transform, false);
            Text labelText = labelObj.AddComponent<Text>();
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.text = labels[i];
            labelText.alignment = TextAnchor.UpperLeft;
            labelText.color = teamColors[i];
            labelText.fontSize = 18;
            slotUI.SetPlayerLabel(labelText);

            RectTransform labelRT = labelObj.GetComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 1);
            labelRT.anchorMax = new Vector2(1, 1);
            labelRT.offsetMin = new Vector2(15, -30);
            labelRT.offsetMax = new Vector2(-15, -5);
            labelRT.sizeDelta = new Vector2(0, 25);

            GameObject charNameObj = new GameObject("CharacterName");
            charNameObj.transform.SetParent(slotObj.transform, false);
            Text charNameText = charNameObj.AddComponent<Text>();
            charNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            charNameText.text = (characters.Count > 0) ? characters[0].characterName : "No Characters";
            charNameText.alignment = TextAnchor.MiddleCenter;
            charNameText.color = Color.white;
            charNameText.fontSize = 22;
            slotUI.SetCharacterNameText(charNameText);

            RectTransform charNameRT = charNameObj.GetComponent<RectTransform>();
            charNameRT.anchorMin = new Vector2(0, 0.5f);
            charNameRT.anchorMax = new Vector2(1, 0.5f);
            charNameRT.offsetMin = new Vector2(20, -20);
            charNameRT.offsetMax = new Vector2(-20, 20);
            charNameRT.sizeDelta = new Vector2(0, 40);

            GameObject leftBtn = CreateArrowButton(slotObj.transform, "LeftArrow", new Vector2(-120, 0), "<");
            slotUI.SetLeftArrow(leftBtn.GetComponent<Button>());

            GameObject rightBtn = CreateArrowButton(slotObj.transform, "RightArrow", new Vector2(120, 0), ">");
            slotUI.SetRightArrow(rightBtn.GetComponent<Button>());

            GameObject readyObj = new GameObject("ReadyButton");
            readyObj.transform.SetParent(slotObj.transform, false);
            Image readyImg = readyObj.AddComponent<Image>();
            readyImg.color = teamColors[i];
            Button readyBtn = readyObj.AddComponent<Button>();
            readyBtn.targetGraphic = readyImg;
            slotUI.SetReadyButton(readyBtn);

            RectTransform readyRT = readyObj.GetComponent<RectTransform>();
            readyRT.anchorMin = new Vector2(0, 0);
            readyRT.anchorMax = new Vector2(1, 0);
            readyRT.offsetMin = new Vector2(30, 15);
            readyRT.offsetMax = new Vector2(-30, 40);
            readyRT.sizeDelta = new Vector2(0, 25);

            GameObject readyTextObj = new GameObject("ReadyText");
            readyTextObj.transform.SetParent(readyObj.transform, false);
            Text readyText = readyTextObj.AddComponent<Text>();
            readyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            readyText.text = "READY";
            readyText.alignment = TextAnchor.MiddleCenter;
            readyText.color = Color.white;
            readyText.fontSize = 16;
            slotUI.SetReadyText(readyText);

            RectTransform readyTextRT = readyTextObj.GetComponent<RectTransform>();
            readyTextRT.anchorMin = Vector2.zero;
            readyTextRT.anchorMax = Vector2.one;
            readyTextRT.offsetMin = Vector2.zero;
            readyTextRT.offsetMax = Vector2.zero;
        }
    }

    private GameObject CreateArrowButton(Transform parent, string name, Vector2 offset, string symbol)
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
        rt.anchoredPosition = offset;
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

    private void CreateStartButton()
    {
        GameObject btnObj = new GameObject("StartButton");
        btnObj.transform.SetParent(canvas.transform, false);

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.15f, 0.65f, 0.20f, 1f);
        Button startBtn = btnObj.AddComponent<Button>();
        startBtn.targetGraphic = img;
        manager.startButton = startBtn;

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0);
        rt.anchorMax = new Vector2(0.5f, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(0, 40);
        rt.sizeDelta = new Vector2(180, 50);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        Text txt = textObj.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.text = "START";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.fontSize = 20;

        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
    }

    private void CreateCountdownText()
    {
        GameObject textObj = new GameObject("CountdownText");
        textObj.transform.SetParent(canvas.transform, false);

        Text countdownText = textObj.AddComponent<Text>();
        countdownText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        countdownText.text = "STARTING...";
        countdownText.alignment = TextAnchor.MiddleCenter;
        countdownText.color = Color.yellow;
        countdownText.fontSize = 60;
        manager.countdownText = countdownText;
        countdownText.gameObject.SetActive(false);

        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(400, 80);
    }

    private void SetupManager()
    {
        manager.inputManager = inputManager;
        manager.timerScript = timerScript;
        manager.joinCanvas = canvas.gameObject;

        manager.slots = new List<PlayerSlotUI>();
        for (int i = 0; i < slots.Count; i++)
        {
            manager.slots.Add(slots[i]);
        }

        manager.sharedCharacters = new List<CharacterData>(characters);
    }
}
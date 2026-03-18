using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class UIBootstrap
{
    private const string RuntimeCanvasName = "RuntimeCanvas";
    private static readonly BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.NonPublic;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        EnsureUi();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureUi();
    }

    private static void EnsureUi()
    {
        UIManager uiManager = Object.FindAnyObjectByType<UIManager>();
        if (uiManager == null)
        {
            GameObject managerObject = new GameObject("UIManager");
            uiManager = managerObject.AddComponent<UIManager>();
        }

        if (!HasAllReferences(uiManager))
        {
            Canvas canvas = GetOrCreateCanvas();
            TMP_FontAsset fontAsset = TMP_Settings.defaultFontAsset;
            if (fontAsset == null)
            {
                fontAsset = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            }

            GameObject startScreen = GetOrCreatePanel(uiManager, "startScreen", "StartScreen", canvas.transform, new Color(0.03f, 0.05f, 0.09f, 0.9f));
            EnsureStaticText(startScreen.transform, "TitleText", fontAsset, "Survival of the Fittest", TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 170f), 64f, new Vector2(1200f, 120f));
            EnsureStaticText(startScreen.transform, "InstructionText", fontAsset, "Press Play to start\nMove with WASD or arrow keys", TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), 30f, new Vector2(900f, 120f));

            TextMeshProUGUI startBestText = GetOrCreateText(uiManager, "startBestText", "StartBestText", startScreen.transform, fontAsset, "Best Run\nScore 0 | Logs 0 | Time 0.0s", TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), 36f, new Vector2(900f, 120f));
            Button playButton = GetOrCreateButton(uiManager, "playButton", "PlayButton", startScreen.transform, fontAsset, "Play", new Vector2(300f, 84f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -170f));

            GameObject hudScreen = GetOrCreatePanel(uiManager, "hudScreen", "HUD", canvas.transform, new Color(0f, 0f, 0f, 0f));
            TextMeshProUGUI scoreText = GetOrCreateText(uiManager, "scoreText", "ScoreText", hudScreen.transform, fontAsset, "Score: 0", TextAlignmentOptions.TopLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), 38f, new Vector2(420f, 90f));
            TextMeshProUGUI timerText = GetOrCreateText(uiManager, "timerText", "TimerText", hudScreen.transform, fontAsset, "Time: 0.0s", TextAlignmentOptions.TopRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -24f), 38f, new Vector2(420f, 90f));
            TextMeshProUGUI logsText = GetOrCreateText(uiManager, "logsText", "LogsText", hudScreen.transform, fontAsset, "Logs: 0", TextAlignmentOptions.TopLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -78f), 30f, new Vector2(420f, 80f));
            TextMeshProUGUI bestScoreText = GetOrCreateText(uiManager, "bestScoreText", "BestScoreText", hudScreen.transform, fontAsset, "Best: Score 0 | Logs 0 | Time 0.0s", TextAlignmentOptions.Top, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), 28f, new Vector2(900f, 90f));

            GameObject gameOverScreen = GetOrCreatePanel(uiManager, "gameOverScreen", "GameOver", canvas.transform, new Color(0f, 0f, 0f, 0.78f));
            EnsureStaticText(gameOverScreen.transform, "GameOverTitleText", fontAsset, "Game Over", TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 170f), 72f, new Vector2(900f, 120f));
            TextMeshProUGUI finalScoreText = GetOrCreateText(uiManager, "finalScoreText", "FinalScoreText", gameOverScreen.transform, fontAsset, "Final Score: 0", TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 55f), 52f, new Vector2(900f, 100f));
            TextMeshProUGUI finalBreakdownText = GetOrCreateText(uiManager, "finalBreakdownText", "FinalBreakdownText", gameOverScreen.transform, fontAsset, "Logs Passed: 0 | Time: 0.0s", TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), 34f, new Vector2(900f, 90f));
            TextMeshProUGUI bestResultText = GetOrCreateText(uiManager, "bestResultText", "BestResultText", gameOverScreen.transform, fontAsset, "Best Run: Score 0 | Logs 0 | Time 0.0s", TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -70f), 30f, new Vector2(980f, 90f));
            Button replayButton = GetOrCreateButton(uiManager, "replayButton", "ReplayButton", gameOverScreen.transform, fontAsset, "Replay", new Vector2(300f, 84f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -180f));

            SetField(uiManager, "startBestText", startBestText);
            SetField(uiManager, "playButton", playButton);
            SetField(uiManager, "scoreText", scoreText);
            SetField(uiManager, "timerText", timerText);
            SetField(uiManager, "logsText", logsText);
            SetField(uiManager, "bestScoreText", bestScoreText);
            SetField(uiManager, "finalScoreText", finalScoreText);
            SetField(uiManager, "finalBreakdownText", finalBreakdownText);
            SetField(uiManager, "bestResultText", bestResultText);
            SetField(uiManager, "replayButton", replayButton);
        }

        uiManager.InitializeUi();
        EnsureEventSystem();
    }

    private static bool HasAllReferences(UIManager uiManager)
    {
        return GetField<GameObject>(uiManager, "startScreen") != null
            && GetField<GameObject>(uiManager, "hudScreen") != null
            && GetField<GameObject>(uiManager, "gameOverScreen") != null
            && GetField<TextMeshProUGUI>(uiManager, "startBestText") != null
            && GetField<Button>(uiManager, "playButton") != null
            && GetField<TextMeshProUGUI>(uiManager, "scoreText") != null
            && GetField<TextMeshProUGUI>(uiManager, "timerText") != null
            && GetField<TextMeshProUGUI>(uiManager, "logsText") != null
            && GetField<TextMeshProUGUI>(uiManager, "bestScoreText") != null
            && GetField<TextMeshProUGUI>(uiManager, "finalScoreText") != null
            && GetField<TextMeshProUGUI>(uiManager, "finalBreakdownText") != null
            && GetField<TextMeshProUGUI>(uiManager, "bestResultText") != null
            && GetField<Button>(uiManager, "replayButton") != null;
    }

    private static GameObject GetOrCreatePanel(UIManager uiManager, string fieldName, string objectName, Transform parent, Color color)
    {
        GameObject panel = GetField<GameObject>(uiManager, fieldName);
        if (panel == null)
        {
            panel = CreatePanel(objectName, parent, color);
            SetField(uiManager, fieldName, panel);
        }

        return panel;
    }

    private static TextMeshProUGUI GetOrCreateText(
        UIManager uiManager,
        string fieldName,
        string objectName,
        Transform parent,
        TMP_FontAsset fontAsset,
        string content,
        TextAlignmentOptions alignment,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        float fontSize,
        Vector2 size)
    {
        TextMeshProUGUI text = GetField<TextMeshProUGUI>(uiManager, fieldName);
        if (text == null)
        {
            text = CreateText(objectName, parent, fontAsset, content, alignment, anchorMin, anchorMax, anchoredPosition, fontSize, size);
            SetField(uiManager, fieldName, text);
        }

        return text;
    }

    private static Button GetOrCreateButton(
        UIManager uiManager,
        string fieldName,
        string objectName,
        Transform parent,
        TMP_FontAsset fontAsset,
        string label,
        Vector2 size,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition)
    {
        Button button = GetField<Button>(uiManager, fieldName);
        if (button == null)
        {
            button = CreateButton(objectName, parent, fontAsset, label, size, anchorMin, anchorMax, anchoredPosition);
            SetField(uiManager, fieldName, button);
        }

        return button;
    }

    private static void EnsureStaticText(
        Transform parent,
        string objectName,
        TMP_FontAsset fontAsset,
        string content,
        TextAlignmentOptions alignment,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        float fontSize,
        Vector2 size)
    {
        Transform existing = parent.Find(objectName);
        if (existing != null)
        {
            TextMeshProUGUI existingText = existing.GetComponent<TextMeshProUGUI>();
            if (existingText != null)
            {
                existingText.font = fontAsset;
                existingText.text = content;
                existingText.fontSize = fontSize;
                existingText.alignment = alignment;
                existingText.rectTransform.anchorMin = anchorMin;
                existingText.rectTransform.anchorMax = anchorMax;
                existingText.rectTransform.pivot = new Vector2((anchorMin.x + anchorMax.x) * 0.5f, (anchorMin.y + anchorMax.y) * 0.5f);
                existingText.rectTransform.anchoredPosition = anchoredPosition;
                existingText.rectTransform.sizeDelta = size;
            }
            return;
        }

        CreateText(objectName, parent, fontAsset, content, alignment, anchorMin, anchorMax, anchoredPosition, fontSize, size);
    }

    private static T GetField<T>(UIManager uiManager, string fieldName) where T : Object
    {
        FieldInfo field = typeof(UIManager).GetField(fieldName, FieldFlags);
        return field?.GetValue(uiManager) as T;
    }

    private static void SetField(UIManager uiManager, string fieldName, Object value)
    {
        FieldInfo field = typeof(UIManager).GetField(fieldName, FieldFlags);
        field?.SetValue(uiManager, value);
    }

    private static Canvas GetOrCreateCanvas()
    {
        Canvas canvas = GameObject.Find(RuntimeCanvasName)?.GetComponent<Canvas>();
        if (canvas != null)
        {
            return canvas;
        }

        GameObject canvasObject = new GameObject(RuntimeCanvasName);
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static GameObject CreatePanel(string objectName, Transform parent, Color color)
    {
        GameObject panel = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        panel.GetComponent<Image>().color = color;
        return panel;
    }

    private static TextMeshProUGUI CreateText(
        string objectName,
        Transform parent,
        TMP_FontAsset fontAsset,
        string content,
        TextAlignmentOptions alignment,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        float fontSize,
        Vector2 size)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = new Vector2((anchorMin.x + anchorMax.x) * 0.5f, (anchorMin.y + anchorMax.y) * 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = fontAsset;
        text.text = content;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = alignment;
        return text;
    }

    private static Button CreateButton(
        string objectName,
        Transform parent,
        TMP_FontAsset fontAsset,
        string label,
        Vector2 size,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.92f, 0.29f, 0.18f, 0.95f);

        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = new Color(1f, 0.42f, 0.28f, 1f);
        colors.pressedColor = new Color(0.8f, 0.22f, 0.12f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        button.targetGraphic = image;

        TextMeshProUGUI labelText = CreateText(
            "Label",
            buttonObject.transform,
            fontAsset,
            label,
            TextAlignmentOptions.Center,
            Vector2.zero,
            Vector2.one,
            Vector2.zero,
            38f,
            Vector2.zero);
        labelText.rectTransform.offsetMin = Vector2.zero;
        labelText.rectTransform.offsetMax = Vector2.zero;

        return button;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }
}

using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class UIBootstrap
{
    private const string RuntimeCanvasName = "RuntimeCanvas";
    private static readonly BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.NonPublic;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureUi()
    {
        Time.timeScale = 1f;

        UIManager uiManager = Object.FindAnyObjectByType<UIManager>();
        if (uiManager == null)
        {
            GameObject managerObject = new GameObject("UIManager");
            uiManager = managerObject.AddComponent<UIManager>();
        }

        if (HasAllReferences(uiManager))
        {
            EnsureEventSystem();
            return;
        }

        Canvas canvas = GetOrCreateCanvas();
        TMP_FontAsset fontAsset = TMP_Settings.defaultFontAsset;
        if (fontAsset == null)
        {
            fontAsset = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }

        GameObject hudScreen = GetField<GameObject>(uiManager, "hudScreen");
        if (hudScreen == null)
        {
            hudScreen = CreatePanel("HUD", canvas.transform, new Color(0f, 0f, 0f, 0f));
            SetField(uiManager, "hudScreen", hudScreen);
        }

        TextMeshProUGUI scoreText = GetField<TextMeshProUGUI>(uiManager, "scoreText");
        if (scoreText == null)
        {
            scoreText = CreateText(
                "ScoreText",
                hudScreen.transform,
                fontAsset,
                "Score: 0",
                TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(24f, -24f),
                42f);
            SetField(uiManager, "scoreText", scoreText);
        }

        TextMeshProUGUI timerText = GetField<TextMeshProUGUI>(uiManager, "timerText");
        if (timerText == null)
        {
            timerText = CreateText(
                "TimerText",
                hudScreen.transform,
                fontAsset,
                "Time: 0s",
                TextAlignmentOptions.TopRight,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-24f, -24f),
                42f);
            SetField(uiManager, "timerText", timerText);
        }

        GameObject gameOverScreen = GetField<GameObject>(uiManager, "gameOverScreen");
        if (gameOverScreen == null)
        {
            gameOverScreen = CreatePanel("GameOver", canvas.transform, new Color(0f, 0f, 0f, 0.75f));
            SetField(uiManager, "gameOverScreen", gameOverScreen);
        }

        TextMeshProUGUI finalScoreText = GetField<TextMeshProUGUI>(uiManager, "finalScoreText");
        if (finalScoreText == null)
        {
            finalScoreText = CreateText(
                "FinalScoreText",
                gameOverScreen.transform,
                fontAsset,
                "Final Score: 0",
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 40f),
                54f);
            SetField(uiManager, "finalScoreText", finalScoreText);
        }

        Button replayButton = GetField<Button>(uiManager, "replayButton");
        if (replayButton == null)
        {
            replayButton = CreateButton(
                "ReplayButton",
                gameOverScreen.transform,
                fontAsset,
                "Replay",
                new Vector2(280f, 80f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -60f));
            SetField(uiManager, "replayButton", replayButton);
        }

        hudScreen.SetActive(true);
        gameOverScreen.SetActive(false);
        EnsureEventSystem();
    }

    private static bool HasAllReferences(UIManager uiManager)
    {
        return GetField<GameObject>(uiManager, "hudScreen") != null
            && GetField<GameObject>(uiManager, "gameOverScreen") != null
            && GetField<TextMeshProUGUI>(uiManager, "scoreText") != null
            && GetField<TextMeshProUGUI>(uiManager, "timerText") != null
            && GetField<TextMeshProUGUI>(uiManager, "finalScoreText") != null
            && GetField<Button>(uiManager, "replayButton") != null;
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
        float fontSize)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = new Vector2((anchorMin.x + anchorMax.x) * 0.5f, (anchorMin.y + anchorMax.y) * 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(600f, 100f);

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
            38f);
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

using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// Manages Main Menu and Game Over screens.
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // Panels 
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject gameOverPanel;

    // Main Menu 
    [Header("Main Menu")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button easyButton;
    [SerializeField] private Button mediumButton;
    [SerializeField] private Button hardButton;
    [SerializeField] private TextMeshProUGUI selectedDifficultyText;

    // HUD 
    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;

    // Game Over 
    [Header("Game Over")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private Button replayButton;
    [SerializeField] private Button mainMenuButton;

    //  Internal
    private Difficulty _selectedDifficulty = Difficulty.Medium;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Wire up buttons
        playButton?.onClick.AddListener(OnPlayClicked);
        quitButton?.onClick.AddListener(OnQuitClicked);
        easyButton?.onClick.AddListener(() => SelectDifficulty(Difficulty.Easy));
        mediumButton?.onClick.AddListener(() => SelectDifficulty(Difficulty.Medium));
        hardButton?.onClick.AddListener(() => SelectDifficulty(Difficulty.Hard));
        replayButton?.onClick.AddListener(OnReplayClicked);
        mainMenuButton?.onClick.AddListener(OnMainMenuClicked);

        ShowMainMenu();
        SelectDifficulty(Difficulty.Medium);
    }

    void Update()
    {
        if (GameManager.Instance?.CurrentState == GameState.Playing)
        {
            float score = GameManager.Instance.Score;
            if (scoreText)  scoreText.text  = $"Score: {Mathf.FloorToInt(score)}";
            if (timerText)  timerText.text  = $"Time: {score:F1}s";
        }
    }

    //Panel Switching 

    public void ShowMainMenu()
    {
        SetPanels(main: true, hud: false, gameOver: false);
    }

    public void ShowHUD()
    {
        SetPanels(main: false, hud: true, gameOver: false);
    }

    public void ShowGameOver(float finalScore)
    {
        SetPanels(main: false, hud: false, gameOver: true);
        if (finalScoreText)
            finalScoreText.text = $"Score: {Mathf.FloorToInt(finalScore)}\nTime: {finalScore:F1}s";
    }

    private void SetPanels(bool main, bool hud, bool gameOver)
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(main);
        if (hudPanel)      hudPanel.SetActive(hud);
        if (gameOverPanel) gameOverPanel.SetActive(gameOver);
    }

    // Button Callbacks 

    private void OnPlayClicked()
    {
        GameManager.Instance?.SetDifficulty(_selectedDifficulty);
        GameManager.Instance?.StartGame();
        ShowHUD();
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SelectDifficulty(Difficulty d)
    {
        _selectedDifficulty = d;
        if (selectedDifficultyText)
            selectedDifficultyText.text = $"Difficulty: {d}";

        // Visual feedback, highlight selected button
        SetDifficultyButtonColors(d);
    }

    private void SetDifficultyButtonColors(Difficulty selected)
    {
        Color active   = new Color(0.2f, 0.8f, 0.2f); // green
        Color inactive = Color.white;

        if (easyButton)   easyButton.image.color   = selected == Difficulty.Easy   ? active : inactive;
        if (mediumButton) mediumButton.image.color  = selected == Difficulty.Medium ? active : inactive;
        if (hardButton)   hardButton.image.color    = selected == Difficulty.Hard   ? active : inactive;
    }

    private void OnReplayClicked()
    {
        GameManager.Instance?.RestartGame();
        ShowHUD();
    }

    private void OnMainMenuClicked()
    {
        GameManager.Instance?.GoToMainMenu();
        ShowMainMenu();
    }
}

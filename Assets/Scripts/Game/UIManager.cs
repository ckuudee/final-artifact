using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private const string BestScoreKey = "UIManager.BestScore";
    private const string BestTimeKey = "UIManager.BestTime";
    private const string BestLogsKey = "UIManager.BestLogs";
    private const int ScorePerLog = 100;
    private const float ScorePerSecond = 10f;

    public static UIManager Instance;

    [Header("Screens")]
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject hudScreen;
    [SerializeField] private GameObject gameOverScreen;

    [Header("Start Screen")]
    [SerializeField] private TextMeshProUGUI startBestText;
    [SerializeField] private Button playButton;

    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI logsText;
    [SerializeField] private TextMeshProUGUI bestScoreText;

    [Header("Game Over Elements")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI finalBreakdownText;
    [SerializeField] private TextMeshProUGUI bestResultText;
    [SerializeField] private Button replayButton;

    private float _survivalTime;
    private int _logsPassed;
    private bool _gameOver;
    private bool _gameStarted;
    private int _bestScore;
    private float _bestTime;
    private int _bestLogs;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadBestRun();
    }

    private void Start()
    {
        InitializeUi();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (!IsRunActive())
        {
            return;
        }

        _survivalTime += Time.deltaTime;
        RefreshHud();
    }

    public void InitializeUi()
    {
        BindButtons();
        ShowStartScreen();
        RefreshAllUi();
    }

    public void RegisterPassedLog()
    {
        if (!IsRunActive())
        {
            return;
        }

        _logsPassed++;
        RefreshHud();
    }

    public void TriggerGameOver()
    {
        if (_gameOver)
        {
            return;
        }

        _gameOver = true;
        _gameStarted = false;

        int finalScore = CalculateScore(_survivalTime, _logsPassed);
        UpdateBestRun(finalScore, _survivalTime, _logsPassed);

        if (startScreen != null)
        {
            startScreen.SetActive(false);
        }

        if (hudScreen != null)
        {
            hudScreen.SetActive(false);
        }

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }

        RefreshGameOver(finalScore);
        Time.timeScale = 0f;
    }

    private void BindButtons()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(OnPlayClicked);
            playButton.onClick.AddListener(OnPlayClicked);
        }

        if (replayButton != null)
        {
            replayButton.onClick.RemoveListener(OnReplayClicked);
            replayButton.onClick.AddListener(OnReplayClicked);
        }
    }

    private void OnPlayClicked()
    {
        BeginRun();
    }

    private void OnReplayClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void BeginRun()
    {
        _survivalTime = 0f;
        _logsPassed = 0;
        _gameOver = false;
        _gameStarted = true;

        if (startScreen != null)
        {
            startScreen.SetActive(false);
        }

        if (hudScreen != null)
        {
            hudScreen.SetActive(true);
        }

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(false);
        }

        Time.timeScale = 1f;
        RefreshHud();
    }

    private void ShowStartScreen()
    {
        _survivalTime = 0f;
        _logsPassed = 0;
        _gameOver = false;
        _gameStarted = false;

        if (startScreen != null)
        {
            startScreen.SetActive(true);
        }

        if (hudScreen != null)
        {
            hudScreen.SetActive(false);
        }

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(false);
        }

        Time.timeScale = 0f;
    }

    private void RefreshAllUi()
    {
        RefreshHud();
        RefreshStartScreen();
        RefreshGameOver(CalculateScore(_survivalTime, _logsPassed));
    }

    private void RefreshHud()
    {
        int score = CalculateScore(_survivalTime, _logsPassed);

        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }

        if (timerText != null)
        {
            timerText.text = $"Time: {FormatSeconds(_survivalTime)}";
        }

        if (logsText != null)
        {
            logsText.text = $"Logs: {_logsPassed}";
        }

        if (bestScoreText != null)
        {
            bestScoreText.text = $"Best: {BuildBestRunLabel()}";
        }
    }

    private void RefreshStartScreen()
    {
        if (startBestText != null)
        {
            startBestText.text = $"Best Run\n{BuildBestRunLabel()}";
        }
    }

    private void RefreshGameOver(int finalScore)
    {
        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {finalScore}";
        }

        if (finalBreakdownText != null)
        {
            finalBreakdownText.text = $"Logs Passed: {_logsPassed} | Time: {FormatSeconds(_survivalTime)}";
        }

        if (bestResultText != null)
        {
            bestResultText.text = $"Best Run: {BuildBestRunLabel()}";
        }
    }

    private void UpdateBestRun(int score, float survivalTime, int logsPassed)
    {
        bool beatBest = score > _bestScore;
        bool tieOnScore = score == _bestScore;
        bool beatOnTime = tieOnScore && survivalTime > _bestTime;
        bool beatOnLogs = tieOnScore && Mathf.Approximately(survivalTime, _bestTime) && logsPassed > _bestLogs;

        if (!beatBest && !beatOnTime && !beatOnLogs)
        {
            return;
        }

        _bestScore = score;
        _bestTime = survivalTime;
        _bestLogs = logsPassed;

        PlayerPrefs.SetInt(BestScoreKey, _bestScore);
        PlayerPrefs.SetFloat(BestTimeKey, _bestTime);
        PlayerPrefs.SetInt(BestLogsKey, _bestLogs);
        PlayerPrefs.Save();
    }

    private void LoadBestRun()
    {
        _bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        _bestTime = PlayerPrefs.GetFloat(BestTimeKey, 0f);
        _bestLogs = PlayerPrefs.GetInt(BestLogsKey, 0);
    }

    private bool IsRunActive()
    {
        return _gameStarted && !_gameOver && Time.timeScale > 0f;
    }

    private static int CalculateScore(float survivalTime, int logsPassed)
    {
        int timeScore = Mathf.FloorToInt(Mathf.Max(0f, survivalTime) * ScorePerSecond);
        int logScore = Mathf.Max(0, logsPassed) * ScorePerLog;
        return timeScore + logScore;
    }

    private string BuildBestRunLabel()
    {
        return $"Score {_bestScore} | Logs {_bestLogs} | Time {FormatSeconds(_bestTime)}";
    }

    private static string FormatSeconds(float seconds)
    {
        return $"{Mathf.Max(0f, seconds):0.0}s";
    }
}

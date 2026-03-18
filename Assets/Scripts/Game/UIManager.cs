using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance; // so other scripts can access this easily

    [Header("Screens")]
    [SerializeField] private GameObject hudScreen;
    [SerializeField] private GameObject gameOverScreen;

    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Game Over Elements")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private Button replayButton;

    private float _survivalTime = 0f;
    private bool _gameOver = false;

    private void Awake()
    {
        // make sure only one UIManager exists at a time
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // hook up replay button and show the HUD at the start
        replayButton?.onClick.AddListener(OnReplayClicked);
        hudScreen?.SetActive(true);
        gameOverScreen?.SetActive(false);
    }

    private void Update()
    {
        if (_gameOver) return;

        // keep track of how long the player has survived
        _survivalTime += Time.deltaTime;

        // score is just time * 10 for now
        if (scoreText != null)
            scoreText.text = "Score: " + Mathf.FloorToInt(_survivalTime * 10).ToString();

        if (timerText != null)
            timerText.text = "Time: " + Mathf.FloorToInt(_survivalTime).ToString() + "s";
    }

    // call this when the player dies
    // need to hook this into player death logic once thats implemented
    public void TriggerGameOver()
    {
        if (_gameOver) return;
        _gameOver = true;

        // swap screens
        hudScreen?.SetActive(false);
        gameOverScreen?.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = "Final Score: " + Mathf.FloorToInt(_survivalTime * 10).ToString();
    }

    // just reload the scene
    private void OnReplayClicked()
    {
        Time.timeScale = 1f;
        Instance = null;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
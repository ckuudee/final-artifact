using UnityEngine;

public enum Difficulty { Easy, Medium, Hard }
public enum GameState { MainMenu, Playing, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Difficulty Settings")]
    public float easyStartSpeed = 3f;
    public float mediumStartSpeed = 5f;
    public float hardStartSpeed = 8f;

    [Header("Runtime State")]
    public Difficulty SelectedDifficulty { get; private set; } = Difficulty.Medium;
    public GameState CurrentState { get; private set; } = GameState.MainMenu;
    public float Score { get; private set; } = 0f;
    public float ObstacleSpeed { get; private set; }

    private float speedAcceleration = 0.5f; // units per second increase

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (CurrentState != GameState.Playing) return;

        Score += Time.deltaTime;
        ObstacleSpeed += speedAcceleration * Time.deltaTime;
    }

    public void SetDifficulty(Difficulty difficulty)
    {
        SelectedDifficulty = difficulty;
        Debug.Log($"Difficulty set to: {difficulty}");
    }

    public void StartGame()
    {
        Score = 0f;
        ObstacleSpeed = SelectedDifficulty switch
        {
            Difficulty.Easy   => easyStartSpeed,
            Difficulty.Hard   => hardStartSpeed,
            _                 => mediumStartSpeed,
        };
        CurrentState = GameState.Playing;
        // Scene loading will work when GameScene is finished
        // UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    public void TriggerGameOver()
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.GameOver;
        UIManager.Instance?.ShowGameOver(Score);
    }

    public void RestartGame()
    {
        StartGame();
    }

    public void GoToMainMenu()
    {
        CurrentState = GameState.MainMenu;
        // UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}

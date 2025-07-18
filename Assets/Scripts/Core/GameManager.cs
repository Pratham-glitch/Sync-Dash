using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
// Removed Unity.VisualScripting as it wasn't used and might cause conflicts if not installed

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public float initialSpeed = 5f;
    public float speedIncrement = 0.1f;
    public float speedIncreaseInterval = 10f;

    [Header("References")]
    public PlayerController playerController;
    public GhostPlayer ghostPlayer;
    public ObstacleSpawner obstacleSpawner;
    public NetworkSimulator networkSimulator;

    [Header("UI")]
    public GameUI gameUI;
    public GameObject gameOverScreen;
    public GameObject mainMenuScreen; // Reference to your main menu UI panel

    private float currentSpeed;
    private int score;
    private int botScore; // Added bot score variable
    private bool isGameRunning;
    public bool firstOpening; // This will now work as intended with scene reloads
    private float gameTime;

    public float CurrentSpeed => currentSpeed;
    public int Score => score;
    public bool IsGameRunning => isGameRunning;

    void Awake()
    {
        Debug.Log("GameManager: Awake() called.");
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("GameManager: Instance set successfully.");
            // REMOVED: DontDestroyOnLoad(gameObject);
            // Removing this line ensures GameManager is recreated with each scene load,
            // allowing 'firstOpening' and scene-specific references to work correctly.
        }
        else
        {
            // If an instance already exists (e.g., from a previous scene load), destroy this new one.
            Destroy(gameObject);
            return; // Exit to prevent further execution on duplicate
        }

        // Initialize firstOpening here. It will be true on the very first load of the scene.
        // It will be set to false by StartGame() and persist for the duration of the scene.
        firstOpening = true;
    }

    void Start()
    {
        Debug.Log("GameManager: Start() called.");
        // Check if ObjectPool.Instance is available here
        if (ObjectPool.Instance == null)
        {
            Debug.LogError("GameManager: ObjectPool.Instance is NULL in Start()! Check Script Execution Order.");
        }
        else
        {
            Debug.Log("GameManager: ObjectPool.Instance is available in Start().");
        }

        // Now, 'firstOpening' correctly dictates initial state based on scene load
        if (firstOpening == false)
        {
            // This path is taken if the scene was reloaded (e.g., via RestartGame)
            StartGame();
        }
        else
        {
            // This path is taken on the very first load of this game scene
            ShowMainMenu();
        }
    }

    void Update()
    {
        if (isGameRunning)
        {
            gameTime += Time.deltaTime;
            UpdateSpeed(); // Uncommented
            UpdateScore(); // Uncommented
        }
    }

    public void ShowMainMenu()
    {
        isGameRunning = false;
        if (mainMenuScreen != null)
            mainMenuScreen.SetActive(true);
        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);
        Time.timeScale = 0f; // Pause game logic
    }

    public void StartGame()
    {
        firstOpening = false; // Set to false once game starts, so restarts go directly to game
        isGameRunning = true;
        currentSpeed = initialSpeed;
        score = 0;
        botScore = 0;
        gameTime = 0;

        if (mainMenuScreen != null)
            mainMenuScreen.SetActive(false);
        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);

        // Ensure all scene references are valid before calling their methods
        if (playerController != null) playerController.ResetPlayer();
        if (ghostPlayer != null) ghostPlayer.ResetPlayer();
        if (obstacleSpawner != null) obstacleSpawner.StartSpawning();
        if (networkSimulator != null) networkSimulator.StartSimulation();

        Time.timeScale = 1f; // Resume game logic
    }

    void UpdateSpeed()
    {
        /*currentSpeed = initialSpeed + (gameTime / speedIncreaseInterval) * speedIncrement;
        if (gameUI != null)
            gameUI.UpdateSpeed(currentSpeed);*/
    }

    void UpdateScore()
    {
        score += Mathf.RoundToInt(currentSpeed * Time.deltaTime);
        if (gameUI != null)
            gameUI.UpdateScore(score);
    }

    public void AddScore(int points)
    {
        score += points;
        if (gameUI != null)
            gameUI.UpdateScore(score);
    }

    public void AddScore_Bot(int points)
    {
        botScore += points;
        if (gameUI != null)
            gameUI.UpdateScore_Bot(botScore); // Pass botScore to the UI method
    }

    public void GameOver()
    {
        isGameRunning = false;
        if (gameOverScreen != null)
            gameOverScreen.SetActive(true);

        if (obstacleSpawner != null) obstacleSpawner.StopSpawning();
        if (networkSimulator != null) networkSimulator.StopSimulation();
        Time.timeScale = 0f; // Pause game logic
    }

    public void RestartGame()
    {
        // When RestartGame is called, we want to reload the scene.
        // The 'firstOpening' flag will then correctly ensure StartGame() is called
        // on the new GameManager instance in the reloaded scene's Start() method.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f; // Ensure timeScale is reset for the next scene load
    }

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

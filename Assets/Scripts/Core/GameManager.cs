using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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
    public GameObject mainMenuScreen; 

    private float currentSpeed;
    private int score;
    private int botScore; 
    private bool isGameRunning;
    public bool firstOpening; 
    private float gameTime;
    public GameObject gameOverText;

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
        }
        else
        {
            Destroy(gameObject);
            return;         }

        firstOpening = true;
    }

    void Start()
    {
        if(!gameUI.gameObject.activeInHierarchy) gameUI.gameObject.SetActive(true);

        Debug.Log("GameManager: Start() called.");
        if (ObjectPool.Instance == null)
        {
            Debug.LogError("GameManager: ObjectPool.Instance is NULL in Start()! Check Script Execution Order.");
        }
        else
        {
            Debug.Log("GameManager: ObjectPool.Instance is available in Start().");
        }

        if (firstOpening == false)
        {
            StartGame();
        }
        else
        {
            ShowMainMenu();
        }
    }

    void Update()
    {
        if (isGameRunning)
        {
            gameTime += Time.deltaTime;
            UpdateSpeed(); 
            UpdateScore(); 
        }
    }

    public void ShowMainMenu()
    {
        isGameRunning = false;
        if (mainMenuScreen != null)
            mainMenuScreen.SetActive(true);
        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);
        Time.timeScale = 0f; 
    }

    public void StartGame()
    {
        firstOpening = false; 
        isGameRunning = true;
        currentSpeed = initialSpeed;
        score = 0;
        botScore = 0;
        gameTime = 0;

        if (mainMenuScreen != null)
            mainMenuScreen.SetActive(false);
        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);

        if (playerController != null) playerController.ResetPlayer();
        if (ghostPlayer != null) ghostPlayer.ResetPlayer();
        if (obstacleSpawner != null) obstacleSpawner.StartSpawning();
        if (networkSimulator != null) networkSimulator.StartSimulation();

        Time.timeScale = 1f; 
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
            gameUI.UpdateScore_Bot(botScore); 
    }

    public void GameOver()
    {
        StartCoroutine(GameOverCoroutine());
    }

    private IEnumerator GameOverCoroutine()
    {
        Debug.Log("GameOverCoroutine just ran");
        CameraManager.instance.HitShake();
        gameOverText.SetActive(true);

        GameObject Orbit = new GameObject("Orbit");
        Orbit.transform.position = playerController.gameObject.transform.position;
        CameraManager.instance.StartRotating(Orbit.transform);

        Destroy(playerController.gameObject);
        yield return new WaitForSeconds(3);

        isGameRunning = false;
        if (gameOverScreen != null)
            gameOverScreen.SetActive(true);

        if (obstacleSpawner != null) obstacleSpawner.StopSpawning();
        if (networkSimulator != null) networkSimulator.StopSimulation();
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f;
    }

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

using UnityEngine;

public class GameController : MonoBehaviour, IController
{
    [Header("MVC Components")]
    [SerializeField] private GameModel gameModel;
    [SerializeField] private GameView gameView;
    [SerializeField] private GridController gridController;
    [SerializeField] private ScoreController scoreController;
    [SerializeField] private InputController inputController;

    public static GameController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        gameModel.Initialize();
        gameView.Initialize();

        gameModel.OnGameStateChanged += HandleGameStateChanged;
        gameModel.OnGameStarted += HandleGameStarted;
        gameModel.OnGameEnded += HandleGameEnded;
        gameModel.OnLevelCompleted += HandleLevelCompleted;

        gameView.OnStartButtonClicked += StartGame;
        gameView.OnPauseButtonClicked += PauseGame;
        gameView.OnResumeButtonClicked += ResumeGame;
        gameView.OnRestartButtonClicked += RestartGame;
    }

    private void Update()
    {
        if (gameModel.IsGameActive)
        {
            gameModel.UpdateTimer(Time.deltaTime);
        }
    }

    public void StartGame()
    {
        gameModel.StartGame();
        gridController.GenerateNewGrid();
        scoreController.ResetScore();
    }

    public void PauseGame()
    {
        gameModel.PauseGame();
    }

    public void ResumeGame()
    {
        gameModel.ResumeGame();
    }

    public void RestartGame()
    {
        StartGame();
    }

    public void CompleteLevel()
    {
        gameModel.CompleteLevel();
    }

    private void HandleGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Playing:
                inputController.EnableInput();
                break;
            case GameState.Paused:
            case GameState.GameOver:
            case GameState.Victory:
                inputController.DisableInput();
                break;
        }
    }

    private void HandleGameStarted()
    {
        Debug.Log("Game Started!");
    }

    private void HandleGameEnded()
    {
        Debug.Log("Game Ended!");
    }

    private void HandleLevelCompleted()
    {
        Debug.Log("Level Completed!");
    }

    public void Cleanup()
    {
        if (gameModel != null)
        {
            gameModel.OnGameStateChanged -= HandleGameStateChanged;
            gameModel.OnGameStarted -= HandleGameStarted;
            gameModel.OnGameEnded -= HandleGameEnded;
            gameModel.OnLevelCompleted -= HandleLevelCompleted;
        }

        if (gameView != null)
        {
            gameView.OnStartButtonClicked -= StartGame;
            gameView.OnPauseButtonClicked -= PauseGame;
            gameView.OnResumeButtonClicked -= ResumeGame;
            gameView.OnRestartButtonClicked -= RestartGame;
        }
    }

    private void OnDestroy()
    {
        Cleanup();
    }
}
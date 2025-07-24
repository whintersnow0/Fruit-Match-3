using UnityEngine;
using System;

public class GameModel : BaseModel
{
    [Header("Game Settings")]
    [SerializeField] private GameConfig gameConfig;

    private GameState currentState;
    private int currentLevel;
    private float timeRemaining;
    private bool isGameActive;

    public GameState CurrentState
    {
        get => currentState;
        private set
        {
            if (currentState != value)
            {
                currentState = value;
                NotifyModelChanged();
                OnGameStateChanged?.Invoke(value);
            }
        }
    }

    public int CurrentLevel
    {
        get => currentLevel;
        private set
        {
            currentLevel = value;
            NotifyModelChanged();
        }
    }

    public float TimeRemaining
    {
        get => timeRemaining;
        private set
        {
            timeRemaining = value;
            NotifyModelChanged();
        }
    }

    public bool IsGameActive => isGameActive;
    public GameConfig Config => gameConfig;

    public event Action<GameState> OnGameStateChanged;
    public event Action OnGameStarted;
    public event Action OnGameEnded;
    public event Action OnLevelCompleted;

    public override void Initialize()
    {
        currentState = GameState.Menu;
        currentLevel = 1;
        isGameActive = false;
    }

    public void StartGame()
    {
        CurrentState = GameState.Playing;
        timeRemaining = gameConfig.levelTimeLimit;
        isGameActive = true;
        OnGameStarted?.Invoke();
    }

    public void PauseGame()
    {
        CurrentState = GameState.Paused;
        isGameActive = false;
    }

    public void ResumeGame()
    {
        CurrentState = GameState.Playing;
        isGameActive = true;
    }

    public void EndGame()
    {
        CurrentState = GameState.GameOver;
        isGameActive = false;
        OnGameEnded?.Invoke();
    }

    public void CompleteLevel()
    {
        CurrentLevel++;
        OnLevelCompleted?.Invoke();
    }

    public void UpdateTimer(float deltaTime)
    {
        if (isGameActive && timeRemaining > 0)
        {
            TimeRemaining -= deltaTime;
            if (timeRemaining <= 0)
            {
                EndGame();
            }
        }
    }
}

public enum GameState
{
    Menu,
    Playing,
    Paused,
    GameOver,
    Victory
}
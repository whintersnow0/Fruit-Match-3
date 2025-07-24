using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameView : MonoBehaviour, IView
{
    [Header("UI Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("UI Elements")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Game Model Reference")]
    [SerializeField] private GameModel gameModel;

    public event Action OnStartButtonClicked;
    public event Action OnPauseButtonClicked;
    public event Action OnResumeButtonClicked;
    public event Action OnRestartButtonClicked;

    public void Initialize()
    {
        SetupButtons();
        ShowMenuPanel();

        if (gameModel != null)
        {
            gameModel.OnModelChanged += UpdateView;
            gameModel.OnGameStateChanged += HandleGameStateChanged;
        }
    }

    private void SetupButtons()
    {
        startButton?.onClick.AddListener(() => OnStartButtonClicked?.Invoke());
        pauseButton?.onClick.AddListener(() => OnPauseButtonClicked?.Invoke());
        resumeButton?.onClick.AddListener(() => OnResumeButtonClicked?.Invoke());
        restartButton?.onClick.AddListener(() => OnRestartButtonClicked?.Invoke());
    }

    public void UpdateView()
    {
        if (gameModel == null) return;

        UpdateTimer();
        UpdateLevel();
    }

    private void UpdateTimer()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(gameModel.TimeRemaining / 60);
            int seconds = Mathf.FloorToInt(gameModel.TimeRemaining % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    private void UpdateLevel()
    {
        if (levelText != null)
        {
            levelText.text = $"Level {gameModel.CurrentLevel}";
        }
    }

    private void HandleGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Menu:
                ShowMenuPanel();
                break;
            case GameState.Playing:
                ShowGamePanel();
                break;
            case GameState.Paused:
                ShowPausePanel();
                break;
            case GameState.GameOver:
                ShowGameOverPanel();
                break;
        }
    }

    private void ShowMenuPanel()
    {
        SetPanelActive(menuPanel, true);
        SetPanelActive(gamePanel, false);
        SetPanelActive(pausePanel, false);
        SetPanelActive(gameOverPanel, false);
    }

    private void ShowGamePanel()
    {
        SetPanelActive(menuPanel, false);
        SetPanelActive(gamePanel, true);
        SetPanelActive(pausePanel, false);
        SetPanelActive(gameOverPanel, false);
    }

    private void ShowPausePanel()
    {
        SetPanelActive(pausePanel, true);
    }

    private void ShowGameOverPanel()
    {
        SetPanelActive(gameOverPanel, true);
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    public void Cleanup()
    {
        if (gameModel != null)
        {
            gameModel.OnModelChanged -= UpdateView;
            gameModel.OnGameStateChanged -= HandleGameStateChanged;
        }

        startButton?.onClick.RemoveAllListeners();
        pauseButton?.onClick.RemoveAllListeners();
        resumeButton?.onClick.RemoveAllListeners();
        restartButton?.onClick.RemoveAllListeners();
    }
}
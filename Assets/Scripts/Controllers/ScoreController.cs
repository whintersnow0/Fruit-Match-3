
using UnityEngine;

public class ScoreController : MonoBehaviour, IController
{
    [Header("MVC Components")]
    [SerializeField] private ScoreModel scoreModel;
    [SerializeField] private ScoreView scoreView;

    public void Initialize()
    {
        if (scoreModel == null || scoreView == null)
        {
            Debug.LogError("ScoreController: Missing required components!");
            return;
        }

        scoreModel.Initialize();
        scoreView.Initialize();
        SubscribeToEvents();
        UpdateView();
    }

    private void SubscribeToEvents()
    {
        if (scoreModel != null)
        {
            scoreModel.OnScoreChanged += HandleScoreChanged;
            scoreModel.OnComboChanged += HandleComboChanged;
            scoreModel.OnTargetReached += HandleTargetReached;
        }
    }

    public void AddScore(int points)
    {
        if (scoreModel != null)
        {
            scoreModel.AddScore(points);
        }
    }

    public void SetCombo(int combo)
    {
        if (scoreModel != null)
        {
            scoreModel.SetCombo(combo);
        }
    }

    public void AddCombo()
    {
        if (scoreModel != null)
        {
            scoreModel.AddCombo();
        }
    }

    public void ResetCombo()
    {
        if (scoreModel != null)
        {
            scoreModel.ResetCombo();
        }
    }

    public void ResetScore()
    {
        if (scoreModel != null)
        {
            scoreModel.ResetScore();
        }
    }

    public void SetTargetScore(int target)
    {
        if (scoreModel != null)
        {
            scoreModel.TargetScore = target;
            UpdateView();
        }
    }

    private void HandleScoreChanged(int newScore)
    {
        if (scoreView != null)
        {
            scoreView.UpdateScore(newScore);
            scoreView.UpdateProgress(scoreModel.GetProgress());
        }
    }

    private void HandleComboChanged(int newCombo)
    {
        if (scoreView != null)
        {
            scoreView.UpdateCombo(newCombo);
        }
    }

    private void HandleTargetReached()
    {
        if (scoreView != null)
        {
            scoreView.PlayTargetReachedAnimation();
        }

        var gameController = FindObjectOfType<GameController>();
        if (gameController != null)
        {
            gameController.CompleteLevel();
        }
    }

    private void UpdateView()
    {
        if (scoreModel == null || scoreView == null) return;

        scoreView.UpdateScore(scoreModel.CurrentScore);
        scoreView.UpdateCombo(scoreModel.Combo);
        scoreView.UpdateTarget(scoreModel.TargetScore);
        scoreView.UpdateProgress(scoreModel.GetProgress());
    }

    public int GetCurrentScore()
    {
        return scoreModel != null ? scoreModel.CurrentScore : 0;
    }

    public int GetCurrentCombo()
    {
        return scoreModel != null ? scoreModel.Combo : 0;
    }

    public int GetTargetScore()
    {
        return scoreModel != null ? scoreModel.TargetScore : 0;
    }

    public float GetProgress()
    {
        return scoreModel != null ? scoreModel.GetProgress() : 0f;
    }

    public void Cleanup()
    {
        UnsubscribeFromEvents();

        if (scoreView != null)
        {
            scoreView.Cleanup();
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (scoreModel != null)
        {
            scoreModel.OnScoreChanged -= HandleScoreChanged;
            scoreModel.OnComboChanged -= HandleComboChanged;
            scoreModel.OnTargetReached -= HandleTargetReached;
        }
    }

    private void OnDestroy()
    {
        Cleanup();
    }
}
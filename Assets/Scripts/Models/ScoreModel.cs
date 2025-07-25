using System;
using UnityEngine;

public class ScoreModel : BaseModel
{
    private int currentScore;
    private int targetScore;
    private int bestScore;
    private int multiplier;
    private int combo;

    public int CurrentScore
    {
        get => currentScore;
        private set
        {
            if (currentScore != value)
            {
                currentScore = value;
                NotifyModelChanged();
                OnScoreChanged?.Invoke(currentScore);
            }
        }
    }

    public int TargetScore
    {
        get => targetScore;
        set
        {
            targetScore = value;
            NotifyModelChanged();
        }
    }

    public int BestScore
    {
        get => bestScore;
        private set
        {
            bestScore = value;
            NotifyModelChanged();
        }
    }

    public int Multiplier => multiplier;
    public int Combo => combo;
    public bool IsTargetReached => currentScore >= targetScore && targetScore > 0;

    public event Action<int> OnScoreChanged;
    public event Action<int> OnComboChanged;
    public event Action OnTargetReached;

    public override void Initialize()
    {
        currentScore = 0;
        multiplier = 1;
        combo = 0;
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        NotifyModelChanged();
    }

    public void AddScore(int points)
    {
        int scoreToAdd = points * multiplier;
        CurrentScore += scoreToAdd;

        if (currentScore > bestScore)
        {
            BestScore = currentScore;
            PlayerPrefs.SetInt("BestScore", bestScore);
        }

        if (IsTargetReached)
        {
            OnTargetReached?.Invoke();
        }
    }

    public void SetCombo(int newCombo)
    {
        combo = newCombo;
        multiplier = 1 + (combo / 3);
        OnComboChanged?.Invoke(combo);
        NotifyModelChanged();
    }

    public void AddCombo()
    {
        combo++;
        multiplier = 1 + (combo / 3);
        OnComboChanged?.Invoke(combo);
        NotifyModelChanged();
    }

    public void ResetCombo()
    {
        combo = 0;
        multiplier = 1;
        OnComboChanged?.Invoke(combo);
        NotifyModelChanged();
    }

    public void ResetScore()
    {
        CurrentScore = 0;
        ResetCombo();
    }

    public float GetProgress()
    {
        if (targetScore <= 0) return 0f;
        return Mathf.Clamp01((float)currentScore / targetScore);
    }
}
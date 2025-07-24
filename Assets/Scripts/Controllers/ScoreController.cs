using UnityEditor;
using UnityEngine;

public class ScoreController : MonoBehaviour, IController
{
    [Header("MVC Components")]
    [SerializeField] private ScoreModel scoreModel;
    [SerializeField] private ScoreView scoreView;

    public void Initialize()
    {
        scoreModel.Initialize();
        scoreView.Initialize();

        scoreModel.OnTargetReached += HandleTargetReached;
    }

    public void AddScore(int points)
    {
        scoreModel.AddScore(points);
    }

    public void AddCombo()
    {
        scoreModel.AddCombo();
    }

    public void ResetCombo()
    {
        scoreModel.ResetCombo();
    }

    public void ResetScore()
    {
        scoreModel.ResetScore();
    }

    public void SetTargetScore(int target)
    {
        scoreModel.TargetScore = target;
    }

    private void HandleTargetReached()
    {
        GameController.Instance?.CompleteLevel();
    }

    public void Cleanup()
    {
        if (scoreModel != null)
        {
            scoreModel.OnTargetReached -= HandleTargetReached;
        }
    }
}
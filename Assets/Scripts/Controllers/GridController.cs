using UnityEngine;
using System.Collections.Generic;

public class GridController : MonoBehaviour, IController
{
    [Header("MVC Components")]
    [SerializeField] private GridModel gridModel;
    [SerializeField] private GridView gridView;
    [SerializeField] private ScoreController scoreController;

    private int currentCombo;

    public void Initialize()
    {
        currentCombo = 0;
        gridModel.Initialize();
        gridView.Initialize();
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        if (gridModel != null)
        {
            gridModel.OnModelChanged += gridView.UpdateView;
            gridModel.OnGemsSwapped += HandleGemsSwapped;
            gridModel.OnMatchesFound += HandleMatchesFound;
            gridModel.OnGemsDestroyed += HandleGemsDestroyed;
            gridModel.OnGridRefilled += HandleGridRefilled;
        }

        if (gridView != null)
        {
            gridView.OnGemSwapRequested += HandleSwapRequest;
        }
    }

    public void GenerateNewGrid()
    {
        currentCombo = 0;
        if (scoreController != null)
        {
            scoreController.ResetCombo();
        }
        gridModel.Initialize();
    }

    private void HandleSwapRequest(Vector2Int pos1, Vector2Int pos2)
    {
        if (gridModel == null) return;

        if (!IsValidPosition(pos1) || !IsValidPosition(pos2))
            return;

        bool swapSuccessful = gridModel.TrySwapGems(pos1, pos2);

        if (!swapSuccessful)
        {
            gridView.PlayInvalidSwapAnimation(pos1, pos2);
        }
    }

    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridModel.Width &&
               pos.y >= 0 && pos.y < gridModel.Height;
    }

    private void HandleGemsSwapped(Vector2Int pos1, Vector2Int pos2)
    {
        currentCombo = 0;
        if (scoreController != null)
        {
            scoreController.ResetCombo();
        }
    }

    private void HandleMatchesFound(List<Match> matches)
    {
        if (matches == null || matches.Count == 0) return;

        currentCombo++;

        int totalScore = 0;
        foreach (var match in matches)
        {
            totalScore += match.Score;
        }

        int comboMultiplier = Mathf.Max(1, currentCombo);
        int finalScore = totalScore * comboMultiplier;

        if (scoreController != null)
        {
            scoreController.AddScore(finalScore);
            scoreController.SetCombo(currentCombo);
        }
    }

    private void HandleGemsDestroyed(List<Vector2Int> positions)
    {
    }

    private void HandleGridRefilled()
    {
        if (!gridModel.IsProcessing)
        {
            currentCombo = 0;
            if (scoreController != null)
            {
                scoreController.ResetCombo();
            }
        }
    }

    public void Cleanup()
    {
        UnsubscribeFromEvents();
        if (gridView != null)
        {
            gridView.Cleanup();
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (gridModel != null)
        {
            gridModel.OnModelChanged -= gridView.UpdateView;
            gridModel.OnGemsSwapped -= HandleGemsSwapped;
            gridModel.OnMatchesFound -= HandleMatchesFound;
            gridModel.OnGemsDestroyed -= HandleGemsDestroyed;
            gridModel.OnGridRefilled -= HandleGridRefilled;
        }

        if (gridView != null)
        {
            gridView.OnGemSwapRequested -= HandleSwapRequest;
        }
    }

    private void OnDestroy()
    {
        Cleanup();
    }
}
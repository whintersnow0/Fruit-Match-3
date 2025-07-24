using UnityEngine;
using System.Collections.Generic;

public class GridController : MonoBehaviour, IController
{
    [Header("MVC Components")]
    [SerializeField] private GridModel gridModel;
    [SerializeField] private GridView gridView;
    [SerializeField] private ScoreController scoreController;

    public void Initialize()
    {
        gridModel.Initialize();
        gridView.Initialize();

        gridModel.OnModelChanged += gridView.UpdateView;
        gridModel.OnGemsSwapped += HandleGemsSwapped;
        gridModel.OnMatchesFound += HandleMatchesFound;
        gridModel.OnGemsDestroyed += HandleGemsDestroyed;
        gridModel.OnGridRefilled += HandleGridRefilled;

        gridView.OnGemSwapRequested += HandleSwapRequest;
    }

    public void GenerateNewGrid()
    {
        gridModel.Initialize();
    }

    private void HandleSwapRequest(Vector2Int pos1, Vector2Int pos2)
    {
        bool swapSuccessful = gridModel.TrySwapGems(pos1, pos2);
        if (!swapSuccessful)
        {
            gridView.PlayInvalidSwapAnimation(pos1, pos2);
        }
    }

    private void HandleGemsSwapped(Vector2Int pos1, Vector2Int pos2)
    {
        gridView.PlaySwapAnimation(pos1, pos2);
    }

    private void HandleMatchesFound(List<Match> matches)
    {
        gridView.HighlightMatches(matches);

        foreach (var match in matches)
        {
            scoreController.AddScore(match.Score);
        }

        scoreController.AddCombo();
    }

    private void HandleGemsDestroyed(List<Vector2Int> positions)
    {
        gridView.PlayDestroyAnimation(positions);
    }

    private void HandleGridRefilled()
    {
        gridView.PlayRefillAnimation();
        if (gridModel.CurrentMatches.Count == 0)
        {
            scoreController.ResetCombo();
        }
    }

    public void Cleanup()
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
}
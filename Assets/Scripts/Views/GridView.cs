using UnityEngine;
using System;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

public class GridView : MonoBehaviour
{
    [SerializeField] private Transform gridParent;
    [SerializeField] private float gemSpacing = 1f;
    [SerializeField] private GameObject[] gemPrefabs;
    [SerializeField] private float swapDuration = 0.3f;
    [SerializeField] private float destroyDuration = 0.2f;
    [SerializeField] private float fallDuration = 0.5f;
    [SerializeField] private GridModel gridModel;

    private GemView[,] gemViews;
    private bool isAnimating;

    public event Action<Vector2Int, Vector2Int> OnGemSwapRequested;

    public void Initialize()
    {
        if (gridModel == null) return;
        gemViews = new GemView[gridModel.Width, gridModel.Height];
        CreateInitialGems();
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        var inputController = FindObjectOfType<InputController>();
        if (inputController != null)
            inputController.OnSwipeDetected += HandleSwipeInput;

        if (gridModel != null)
        {
            gridModel.OnGemsSwapped += HandleValidSwap;
            gridModel.OnMatchesFound += HandleMatchesFound;
            gridModel.OnGemsDestroyed += HandleGemsDestroyed;
            gridModel.OnGridRefilled += HandleGridRefilled;
            gridModel.OnModelChanged += UpdateView;
        }
    }

    private void HandleSwipeInput(Vector2Int pos1, Vector2Int pos2)
    {
        if (!isAnimating)
            OnGemSwapRequested?.Invoke(pos1, pos2);
    }

    private void CreateInitialGems()
    {
        for (int x = 0; x < gridModel.Width; x++)
            for (int y = 0; y < gridModel.Height; y++)
                if (gridModel.GetGem(x, y) != null)
                    CreateGemView(gridModel.GetGem(x, y), x, y);
    }

    private void CreateGemView(GemModel gemModel, int x, int y)
    {
        Vector3 position = GetWorldPosition(x, y);
        GameObject gemPrefab = gemPrefabs[(int)gemModel.Type];
        GameObject gemObject = Instantiate(gemPrefab, position, Quaternion.identity, gridParent);
        GemView gemView = gemObject.GetComponent<GemView>();
        if (gemView == null) gemView = gemObject.AddComponent<GemView>();
        gemView.Initialize(gemModel, new Vector2Int(x, y));
        gemViews[x, y] = gemView;
    }

    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(
            x * gemSpacing - (gridModel.Width - 1) * gemSpacing * 0.5f,
            y * gemSpacing - (gridModel.Height - 1) * gemSpacing * 0.5f,
            0
        );
    }

    private void HandleValidSwap(Vector2Int pos1, Vector2Int pos2)
    {
        StartCoroutine(PlayValidSwapSequence(pos1, pos2));
    }

    private IEnumerator PlayValidSwapSequence(Vector2Int pos1, Vector2Int pos2)
    {
        isAnimating = true;
        if (!IsValidPosition(pos1) || !IsValidPosition(pos2))
        {
            isAnimating = false;
            yield break;
        }
        var gemView1 = gemViews[pos1.x, pos1.y];
        var gemView2 = gemViews[pos2.x, pos2.y];
        if (gemView1 != null && gemView2 != null)
        {
            Vector3 targetPos1 = GetWorldPosition(pos2.x, pos2.y);
            Vector3 targetPos2 = GetWorldPosition(pos1.x, pos1.y);
            var t1 = gemView1.transform.DOMove(targetPos1, swapDuration).SetEase(Ease.OutCubic);
            var t2 = gemView2.transform.DOMove(targetPos2, swapDuration).SetEase(Ease.OutCubic);
            yield return DOTween.Sequence().Join(t1).Join(t2).WaitForCompletion();
            gemViews[pos1.x, pos1.y] = gemView2;
            gemViews[pos2.x, pos2.y] = gemView1;
            gemView1.UpdateGridPosition(pos2);
            gemView2.UpdateGridPosition(pos1);
        }
        isAnimating = false;
    }

    public void PlayInvalidSwapAnimation(Vector2Int pos1, Vector2Int pos2)
    {
        if (isAnimating) return;
        StartCoroutine(PlayInvalidSwapSequence(pos1, pos2));
    }

    private IEnumerator PlayInvalidSwapSequence(Vector2Int pos1, Vector2Int pos2)
    {
        isAnimating = true;
        if (!IsValidPosition(pos1) || !IsValidPosition(pos2))
        {
            isAnimating = false;
            yield break;
        }
        var gem1 = gemViews[pos1.x, pos1.y];
        var gem2 = gemViews[pos2.x, pos2.y];
        if (gem1 == null || gem2 == null)
        {
            isAnimating = false;
            yield break;
        }
        Vector3 posA = gem1.transform.position;
        Vector3 posB = gem2.transform.position;
        float halfDuration = swapDuration * 0.5f;
        gem1.transform.DOMove(posB, halfDuration).SetEase(Ease.InOutQuad);
        gem2.transform.DOMove(posA, halfDuration).SetEase(Ease.InOutQuad);
        yield return new WaitForSeconds(halfDuration);
        gem1.transform.DOMove(posA, halfDuration).SetEase(Ease.InOutQuad);
        gem2.transform.DOMove(posB, halfDuration).SetEase(Ease.InOutQuad);
        yield return new WaitForSeconds(halfDuration);
        isAnimating = false;
    }

    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gemViews.GetLength(0) &&
               pos.y >= 0 && pos.y < gemViews.GetLength(1);
    }

    private void HandleMatchesFound(List<Match> matches)
    {
        StartCoroutine(HandleMatchesSequence(matches));
    }

    private IEnumerator HandleMatchesSequence(List<Match> matches)
    {
        foreach (var match in matches)
            foreach (var pos in match.Positions)
                if (IsValidPosition(pos) && gemViews[pos.x, pos.y] != null)
                    gemViews[pos.x, pos.y].PlayMatchHighlight();
        yield return new WaitForSeconds(0.3f);
    }

    private void HandleGemsDestroyed(List<Vector2Int> positions)
    {
        StartCoroutine(HandleDestroySequence(positions));
    }

    private IEnumerator HandleDestroySequence(List<Vector2Int> positions)
    {
        foreach (var pos in positions)
        {
            if (!IsValidPosition(pos)) continue;
            var gemView = gemViews[pos.x, pos.y];
            if (gemView != null)
            {
                gemView.PlayDestroyAnimation(() => Destroy(gemView.gameObject));
                gemViews[pos.x, pos.y] = null;
            }
        }
        yield return new WaitForSeconds(destroyDuration);
    }

    private void HandleGridRefilled()
    {
        StartCoroutine(HandleRefillSequence());
    }

    private IEnumerator HandleRefillSequence()
    {
        for (int x = 0; x < gridModel.Width; x++)
        {
            for (int y = 0; y < gridModel.Height; y++)
            {
                var gemModel = gridModel.GetGem(x, y);
                var gemView = gemViews[x, y];
                if (gemModel != null && gemView == null)
                {
                    Vector3 startPosition = GetWorldPosition(x, gridModel.Height + 2);
                    Vector3 targetPosition = GetWorldPosition(x, y);
                    GameObject gemPrefab = gemPrefabs[(int)gemModel.Type];
                    GameObject gemObject = Instantiate(gemPrefab, startPosition, Quaternion.identity, gridParent);
                    GemView newGemView = gemObject.GetComponent<GemView>();
                    if (newGemView == null) newGemView = gemObject.AddComponent<GemView>();
                    newGemView.Initialize(gemModel, new Vector2Int(x, y));
                    gemViews[x, y] = newGemView;
                    float delay = UnityEngine.Random.Range(0f, 0.2f);
                    newGemView.transform.DOMove(targetPosition, fallDuration)
                        .SetEase(Ease.InBounce)
                        .SetDelay(delay);
                }
                else if (gemModel != null && gemView != null)
                {
                    Vector3 targetPosition = GetWorldPosition(x, y);
                    gemView.transform.DOMove(targetPosition, fallDuration).SetEase(Ease.InBounce);
                    gemView.UpdateGridPosition(new Vector2Int(x, y));
                    gemView.UpdateModel(gemModel);
                }
                else if (gemModel == null && gemView != null)
                {
                    Destroy(gemView.gameObject);
                    gemViews[x, y] = null;
                }
            }
        }
        yield return new WaitForSeconds(fallDuration + 0.2f);
        isAnimating = false;
    }

    public void UpdateView()
    {
        if (gemViews == null || gridModel == null) return;
        for (int x = 0; x < gridModel.Width; x++)
        {
            for (int y = 0; y < gridModel.Height; y++)
            {
                var gemModel = gridModel.GetGem(x, y);
                var gemView = gemViews[x, y];

                if (gemModel != null && gemView == null)
                    CreateGemView(gemModel, x, y);
                else if (gemModel == null && gemView != null)
                {
                    Destroy(gemView.gameObject);
                    gemViews[x, y] = null;
                }
                else if (gemModel != null && gemView != null)
                    gemView.UpdateModel(gemModel);
            }
        }
    }

    public void Cleanup()
    {
        DOTween.KillAll();
        isAnimating = false;
        if (gemViews != null)
            for (int x = 0; x < gemViews.GetLength(0); x++)
                for (int y = 0; y < gemViews.GetLength(1); y++)
                    if (gemViews[x, y] != null)
                    {
                        Destroy(gemViews[x, y].gameObject);
                        gemViews[x, y] = null;
                    }
    }

    private void OnDestroy()
    {
        if (gridModel != null)
        {
            gridModel.OnGemsSwapped -= HandleValidSwap;
            gridModel.OnMatchesFound -= HandleMatchesFound;
            gridModel.OnGemsDestroyed -= HandleGemsDestroyed;
            gridModel.OnGridRefilled -= HandleGridRefilled;
            gridModel.OnModelChanged -= UpdateView;
        }
    }
}
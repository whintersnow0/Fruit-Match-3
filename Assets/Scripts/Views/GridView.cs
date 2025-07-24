using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;
using System;
using Unity.VisualScripting;

public class GridView : MonoBehaviour, IView
{
    [Header("Grid Settings")]
    [SerializeField] private Transform gridParent;
    [SerializeField] private float gemSpacing = 1f;
    [SerializeField] private GameObject[] gemPrefabs;

    [Header("Animation Settings")]
    [SerializeField] private float swapDuration = 0.3f;
    [SerializeField] private float destroyDuration = 0.2f;
    [SerializeField] private float fallDuration = 0.5f;

    [Header("Model Reference")]
    [SerializeField] private GridModel gridModel;

    private GemView[,] gemViews;
    private Dictionary<Vector2Int, GemView> gemViewDict;

    public event Action<Vector2Int, Vector2Int> OnGemSwapRequested;

    public void Initialize()
    {
        if (gridModel == null) return;

        gemViews = new GemView[gridModel.Width, gridModel.Height];
        gemViewDict = new Dictionary<Vector2Int, GemView>();

        CreateInitialGems();

        var inputController = FindObjectOfType<InputController>();
        if (inputController != null)
        {
            inputController.OnSwipeDetected += (pos1, pos2) => OnGemSwapRequested?.Invoke(pos1, pos2);
        }
    }

    private void CreateInitialGems()
    {
        for (int x = 0; x < gridModel.Width; x++)
        {
            for (int y = 0; y < gridModel.Height; y++)
            {
                var gemModel = gridModel.GetGem(x, y);
                if (gemModel != null)
                {
                    CreateGemView(gemModel, x, y);
                }
            }
        }
    }

    private void CreateGemView(GemModel gemModel, int x, int y)
    {
        Vector3 position = GetWorldPosition(x, y);
        GameObject gemPrefab = gemPrefabs[(int)gemModel.Type];
        GameObject gemObject = Instantiate(gemPrefab, position, Quaternion.identity, gridParent);

        GemView gemView = gemObject.GetComponent<GemView>();
        if (gemView == null)
        {
            gemView = gemObject.AddComponent<GemView>();
        }

        gemView.Initialize(gemModel, new Vector2Int(x, y));

        gemViews[x, y] = gemView;
        gemViewDict[new Vector2Int(x, y)] = gemView;
    }

    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(
            x * gemSpacing - (gridModel.Width - 1) * gemSpacing * 0.5f,
            y * gemSpacing - (gridModel.Height - 1) * gemSpacing * 0.5f,
            0
        );
    }

    public void UpdateView()
    {
        for (int x = 0; x < gridModel.Width; x++)
        {
            for (int y = 0; y < gridModel.Height; y++)
            {
                var gemModel = gridModel.GetGem(x, y);
                var gemView = gemViews[x, y];

                if (gemModel != null && gemView == null)
                {
                    CreateGemView(gemModel, x, y);
                }
                else if (gemModel == null && gemView != null)
                {
                    RemoveGemView(x, y);
                }
                else if (gemModel != null && gemView != null)
                {                 
                    gemView.UpdateModel(gemModel);
                }
            }
        }
    }

    private void RemoveGemView(int x, int y)
    {
        var gemView = gemViews[x, y];
        if (gemView != null)
        {
            gemViewDict.Remove(new Vector2Int(x, y));
            gemViews[x, y] = null;
            Destroy(gemView.gameObject);
        }
    }

    public void PlaySwapAnimation(Vector2Int pos1, Vector2Int pos2)
    {
        var gem1 = gemViews[pos1.x, pos1.y];
        var gem2 = gemViews[pos2.x, pos2.y];

        if (gem1 != null && gem2 != null)
        {
            Vector3 pos1World = GetWorldPosition(pos2.x, pos2.y);
            Vector3 pos2World = GetWorldPosition(pos1.x, pos1.y);

            gem1.transform.DOMove(pos1World, swapDuration).SetEase(Ease.OutCubic);
            gem2.transform.DOMove(pos2World, swapDuration).SetEase(Ease.OutCubic);

            StartCoroutine(UpdateGemsArrayAfterSwap(pos1, pos2));
        }
    }

    private IEnumerator UpdateGemsArrayAfterSwap(Vector2Int pos1, Vector2Int pos2)
    {
        yield return new WaitForSeconds(swapDuration);

        var temp = gemViews[pos1.x, pos1.y];
        gemViews[pos1.x, pos1.y] = gemViews[pos2.x, pos2.y];
        gemViews[pos2.x, pos2.y] = temp;

        if (gemViews[pos1.x, pos1.y] != null)
            gemViews[pos1.x, pos1.y].UpdateGridPosition(pos1);
        if (gemViews[pos2.x, pos2.y] != null)
            gemViews[pos2.x, pos2.y].UpdateGridPosition(pos2);
    }

    public void PlayInvalidSwapAnimation(Vector2Int pos1, Vector2Int pos2)
    {
        var gem1 = gemViews[pos1.x, pos1.y];
        var gem2 = gemViews[pos2.x, pos2.y];

        if (gem1 != null && gem2 != null)
        {
            Vector3 originalPos1 = gem1.transform.position;
            Vector3 originalPos2 = gem2.transform.position;
            Vector3 targetPos1 = GetWorldPosition(pos2.x, pos2.y);
            Vector3 targetPos2 = GetWorldPosition(pos1.x, pos1.y);

            DG.Tweening.Sequence sequence = DOTween.Sequence();
            sequence.Append(gem1.transform.DOMove(targetPos1, swapDuration * 0.5f));
            sequence.Join(gem2.transform.DOMove(targetPos2, swapDuration * 0.5f));
            sequence.Append(gem1.transform.DOMove(originalPos1, swapDuration * 0.5f));
            sequence.Join(gem2.transform.DOMove(originalPos2, swapDuration * 0.5f));

            gem1.transform.DOShakePosition(0.3f, 0.1f, 10, 90, false, true);
            gem2.transform.DOShakePosition(0.3f, 0.1f, 10, 90, false, true);
        }
    }

    public void HighlightMatches(List<Match> matches)
    {
        foreach (var match in matches)
        {
            foreach (var pos in match.Positions)
            {
                var gemView = gemViews[pos.x, pos.y];
                if (gemView != null)
                {
                    gemView.PlayMatchHighlight();
                }
            }
        }
    }

    public void PlayDestroyAnimation(List<Vector2Int> positions)
    {
        foreach (var pos in positions)
        {
            var gemView = gemViews[pos.x, pos.y];
            if (gemView != null)
            {
                gemView.PlayDestroyAnimation(() => {
                    RemoveGemView(pos.x, pos.y);
                });
            }
        }
    }

    public void PlayRefillAnimation()
    {
        StartCoroutine(RefillAnimationCoroutine());
    }

    private IEnumerator RefillAnimationCoroutine()
    {
        yield return new WaitForSeconds(destroyDuration);

        for (int x = 0; x < gridModel.Width; x++)
        {
            for (int y = 0; y < gridModel.Height; y++)
            {
                var gemModel = gridModel.GetGem(x, y);
                var gemView = gemViews[x, y];

                if (gemModel != null && gemView != null)
                {
                    Vector3 targetPosition = GetWorldPosition(x, y);
                    if (Vector3.Distance(gemView.transform.position, targetPosition) > 0.1f)
                    {
                        gemView.transform.DOMove(targetPosition, fallDuration).SetEase(Ease.InBounce);
                    }
                }
            }
        }

        for (int x = 0; x < gridModel.Width; x++)
        {
            for (int y = 0; y < gridModel.Height; y++)
            {
                var gemModel = gridModel.GetGem(x, y);
                if (gemModel != null && gemViews[x, y] == null)
                {
                    Vector3 startPosition = GetWorldPosition(x, gridModel.Height + 2);
                    Vector3 targetPosition = GetWorldPosition(x, y);

                    GameObject gemPrefab = gemPrefabs[(int)gemModel.Type];
                    GameObject gemObject = Instantiate(gemPrefab, startPosition, Quaternion.identity, gridParent);

                    GemView gemView = gemObject.GetComponent<GemView>();
                    if (gemView == null)
                    {
                        gemView = gemObject.AddComponent<GemView>();
                    }

                    gemView.Initialize(gemModel, new Vector2Int(x, y));
                    gemViews[x, y] = gemView;
                    gemViewDict[new Vector2Int(x, y)] = gemView;

                    gemView.transform.DOMove(targetPosition, fallDuration).SetEase(Ease.InBounce).SetDelay(UnityEngine.Random.Range(0f, 0.2f));
                }
            }
        }
    }

    public void Cleanup()
    {
        DOTween.KillAll();

        if (gemViewDict != null)
        {
            foreach (var kvp in gemViewDict)
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value.gameObject);
                }
            }
            gemViewDict.Clear();
        }
    }
}
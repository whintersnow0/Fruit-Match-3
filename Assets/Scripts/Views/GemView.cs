using UnityEngine;
using DG.Tweening;
using System;

public class GemView : MonoBehaviour, IView
{
    [Header("Visual Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ParticleSystem destroyEffect;
    [SerializeField] private GameObject selectionHighlight;

    [Header("Animation Settings")]
    [SerializeField] private float highlightDuration = 0.5f;
    [SerializeField] private float destroyDuration = 0.3f;
    [SerializeField] private Vector3 highlightScale = Vector3.one * 1.2f;

    private GemModel gemModel;
    private Vector2Int gridPosition;
    private bool isSelected = false;
    public Vector2Int GridPosition => gridPosition;
    public GemModel Model => gemModel;

    public void Initialize()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (selectionHighlight != null)
            selectionHighlight.SetActive(false);
    }

    public void Initialize(GemModel model, Vector2Int position)
    {
        Initialize();
        UpdateModel(model);
        UpdateGridPosition(position);
    }

    public void UpdateModel(GemModel newModel)
    {
        gemModel = newModel;

        if (gemModel != null)
        {
            UpdateVisual();

            gemModel.OnPositionChanged += HandlePositionChanged;
            gemModel.OnTypeChanged += HandleTypeChanged;
        }
    }

    public void UpdateGridPosition(Vector2Int newPosition)
    {
        gridPosition = newPosition;
    }

    private void UpdateVisual()
    {
        if (spriteRenderer != null)
        {
           
        }
    }

    public void Select()
    {
        isSelected = true;
        if (selectionHighlight != null)
        {
            selectionHighlight.SetActive(true);
        }

        transform.DOScale(highlightScale, 0.2f).SetEase(Ease.OutElastic);
    }

    public void Deselect()
    {
        isSelected = false;
        if (selectionHighlight != null)
        {
            selectionHighlight.SetActive(false);
        }

        transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutElastic);
    }

    public void PlayMatchHighlight()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(highlightScale, highlightDuration * 0.5f));
        sequence.Append(transform.DOScale(Vector3.one, highlightDuration * 0.5f));
        sequence.SetLoops(2);

        if (spriteRenderer != null)
        {
            var originalColor = spriteRenderer.color;
            var highlightColor = Color.white;

            spriteRenderer.DOColor(highlightColor, highlightDuration * 0.5f)
                .SetLoops(2, LoopType.Yoyo)
                .OnComplete(() => spriteRenderer.color = originalColor);
        }
    }

    public void PlayDestroyAnimation(Action onComplete = null)
    {
        if (destroyEffect != null)
        {
            destroyEffect.Play();
        }

        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(Vector3.zero, destroyDuration).SetEase(Ease.InBack));
        sequence.Join(spriteRenderer.DOFade(0f, destroyDuration));
        sequence.OnComplete(() => onComplete?.Invoke());
    }

    private void HandlePositionChanged(Vector2Int newPosition)
    {
        UpdateGridPosition(newPosition);
    }

    private void HandleTypeChanged(GemType newType)
    {
        UpdateVisual();
    }

    public void UpdateView()
    {
        UpdateVisual();
    }

    public void Cleanup()
    {
        if (gemModel != null)
        {
            gemModel.OnPositionChanged -= HandlePositionChanged;
            gemModel.OnTypeChanged -= HandleTypeChanged;
        }

        DOTween.Kill(transform);
        DOTween.Kill(spriteRenderer);
    }

    private void OnDestroy()
    {
        Cleanup();
    }
}
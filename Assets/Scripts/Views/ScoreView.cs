using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ScoreView : MonoBehaviour, IView
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI currentScoreText;
    [SerializeField] private TextMeshProUGUI targetScoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI multiplierText;
    [SerializeField] private Slider progressSlider;

    [Header("Animation Settings")]
    [SerializeField] private float scoreAnimationDuration = 0.5f;
    [SerializeField] private Vector3 scorePopScale = Vector3.one * 1.2f;

    [Header("Model Reference")]
    [SerializeField] private ScoreModel scoreModel;

    private int displayedScore = 0;

    public void Initialize()
    {
        if (scoreModel != null)
        {
            scoreModel.OnModelChanged += UpdateView;
            scoreModel.OnScoreChanged += AnimateScoreChange;
            scoreModel.OnComboChanged += AnimateComboChange;
            scoreModel.OnTargetReached += HandleTargetReached;
        }

        UpdateView();
    }

    public void UpdateView()
    {
        if (scoreModel == null) return;

        UpdateScoreDisplay();
        UpdateTargetScore();
        UpdateBestScore();
        UpdateCombo();
        UpdateMultiplier();
        UpdateProgressBar();
    }

    private void UpdateScoreDisplay()
    {
        if (currentScoreText != null)
        {
            currentScoreText.text = displayedScore.ToString("N0");
        }
    }

    private void UpdateTargetScore()
    {
        if (targetScoreText != null)
        {
            targetScoreText.text = $"Target: {scoreModel.TargetScore:N0}";
        }
    }

    private void UpdateBestScore()
    {
        if (bestScoreText != null)
        {
            bestScoreText.text = $"Best: {scoreModel.BestScore:N0}";
        }
    }

    private void UpdateCombo()
    {
        if (comboText != null)
        {
            if (scoreModel.Combo > 0)
            {
                comboText.text = $"Combo x{scoreModel.Combo}";
                comboText.gameObject.SetActive(true);
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateMultiplier()
    {
        if (multiplierText != null)
        {
            if (scoreModel.Multiplier > 1)
            {
                multiplierText.text = $"x{scoreModel.Multiplier}";
                multiplierText.gameObject.SetActive(true);
            }
            else
            {
                multiplierText.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateProgressBar()
    {
        if (progressSlider != null && scoreModel.TargetScore > 0)
        {
            float progress = Mathf.Clamp01((float)scoreModel.CurrentScore / scoreModel.TargetScore);
            progressSlider.DOValue(progress, 0.3f).SetEase(Ease.OutCubic);
        }
    }

    private void AnimateScoreChange(int newScore)
    {
        DOTween.To(() => displayedScore, x => {
            displayedScore = x;
            UpdateScoreDisplay();
        }, newScore, scoreAnimationDuration).SetEase(Ease.OutCubic);

        if (currentScoreText != null)
        {
            currentScoreText.transform.DOPunchScale(scorePopScale * 0.2f, 0.3f, 5, 0.5f);
        }
    }

    private void AnimateComboChange(int newCombo)
    {
        if (comboText != null && newCombo > 0)
        {
            comboText.transform.DOPunchScale(scorePopScale * 0.3f, 0.4f, 8, 0.6f);

            var originalColor = comboText.color;
            comboText.DOColor(Color.yellow, 0.2f).SetLoops(2, LoopType.Yoyo)
                .OnComplete(() => comboText.color = originalColor);
        }
    }

    private void HandleTargetReached()
    {
        if (progressSlider != null)
        {
            progressSlider.transform.DOPunchScale(Vector3.one * 0.1f, 0.5f, 10, 1f);
        }
        PlayTargetReachedEffect();
    }

    private void PlayTargetReachedEffect()
    {
        Debug.Log("Target Score Reached!");
    }

    public void Cleanup()
    {
        if (scoreModel != null)
        {
            scoreModel.OnModelChanged -= UpdateView;
            scoreModel.OnScoreChanged -= AnimateScoreChange;
            scoreModel.OnComboChanged -= AnimateComboChange;
            scoreModel.OnTargetReached -= HandleTargetReached;
        }

        DOTween.Kill(this);
    }
}
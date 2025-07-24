using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Match3/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Grid Settings")]
    public int gridWidth = 8;
    public int gridHeight = 8;
    public float gemSpacing = 1f;

    [Header("Game Rules")]
    public float levelTimeLimit = 120f;
    public int baseScore = 100;
    public int targetScore = 1000;

    [Header("Match Settings")]
    public int minMatchLength = 3;
    public int comboMultiplierIncrement = 3;

    [Header("Special Gems")]
    public int bombMatchLength = 4;
    public int lightningMatchLength = 5;
    public int rainbowMatchLength = 6;

    [Header("Animation Durations")]
    public float swapDuration = 0.3f;
    public float destroyDuration = 0.2f;
    public float fallDuration = 0.5f;
    public float highlightDuration = 0.5f;
}
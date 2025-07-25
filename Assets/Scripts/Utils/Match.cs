using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Match
{
    public List<Vector2Int> Positions { get; private set; }
    public GemType GemType { get; private set; }
    public MatchType Type { get; private set; }
    public int Score { get; private set; }

    public Match(List<Vector2Int> positions, GemType gemType, MatchType type)
    {
        Positions = positions;
        GemType = gemType;
        Type = type;
        Score = CalculateScore();
    }

    private int CalculateScore()
    {
        int baseScore = 100;
        int multiplier = Positions.Count - 2;
        return baseScore * multiplier;
    }
}

public enum MatchType
{
    Horizontal,
    Vertical,
    Complex,
    LShape,
    TShape
}
using UnityEngine;

public struct GameStartedEvent : IGameEvent
{
    public int Level { get; }

    public GameStartedEvent(int level)
    {
        Level = level;
    }
}

public struct GameEndedEvent : IGameEvent
{
    public int FinalScore { get; }
    public bool IsVictory { get; }

    public GameEndedEvent(int finalScore, bool isVictory)
    {
        FinalScore = finalScore;
        IsVictory = isVictory;
    }
}

public struct MatchFoundEvent : IGameEvent
{
    public Match Match { get; }

    public MatchFoundEvent(Match match)
    {
        Match = match;
    }
}

public struct ComboAchievedEvent : IGameEvent
{
    public int ComboCount { get; }
    public int Multiplier { get; }

    public ComboAchievedEvent(int comboCount, int multiplier)
    {
        ComboCount = comboCount;
        Multiplier = multiplier;
    }
}

public struct ScoreChangedEvent : IGameEvent
{
    public int NewScore { get; }
    public int ScoreDelta { get; }

    public ScoreChangedEvent(int newScore, int scoreDelta)
    {
        NewScore = newScore;
        ScoreDelta = scoreDelta;
    }
}
using UnityEngine;
using System;

public class GemModel : IModel
{
    private int x, y;
    private GemType type;
    private bool isSpecial;
    private SpecialGemType specialType;
    private static int _counter = 0;
    public int Id { get; private set; }

    public int X => x;
    public int Y => y;
    public GemType Type => type;
    public bool IsSpecial => isSpecial;
    public SpecialGemType SpecialType => specialType;
    public Vector2Int Position => new Vector2Int(x, y);

    public event Action<Vector2Int> OnPositionChanged;
    public event Action<GemType> OnTypeChanged;
    public event Action OnModelChanged;

    public GemModel(int x, int y, GemType type)
    {
        this.x = x;
        this.y = y;
        this.type = type;
        this.isSpecial = false;
        this.specialType = SpecialGemType.None;
        Id = ++_counter;
        Debug.Log($"[GemModel] Created gem ID={Id}, Type={type}, Pos=({x},{y})");
    }

    public void UpdatePosition(int newX, int newY)
    {
        Debug.Log($"[GemModel] Gem ID={Id}, Type={type} moved: ({x},{y}) -> ({newX},{newY})");
        x = newX;
        y = newY;
        OnPositionChanged?.Invoke(new Vector2Int(x, y));
    }

    public void SetType(GemType newType)
    {
        Debug.Log($"[GemModel] Gem ID={Id} type changed: {type} -> {newType} at ({x},{y})");
        type = newType;
        OnTypeChanged?.Invoke(type);
        OnModelChanged?.Invoke();
    }

    public void SetSpecial(SpecialGemType specialType)
    {
        this.specialType = specialType;
        isSpecial = true;
        Debug.Log($"[GemModel] Gem ID={Id}, Type={type} became special: {specialType} at ({x},{y})");
        OnModelChanged?.Invoke();
    }

    public void RemoveSpecial()
    {
        Debug.Log($"[GemModel] Gem ID={Id}, Type={type} lost special ({specialType}) at ({x},{y})");
        isSpecial = false;
        specialType = SpecialGemType.None;
        OnModelChanged?.Invoke();
    }

    public void Initialize()
    {
    }

    public void Cleanup()
    {
    }

    public override string ToString()
    {
        return $"Gem(ID={Id},Type={type},Special={specialType},Pos=({x},{y}))";
    }
}

public enum GemType
{
    Red,
    Blue,
    Green,
    Yellow,
    Purple,
    Orange
}

public enum SpecialGemType
{
    None,
    Bomb,
    Lightning,
    Rainbow,
    Star
}
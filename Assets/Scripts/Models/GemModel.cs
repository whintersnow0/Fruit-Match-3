using UnityEngine;
using System;

public class GemModel
{
    private int x, y;
    private GemType type;
    private bool isSpecial;
    private SpecialGemType specialType;

    public int X => x;
    public int Y => y;
    public GemType Type => type;
    public bool IsSpecial => isSpecial;
    public SpecialGemType SpecialType => specialType;
    public Vector2Int Position => new Vector2Int(x, y);

    public event Action<Vector2Int> OnPositionChanged;
    public event Action<GemType> OnTypeChanged;

    public GemModel(int x, int y, GemType type)
    {
        this.x = x;
        this.y = y;
        this.type = type;
        this.isSpecial = false;
    }

    public void UpdatePosition(int newX, int newY)
    {
        x = newX;
        y = newY;
        OnPositionChanged?.Invoke(new Vector2Int(x, y));
    }

    public void SetSpecial(SpecialGemType specialType)
    {
        this.specialType = specialType;
        isSpecial = true;
    }

    public void RemoveSpecial()
    {
        isSpecial = false;
        specialType = SpecialGemType.None;
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
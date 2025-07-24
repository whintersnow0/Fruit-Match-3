using UnityEngine;
using System.Collections.Generic;
using System;

public class GridModel : BaseModel
{
    [Header("Grid Settings")]
    [SerializeField] private int width = 8;
    [SerializeField] private int height = 8;

    private GemModel[,] grid;
    private List<Match> currentMatches;
    private bool isProcessing;

    public int Width => width;
    public int Height => height;
    public bool IsProcessing => isProcessing;
    public List<Match> CurrentMatches => currentMatches;

    public event Action<Vector2Int, Vector2Int> OnGemsSwapped;
    public event Action<List<Match>> OnMatchesFound;
    public event Action<List<Vector2Int>> OnGemsDestroyed;
    public event Action OnGridRefilled;

    public override void Initialize()
    {
        grid = new GemModel[width, height];
        currentMatches = new List<Match>();
        GenerateInitialGrid();
    }

    private void GenerateInitialGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GemType gemType = GetRandomGemTypeWithoutMatches(x, y);
                grid[x, y] = new GemModel(x, y, gemType);
            }
        }
        NotifyModelChanged();
    }

    private GemType GetRandomGemTypeWithoutMatches(int x, int y)
    {
        List<GemType> availableTypes = new List<GemType>();
        for (int i = 0; i < System.Enum.GetValues(typeof(GemType)).Length; i++)
        {
            availableTypes.Add((GemType)i);
        }

        if (x >= 2 &&
            grid[x - 1, y] != null && grid[x - 2, y] != null &&
            grid[x - 1, y].Type == grid[x - 2, y].Type)
        {
            availableTypes.Remove(grid[x - 1, y].Type);
        }
      
        if (y >= 2 &&
            grid[x, y - 1] != null && grid[x, y - 2] != null &&
            grid[x, y - 1].Type == grid[x, y - 2].Type)
        {
            availableTypes.Remove(grid[x, y - 1].Type);
        }

        return availableTypes[UnityEngine.Random.Range(0, availableTypes.Count)];
    }

    public GemModel GetGem(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return null;
        return grid[x, y];
    }

    public bool TrySwapGems(Vector2Int pos1, Vector2Int pos2)
    {
        if (isProcessing) return false;

        if (!AreAdjacent(pos1, pos2)) return false;

        var gem1 = GetGem(pos1.x, pos1.y);
        var gem2 = GetGem(pos2.x, pos2.y);

        if (gem1 == null || gem2 == null) return false;
 
        SwapGems(pos1, pos2);

        var matches = FindMatches();
        if (matches.Count > 0)
        {
            OnGemsSwapped?.Invoke(pos1, pos2);
            ProcessMatches(matches);
            return true;
        }
        else
        {
            SwapGems(pos1, pos2);
            return false;
        }
    }

    private void SwapGems(Vector2Int pos1, Vector2Int pos2)
    {
        var gem1 = grid[pos1.x, pos1.y];
        var gem2 = grid[pos2.x, pos2.y];

        grid[pos1.x, pos1.y] = gem2;
        grid[pos2.x, pos2.y] = gem1;

        gem1.UpdatePosition(pos2.x, pos2.y);
        gem2.UpdatePosition(pos1.x, pos1.y);
    }

    private bool AreAdjacent(Vector2Int pos1, Vector2Int pos2)
    {
        int xDiff = Mathf.Abs(pos1.x - pos2.x);
        int yDiff = Mathf.Abs(pos1.y - pos2.y);
        return (xDiff == 1 && yDiff == 0) || (xDiff == 0 && yDiff == 1);
    }

    private List<Match> FindMatches()
    {
        var matches = new List<Match>();
        bool[,] visited = new bool[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width - 2; x++)
            {
                if (visited[x, y]) continue;

                var currentGem = grid[x, y];
                if (currentGem == null) continue;

                var matchPositions = new List<Vector2Int> { new Vector2Int(x, y) };

                for (int i = x + 1; i < width; i++)
                {
                    if (grid[i, y] != null && grid[i, y].Type == currentGem.Type)
                    {
                        matchPositions.Add(new Vector2Int(i, y));
                    }
                    else
                    {
                        break;
                    }
                }

                if (matchPositions.Count >= 3)
                {
                    matches.Add(new Match(matchPositions, currentGem.Type, MatchType.Horizontal));
                    foreach (var pos in matchPositions)
                    {
                        visited[pos.x, pos.y] = true;
                    }
                }
            }
        }

        visited = new bool[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height - 2; y++)
            {
                if (visited[x, y]) continue;

                var currentGem = grid[x, y];
                if (currentGem == null) continue;

                var matchPositions = new List<Vector2Int> { new Vector2Int(x, y) };

                for (int i = y + 1; i < height; i++)
                {
                    if (grid[x, i] != null && grid[x, i].Type == currentGem.Type)
                    {
                        matchPositions.Add(new Vector2Int(x, i));
                    }
                    else
                    {
                        break;
                    }
                }

                if (matchPositions.Count >= 3)
                {
                    matches.Add(new Match(matchPositions, currentGem.Type, MatchType.Vertical));
                    foreach (var pos in matchPositions)
                    {
                        visited[pos.x, pos.y] = true;
                    }
                }
            }
        }

        return matches;
    }

    private void ProcessMatches(List<Match> matches)
    {
        isProcessing = true;
        currentMatches = matches;

        var destroyedPositions = new List<Vector2Int>();
        foreach (var match in matches)
        {
            destroyedPositions.AddRange(match.Positions);
        }

        OnMatchesFound?.Invoke(matches);
        OnGemsDestroyed?.Invoke(destroyedPositions);

        foreach (var pos in destroyedPositions)
        {
            grid[pos.x, pos.y] = null;
        }

        RefillGrid();
    }

    private void RefillGrid()
    {
        for (int x = 0; x < width; x++)
        {
            int writeIndex = 0;
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != null)
                {
                    if (writeIndex != y)
                    {
                        grid[x, writeIndex] = grid[x, y];
                        grid[x, y] = null;
                        grid[x, writeIndex].UpdatePosition(x, writeIndex);
                    }
                    writeIndex++;
                }
            }

            for (int y = writeIndex; y < height; y++)
            {
                GemType newType = (GemType)UnityEngine.Random.Range(0,
                    System.Enum.GetValues(typeof(GemType)).Length);
                grid[x, y] = new GemModel(x, y, newType);
            }
        }

        OnGridRefilled?.Invoke();

        var newMatches = FindMatches();
        if (newMatches.Count > 0)
        {
            ProcessMatches(newMatches);
        }
        else
        {
            isProcessing = false;
            NotifyModelChanged();
        }
    }
}

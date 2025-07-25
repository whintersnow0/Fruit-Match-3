using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridModel : MonoBehaviour
{
    [SerializeField] private int width = 8;
    [SerializeField] private int height = 8;

    private GemModel[,] grid;
    private bool isProcessing;

    public int Width => width;
    public int Height => height;
    public bool IsProcessing => isProcessing;

    public event Action<Vector2Int, Vector2Int> OnGemsSwapped;
    public event Action<List<Match>> OnMatchesFound;
    public event Action<List<Vector2Int>> OnGemsDestroyed;
    public event Action OnGridRefilled;
    public event Action OnModelChanged;

    public void Initialize()
    {
        grid = new GemModel[width, height];
        GenerateInitialGrid();
        AssertGridIntegrity("After Init");
        StartCoroutine(ClearAllMatchesRoutine());
    }

    private void GenerateInitialGrid()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = new GemModel(x, y, GetRandomGemType(x, y));
        OnModelChanged?.Invoke();
        Debug.Log("Grid generated.");
        PrintGrid();
    }

    private GemType GetRandomGemType(int x, int y)
    {
        List<GemType> available = new List<GemType>();
        foreach (GemType t in Enum.GetValues(typeof(GemType)))
            available.Add(t);

        if (x >= 2 && grid[x - 1, y] != null && grid[x - 2, y] != null &&
            grid[x - 1, y].Type == grid[x - 2, y].Type)
            available.Remove(grid[x - 1, y].Type);

        if (y >= 2 && grid[x, y - 1] != null && grid[x, y - 2] != null &&
            grid[x, y - 1].Type == grid[x, y - 2].Type)
            available.Remove(grid[x, y - 1].Type);

        if (available.Count == 0)
            return (GemType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(GemType)).Length);

        return available[UnityEngine.Random.Range(0, available.Count)];
    }

    public GemModel GetGem(int x, int y)
    {
        if (!IsInBounds(new Vector2Int(x, y)))
            return null;
        return grid[x, y];
    }

    public bool TrySwapGems(Vector2Int pos1, Vector2Int pos2)
    {
        if (isProcessing) return false;
        if (!IsAdjacent(pos1, pos2)) return false;
        if (!IsInBounds(pos1) || !IsInBounds(pos2)) return false;
        if (grid[pos1.x, pos1.y] == null || grid[pos2.x, pos2.y] == null) return false;
        Debug.Log($"TrySwapGems: {pos1} [{grid[pos1.x, pos1.y]?.Type}] <-> {pos2} [{grid[pos2.x, pos2.y]?.Type}]");

        SwapGems(pos1, pos2);

        PrintGrid();
        AssertGridIntegrity("After Swap");

        var matches = FindAllMatches();
        PrintMatches(matches);

        if (matches.Count > 0)
        {
            OnGemsSwapped?.Invoke(pos1, pos2);
            StartCoroutine(ProcessMatchesRoutine());
            return true;
        }
        else
        {
            Debug.Log("No matches after swap, reverting.");
            SwapGems(pos1, pos2);
            PrintGrid();
            AssertGridIntegrity("After Swap Revert");
            return false;
        }
    }

    private void SwapGems(Vector2Int pos1, Vector2Int pos2)
    {
        var gem1 = grid[pos1.x, pos1.y];
        var gem2 = grid[pos2.x, pos2.y];
        grid[pos1.x, pos1.y] = gem2;
        grid[pos2.x, pos2.y] = gem1;
        grid[pos1.x, pos1.y]?.UpdatePosition(pos1.x, pos1.y);
        grid[pos2.x, pos2.y]?.UpdatePosition(pos2.x, pos2.y);
        Debug.Log($"Swapped: {pos1} [{gem2?.Type}] <-> {pos2} [{gem1?.Type}]");
    }

    private bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        if (a == b) return false;
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
    }

    private bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    private List<Match> FindAllMatches()
    {
        List<Match> matches = new List<Match>();
        bool[,] marked = new bool[width, height];

        for (int y = 0; y < height; y++)
        {
            int streak = 1;
            for (int x = 1; x < width; x++)
            {
                if (grid[x, y] != null && grid[x - 1, y] != null && grid[x, y].Type == grid[x - 1, y].Type)
                    streak++;
                else
                {
                    if (streak >= 3)
                        for (int k = 0; k < streak; k++)
                            marked[x - 1 - k, y] = true;
                    streak = 1;
                }
            }
            if (streak >= 3)
                for (int k = 0; k < streak; k++)
                    marked[width - 1 - k, y] = true;
        }

        for (int x = 0; x < width; x++)
        {
            int streak = 1;
            for (int y = 1; y < height; y++)
            {
                if (grid[x, y] != null && grid[x, y - 1] != null && grid[x, y].Type == grid[x, y - 1].Type)
                    streak++;
                else
                {
                    if (streak >= 3)
                        for (int k = 0; k < streak; k++)
                            marked[x, y - 1 - k] = true;
                    streak = 1;
                }
            }
            if (streak >= 3)
                for (int k = 0; k < streak; k++)
                    marked[x, height - 1 - k] = true;
        }

        bool[,] visited = new bool[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (marked[x, y] && !visited[x, y] && grid[x, y] != null)
                {
                    List<Vector2Int> group = new List<Vector2Int>();
                    GemType type = grid[x, y].Type;
                    Queue<Vector2Int> queue = new Queue<Vector2Int>();
                    queue.Enqueue(new Vector2Int(x, y));
                    visited[x, y] = true;
                    while (queue.Count > 0)
                    {
                        var p = queue.Dequeue();
                        group.Add(p);
                        Vector2Int[] nbs = {
                        new Vector2Int(p.x+1, p.y),
                        new Vector2Int(p.x-1, p.y),
                        new Vector2Int(p.x, p.y+1),
                        new Vector2Int(p.x, p.y-1)
                    };
                        foreach (var n in nbs)
                        {
                            if (n.x >= 0 && n.x < width && n.y >= 0 && n.y < height &&
                                marked[n.x, n.y] && !visited[n.x, n.y] &&
                                grid[n.x, n.y] != null && grid[n.x, n.y].Type == type)
                            {
                                queue.Enqueue(n);
                                visited[n.x, n.y] = true;
                            }
                        }
                    }
                    if (group.Count >= 3)
                        matches.Add(new Match(group, type, MatchType.Horizontal));
                }
            }
        return matches;
    }

    private IEnumerator ClearAllMatchesRoutine()
    {
        yield return null;
        int guard = 0;
        while (true)
        {
            var matches = FindAllMatches();
            PrintMatches(matches);
            if (matches.Count == 0)
            {
                OnModelChanged?.Invoke();
                break;
            }
            yield return StartCoroutine(DestroyMatchesAndCollapse(matches));
            guard++;
            if (guard > 100) break;
        }
    }

    private IEnumerator ProcessMatchesRoutine()
    {
        isProcessing = true;
        int guard = 0;
        while (true)
        {
            var matches = FindAllMatches();
            PrintMatches(matches);
            if (matches.Count == 0) break;
            OnMatchesFound?.Invoke(matches);
            yield return new WaitForSeconds(0.3f);
            HashSet<Vector2Int> destroyed = new HashSet<Vector2Int>();
            foreach (var match in matches)
                foreach (var pos in match.Positions)
                    if (grid[pos.x, pos.y] != null)
                    {
                        destroyed.Add(pos);
                        grid[pos.x, pos.y] = null;
                    }
            Debug.Log($"Destroyed {destroyed.Count} gems: {string.Join(", ", destroyed)}");
            OnGemsDestroyed?.Invoke(new List<Vector2Int>(destroyed));
            yield return new WaitForSeconds(0.2f);
            CollapseColumns();
            PrintGrid();
            FillEmptyCells();
            PrintGrid();
            OnGridRefilled?.Invoke();
            yield return new WaitForSeconds(0.7f);
            UpdateAllGemPositions();
            guard++;
            if (guard > 100) break;
        }
        isProcessing = false;
        OnModelChanged?.Invoke();
    }

    private IEnumerator DestroyMatchesAndCollapse(List<Match> matches)
    {
        OnMatchesFound?.Invoke(matches);
        yield return new WaitForSeconds(0.3f);
        HashSet<Vector2Int> destroyed = new HashSet<Vector2Int>();
        foreach (var match in matches)
            foreach (var pos in match.Positions)
                if (grid[pos.x, pos.y] != null)
                {
                    destroyed.Add(pos);
                    grid[pos.x, pos.y] = null;
                }
        Debug.Log($"Destroyed {destroyed.Count} gems: {string.Join(", ", destroyed)}");
        OnGemsDestroyed?.Invoke(new List<Vector2Int>(destroyed));
        yield return new WaitForSeconds(0.2f);
        CollapseColumns();
        PrintGrid();
        FillEmptyCells();
        PrintGrid();
        OnGridRefilled?.Invoke();
        yield return new WaitForSeconds(0.7f);
        UpdateAllGemPositions();
    }

    private void CollapseColumns()
    {
        for (int x = 0; x < width; x++)
        {
            int empty = 0;
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null)
                    empty++;
                else if (empty > 0)
                {
                    grid[x, y - empty] = grid[x, y];
                    grid[x, y] = null;
                    grid[x, y - empty]?.UpdatePosition(x, y - empty);
                }
            }
        }
    }

    private void FillEmptyCells()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (grid[x, y] == null)
                    grid[x, y] = new GemModel(x, y, (GemType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(GemType)).Length));
    }

    private void UpdateAllGemPositions()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y]?.UpdatePosition(x, y);
    }

    private void PrintGrid()
    {
        string s = "GRID:\n";
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
                s += grid[x, y] != null ? $"{grid[x, y].Type.ToString()[0]} " : ". ";
            s += "\n";
        }
        Debug.Log(s);
    }

    private void PrintMatches(List<Match> matches)
    {
        if (matches == null || matches.Count == 0)
        {
            Debug.Log("No matches found.");
            return;
        }
        Debug.Log($"MATCHES ({matches.Count}):");
        for (int i = 0; i < matches.Count; i++)
        {
            var m = matches[i];
            Debug.Log($"Match {i + 1}: Type={m.GemType}, Count={m.Positions.Count}, Positions: {string.Join(", ", m.Positions)}");
        }
    }

    private void AssertGridIntegrity(string label)
    {
        HashSet<int> ids = new HashSet<int>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var gem = grid[x, y];
                if (gem == null)
                {
                    Debug.LogError($"[INTEGRITY:{label}] Null gem at ({x},{y})");
                    throw new Exception("Grid integrity broken: null gem");
                }
                if (!ids.Add(gem.Id))
                {
                    Debug.LogError($"[INTEGRITY:{label}] Duplicate gem ID {gem.Id} at ({x},{y})");
                    throw new Exception("Grid integrity broken: duplicate gem ID");
                }
                if (gem.X != x || gem.Y != y)
                {
                    Debug.LogError($"[INTEGRITY:{label}] Gem ID={gem.Id} position mismatch: model=({gem.X},{gem.Y}) grid=({x},{y})");
                    throw new Exception("Grid integrity broken: wrong gem position");
                }
            }
    }
}
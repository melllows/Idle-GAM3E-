using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
    {
    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int target)
    {
        Debugger.Instance.Log($"🔍 Finding path from {start} to {target}");

        if (!GridManager.Instance.IsWalkable(target))
        {
            Debugger.Instance.Log($"❌ Target {target} is not walkable!");
            return new List<Vector2Int>();
        }

        PriorityQueue<Vector2Int> openSet = new();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new();
        Dictionary<Vector2Int, float> gScore = new();
        Dictionary<Vector2Int, float> fScore = new();

        openSet.Enqueue(start, 0);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, target);

        int loopCounter = 0;

        while (openSet.Count > 0)
        {
            if (++loopCounter > 5000)
            {
                Debug.LogError($"❌ Pathfinding failed: Too many iterations from {start} to {target}.");
                return new List<Vector2Int>(); 
            }

            Vector2Int current = openSet.Dequeue();

            if (current == target)
            {
                return ReconstructPath(cameFrom, current);
            }

            foreach (Vector2Int neighbor in GridManager.Instance.GetNeighbors(current))
            {
                if (!GridManager.Instance.IsWalkable(neighbor)) continue; // ✅ Avoid storage tiles

                float tentativeG = gScore[current] + 1f;

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, target);

                    if (!openSet.Contains(neighbor))
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }

        Debug.LogError($"❌ No valid path found from {start} to {target}!");
        return new List<Vector2Int>();
    }

    private static List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        List<Vector2Int> path = new() { current };

        while (cameFrom.ContainsKey(current))
        {
            Vector2Int next = cameFrom[current];

            while (cameFrom.ContainsKey(next) && IsStraightLine(current, cameFrom[next]))
            {
                next = cameFrom[next];
            }

            path.Insert(0, next);
            current = next;
        }

        return path;
    }

    private static bool IsStraightLine(Vector2Int a, Vector2Int b)
    {
        return a.x == b.x || a.y == b.y;
    }

    private static int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}

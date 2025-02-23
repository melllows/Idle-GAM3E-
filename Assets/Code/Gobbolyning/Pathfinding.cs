using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
{
   public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int target)
    {
        Debugger.Instance.Log($"Finding path from {start} to {target}");

        if (!GridManager.Instance.IsWalkable(target))
        {
            Debugger.Instance.Log($"Target {target} is not walkable!");
            return new List<Vector2Int>();
        }

        PriorityQueue<Vector2Int> openSet = new();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new();
        Dictionary<Vector2Int, float> gScore = new();
        Dictionary<Vector2Int, float> fScore = new();

        openSet.Enqueue(start, 0);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, target);

        while (openSet.Count > 0)
        {
            Vector2Int current = openSet.Dequeue();

            if (current == target)
            {
                List<Vector2Int> path = ReconstructPath(cameFrom, current);
                Debugger.Instance.Log($"Path found: {string.Join(" -> ", path)}");
                return path;
            }

            foreach (Vector2Int neighbor in GridManager.Instance.GetNeighbors(current))
            {
                if (!GridManager.Instance.IsValidMove(current, neighbor)) continue;

                float movementCost;
                int dx = Mathf.Abs(neighbor.x - current.x);
                int dy = Mathf.Abs(neighbor.y - current.y);

                if (dx == 1 && dy == 1)
                    movementCost = 1.5f;
                else if (dx == 2 || dy == 2)
                    movementCost = 2.2f;
                else if (dx == 3 || dy == 3)
                    movementCost = 3f;
                else
                    movementCost = 1f;

                float tentativeG = gScore[current] + movementCost;

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

        Debug.LogError($"No valid path found from {start} to {target}!");
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

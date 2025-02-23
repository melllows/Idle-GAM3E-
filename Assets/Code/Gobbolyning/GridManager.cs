using UnityEngine;
using System;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    public Tilemap placementTilemap;
    public Tilemap pathfindingTilemap;
    public Tilemap obstacleTilemap;
    public float tileSize = 0.5f;

    public event Action OnGridUpdated;

    private Dictionary<Vector2Int, bool> grid = new();
    private HashSet<Vector2Int> occupiedTiles = new();


    private void Awake()
    {
        Instance = this;
        GenerateGrid();
    }

    public void UpdateWalkability(Vector2Int position, bool isWalkable)
    {
        grid[position] = isWalkable;

        if (!isWalkable)
            occupiedTiles.Add(position);
        else
            occupiedTiles.Remove(position);
    }


    void GenerateGrid()
    {
        grid.Clear();
        occupiedTiles.Clear();
        BoundsInt bounds = pathfindingTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                Vector2Int gridPosition = new Vector2Int(x, y);
                bool isWalkable = pathfindingTilemap.HasTile(tilePosition) && !obstacleTilemap.HasTile(tilePosition);
                grid[gridPosition] = isWalkable;
            }
        }
    }

    public bool IsWalkable(Vector2Int position)
    {
        if (!grid.ContainsKey(position) || !grid[position])
            return false;

        Vector3 worldPos = pathfindingTilemap.CellToWorld(new Vector3Int(position.x, position.y, 0));
        Collider2D collider = Physics2D.OverlapBox(worldPos, Vector2.one * tileSize * 0.9f, 0);

        return collider == null;
    }

    public Vector2Int GetNearestPathTile(Vector2 worldPosition)
    {
        Vector3Int cellPos = pathfindingTilemap.WorldToCell(worldPosition);
        Vector2Int tilePos = new Vector2Int(cellPos.x, cellPos.y);
        
        return tilePos;
    }

    public List<Vector2Int> GetNeighbors(Vector2Int position)
    {
        List<Vector2Int> neighbors = new()
        {
            position + Vector2Int.up,    
            position + Vector2Int.down,  
            position + Vector2Int.left,  
            position + Vector2Int.right, 

            position + new Vector2Int(-1, 1),  
            position + new Vector2Int(1, 1),   
            position + new Vector2Int(-1, -1), 
            position + new Vector2Int(1, -1),  

            position + new Vector2Int(-2, 1),  
            position + new Vector2Int(2, 1),   
            position + new Vector2Int(-2, -1), 
            position + new Vector2Int(2, -1),  
            position + new Vector2Int(-1, 2),  
            position + new Vector2Int(1, 2),   
            position + new Vector2Int(-1, -2), 
            position + new Vector2Int(1, -2)
        };
        neighbors.RemoveAll(n => !IsValidMove(position, n));
        return neighbors;
    }

    public bool IsValidMove(Vector2Int start, Vector2Int end)
    {
        if (!IsWalkable(end)) return false;

        int dx = end.x - start.x;
        int dy = end.y - start.y;

        if (dx != 0 && dy != 0)
        {
            Vector2Int check1 = new Vector2Int(start.x + dx, start.y);
            Vector2Int check2 = new Vector2Int(start.x, start.y + dy);

            if (!IsWalkable(check1) || !IsWalkable(check2))
                return false;
        }

        return true;
    }

    public bool AreTilesAdjacent(Vector2Int tileA, Vector2Int tileB)
    {
        if (tileA == tileB) return true;

        return Mathf.Abs(tileA.x - tileB.x) <= 1 && Mathf.Abs(tileA.y - tileB.y) <= 1;
    }

    public bool CanPlaceHere(Vector2Int tilePosition)
    {
        Vector3Int cellPosition = new Vector3Int(tilePosition.x, tilePosition.y, 0);

        if (placementTilemap.HasTile(cellPosition))
        {
            Debugger.Instance.Log($"Cannot place here, tile is occupied at {tilePosition}");
            return false;
        }

        return true;
    }

    public void MarkTileOccupied(Vector2Int position, GameObject placedObject)
    {
        if (!occupiedTiles.Contains(position))
        {
            occupiedTiles.Add(position);
            Debugger.Instance.Log($"Tile at {position} is now occupied by {placedObject.name}");
        }
        else
        {
            Debugger.Instance.Log($"Tile at {position} is already occupied.");
        }

        OnGridUpdated?.Invoke();
    }


    public Vector2Int GetNearestPlacementTile(Vector2 worldPosition)
    {
        Vector3Int cellPos = placementTilemap.WorldToCell(worldPosition);
        return new Vector2Int(cellPos.x, cellPos.y);
    }

    public bool IsOccupied(Vector2Int position)
    {
        return occupiedTiles.Contains(position);
    }
    private void OnDrawGizmos()
    {
        if (grid == null) return;

        foreach (var tile in grid)
        {
            Vector3 worldPos = pathfindingTilemap.CellToWorld(new Vector3Int(tile.Key.x, tile.Key.y, 0));

            if (occupiedTiles.Contains(tile.Key)) 
            {
                if (tile.Value) 
                {
                    Gizmos.color = Color.blue; // Walkable but occupied → BLUE
                } 
                else 
                {
                    Gizmos.color = Color.red; // Unwalkable → RED
                }
            } 
            else 
            {
                Gizmos.color = Color.green; // Empty walkable tile → GREEN
            }

            Gizmos.DrawWireCube(worldPos, Vector3.one * tileSize * 0.9f);
        }
    }
}

using UnityEngine;
using System.Collections.Generic;

public class StorageBuilding : MonoBehaviour
{
    void Awake()
    {
        if (StorageManager.Instance == null)
        {
            Debug.LogError("❌ StorageManager is missing in the scene!");
            return;
        }
        StorageManager.Instance.RegisterStorage(this);
    }

    void Start()
    {
        Vector2Int storagePos = GridManager.Instance.GetNearestPathTile(transform.position);
        GridManager.Instance.UpdateWalkability(storagePos, false);

        List<Vector2Int> neighbors = GridManager.Instance.GetNeighbors(storagePos);
        bool hasWalkableTile = false;

        foreach (var tile in neighbors)
        {
            if (!GridManager.Instance.IsOccupied(tile))
            {
                GridManager.Instance.UpdateWalkability(tile, true);
                hasWalkableTile = true;
                Debugger.Instance.Log($"✅ Marked adjacent tile {tile} as walkable for storage.");
            }
        }

        if (!hasWalkableTile)
        {
            Debug.LogError($"❌ No walkable tiles near storage at {storagePos}! Gobbos may get stuck.");
        }
    }

    public void DepositItems(Gobblyns gobbo)
    {
        gobbo.inventory.Clear();
        Debugger.Instance.Log($"[Storage] {gobbo.name} deposited items at {transform.position}");
    }
}

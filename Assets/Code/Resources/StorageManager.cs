using System.Collections.Generic;
using UnityEngine;

public class StorageManager : MonoBehaviour
{
    public static StorageManager Instance;

    public Dictionary<GobboResources, float> ResourceStorage = new();
    public List<GobboResources> allResources;
    public List<StorageBuilding> storageBuildings = new();

    void Start()
    {
        GobboResources.RegisterAllResources(); // ✅ Ensures all resources are loaded
    }

    void Awake()
    {
        Instance = this;

        InitalizeStorage();
    }

    private void InitalizeStorage()
    {
        foreach (var resource in allResources)
        {
            ResourceStorage[resource] = resource.defaultStorageAmount;
        }
    }

    public void AddResource(GobboResources resource, float amount)
    {
        if (!ResourceStorage.ContainsKey(resource))
            ResourceStorage[resource] = 0; 

        ResourceStorage[resource] += amount;
        Debugger.Instance.Log($"✅ Added {amount} {resource.resourceName}. Total: {ResourceStorage[resource]}");

        if (StorageUI.Instance != null) // ✅ Prevents calling UI update when StorageUI is missing
        {
            StorageUI.Instance.UpdateResourceUI();
        }
        else
        {
            Debug.LogWarning("⚠ `StorageUI.Instance` is null. UI will not be updated.");
        }
    }

    public bool RemoveResource(GobboResources resource, float amount)
    {
        if (ResourceStorage.ContainsKey(resource) && ResourceStorage[resource] >= amount)
        {
            ResourceStorage[resource] -= amount;

            Debugger.Instance.Log($"Removed {amount} {resource.resourceName}. Remaining: {ResourceStorage[resource]}");
            StorageUI.Instance.UpdateResourceUI();
            return true;
        }

        Debugger.Instance.Log($"Not enough {resource.resourceName} in storage!");
        StorageUI.Instance.UpdateResourceUI();        
        return false;
    }

    public float GetResourceAmount(string resourceName)
    {
        GobboResources resource = GobboResources.GetResource(resourceName);
        return ResourceStorage.ContainsKey(resource) ? ResourceStorage[resource] : 0;
    }

    public void RegisterStorage(StorageBuilding storage)
    {
        if (!storageBuildings.Contains(storage))
        {
            storageBuildings.Add(storage);
        }
    }

    public StorageBuilding GetClosestStorage(Vector3 position)
    {
        Debugger.Instance.Log($"🔍 Finding closest storage. Total storages: {storageBuildings.Count}");

        StorageBuilding closestStorage = null;
        float closestDistance = float.MaxValue;
        Vector3 closestStoragePos = Vector3.zero;

        foreach (var storage in storageBuildings)
        {
            float distance = Vector3.Distance(position, storage.transform.position);
            if (distance < closestDistance)
            {
                Vector2Int storageTile = GridManager.Instance.GetNearestPathTile(storage.transform.position);
                Vector2Int walkableTile = FindClosestWalkableTile(storageTile);

                if (GridManager.Instance.IsWalkable(walkableTile))
                {
                    closestDistance = distance;
                    closestStorage = storage;
                    closestStoragePos = GridManager.Instance.placementTilemap.CellToWorld(new Vector3Int(walkableTile.x, walkableTile.y, 0));
                }
            }
        }

        return closestStorage;
    }

    public Vector2Int FindClosestWalkableTile(Vector2Int storageTile)
    {
        List<Vector2Int> neighbors = GridManager.Instance.GetNeighbors(storageTile);
        foreach (var tile in neighbors)
        {
            if (GridManager.Instance.IsWalkable(tile))
            {
                return tile;
            }
        }
        return storageTile;
    }
}

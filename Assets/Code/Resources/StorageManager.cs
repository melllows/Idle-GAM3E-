using System.Collections.Generic;
using UnityEngine;

public class StorageManager : MonoBehaviour
{
    public static StorageManager Instance;
    public List<StorageBuilding> storageBuildings = new();

    void Awake()
    {
        Instance = this;
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
        Debugger.Instance.Log("Getting closest storage");
        StorageBuilding closestStorage = null;
        float closestDistance = float.MaxValue;

        foreach (var storage in storageBuildings)
        {
            float distance = Vector3.Distance(position, storage.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestStorage = storage;
            }
        }

        return closestStorage;
    }
}

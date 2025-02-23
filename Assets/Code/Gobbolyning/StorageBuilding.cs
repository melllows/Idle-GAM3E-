using UnityEngine;

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


    public void DepositItems(Gobblyns gobbo)
    {
        gobbo.inventory.Clear();
        Debugger.Instance.Log($"[Storage] {gobbo.name} deposited items at {transform.position}");
    }
}

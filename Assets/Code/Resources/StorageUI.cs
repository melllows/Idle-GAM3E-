using UnityEngine;
using TMPro;

public class StorageUI : MonoBehaviour
{
    public static StorageUI Instance;

    public TextMeshProUGUI resourceText;
    

    void Awake()
    {
        Instance = this;
        Invoke("UpdateResourceUI", 0.1f);
    }

    public void UpdateResourceUI()
    {
        if (resourceText == null)
        {
            Debug.LogError("`resourceText` is missing! Assign it in the Unity Inspector.");
            return;
        }

        Debugger.Instance.Log("Updating resource UI...");
        resourceText.text = "";

        foreach (var resource in StorageManager.Instance.ResourceStorage)
        {
            resourceText.text += $"{resource.Key.resourceName}: {resource.Value}\n";
        }
    }
}

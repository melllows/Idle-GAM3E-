using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;
    private Dictionary<string, GobboResources> resourceDictionary = new Dictionary<string, GobboResources>();

    void Awake() {
        if (Instance == null) {
            Instance = this;
            Debug.Log("[ResourceManager] Instance created.");
        } else {
            Debug.LogError("[ResourceManager] Multiple instances detected!");
            Destroy(gameObject);
            return;
        }
        
        GobboResources[] allResources = Resources.LoadAll<GobboResources>("");
        Debug.Log("[ResourceManager] Loading " + allResources.Length + " resources.");
        foreach (GobboResources res in allResources) {
            RegisterResource(res);
        }
        //Debugger.Instance.Log("[ResourceManager] Total resources registered: " + resourceDictionary.Count);
    }

    public void RegisterResource(GobboResources resource)
    {
        if (!resourceDictionary.ContainsKey(resource.resourceName))
        {
            resourceDictionary.Add(resource.resourceName, resource);
            //Debugger.Instance.Log($"Registered resource: {resource.resourceName}");
        }
    }

    public GobboResources GetResource(string resourceName)
    {
        if (resourceDictionary.TryGetValue(resourceName, out var resource))
        {
            return resource;
        }
        Debug.LogError($"Resource {resourceName} not found.");
        return null;
    }
}

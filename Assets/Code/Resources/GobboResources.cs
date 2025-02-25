using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "Resources", menuName = "Scriptable Objects/Resources")]
public class GobboResources : ScriptableObject
{
    public string resourceName;
    public Sprite resourceSprite;
    public List<Sprite> growthStages;
    public float defaultStorageAmount;
    public int resourceGrowthTime;
    public int resourceHarvestTime;
    public int resourceHarvestAmount;

    private static Dictionary<string, GobboResources> resourceDictionary = new();

    void Awake()
    {
        RegisterAllResources();
    }

    public static void RegisterAllResources()
    {
        if (resourceDictionary.Count == 0)
        {
            GobboResources[] resources = Resources.LoadAll<GobboResources>("");

            foreach (GobboResources resource in resources)
            {
                RegisterResource(resource);
            }

            Debug.Log($"Registered {resourceDictionary.Count} resources.");
        }
    }

    public static void RegisterResource(GobboResources resource)
    {
        if (!resourceDictionary.ContainsKey(resource.resourceName))
        {
            resourceDictionary[resource.resourceName] = resource;
            Debug.Log($"Registered resource: {resource.resourceName}");
        }
    }

    public static GobboResources GetResource(string resourceName)
    {
        if (resourceDictionary.TryGetValue(resourceName, out var resource))
        {
            return resource;
        }

        Debug.LogError($"Resource {resourceName} not found in GobboResources!");
        return null;
    }
}

using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "Resources", menuName = "Scriptable Objects/Resources")]
public class GobboResources : ScriptableObject
{
    public string resourceName;
    public Sprite resourceSprite;
    public List<Sprite> growthStages;

    public int resourceGrowthTime;
    public int resourceHarvestTime;
    public int resourceHarvestAmount;

    private static Dictionary<string, GobboResources> resourceDictionary = new Dictionary<string, GobboResources>();

    void Awake()
    {
        if (resourceDictionary.Count == 0)
        {
            foreach (GobboResources resource in Resources.LoadAll<GobboResources>(""))
            {
                RegisterResource(resource);
            }
        }
    }
    
    public static void RegisterResource(GobboResources resource)
    {
        if (!resourceDictionary.ContainsKey(resource.resourceName))
        {
            resourceDictionary.Add(resource.resourceName, resource);
        }
    }
    
    public static GobboResources GetResource(string resourceName)
    {
        if (resourceDictionary.TryGetValue(resourceName, out var resource))
        {
            return resource;
        }
        Debug.LogError($"Resource {resourceName} not found.");
        return null;
    }

    internal static T[] LoadAll<T>(string v)
    {
        throw new NotImplementedException();
    }
}

using UnityEngine;
using System.Collections;

public class Crops : MonoBehaviour
{
    public GobboResources Resources;
    public bool IsReadyToHarvest { get; private set; }

    private int growthIndex = 0;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("[Crops] SpriteRenderer missing on " + name);
        }
        StartCoroutine(Grow());
    }

    public void InitializeCrop()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(Grow());
    }

    private IEnumerator Grow()
    {
        while (growthIndex < Resources.growthStages.Count - 1)
        {
            Debug.Log("[Crops] " + name + " at growth index " + growthIndex);
            yield return new WaitForSeconds(1 /*Resources.resourceGrowthTime*/);
            growthIndex++;

            if (growthIndex < Resources.growthStages.Count)
            {
                spriteRenderer.sprite = Resources.growthStages[growthIndex];
            }
            else
            {
                Debug.LogError("[Crops] Growth index " + growthIndex + " is out of range for " + name + ". Max index: " + (Resources.growthStages.Count - 1));
                yield break;
            }
            yield return null;
        }

        Debugger.Instance.Log("[Crops] Crop fully grown at: " + transform.position);
        IsReadyToHarvest = true;

        RegisterHarvestTask();
    }

    private void RegisterHarvestTask()
    {
        Debugger.Instance.Log("[Crops] Registering a new harvest task at " + transform.position);

        Task harvestTask = new Task(
            transform.position,
            5,
            (gobbo) =>
            {
                Debugger.Instance.Log("[Crops] " + gobbo.name + " harvested crop at " + transform.position);
                gobbo.AddToInventory("Wheat");
                }
        );
        
        harvestTask.targetCrop = this;

        if (TaskManager.Instance == null)
        {
            Debug.LogError("[Crops] TaskManager.Instance is null!");
        }
        else
        {
            TaskManager.Instance.RegisterTask(harvestTask);
        }
    }

    public void Harvest(Gobblyns gobbo)
    {
        if (!IsReadyToHarvest)
            return;

        Debugger.Instance.Log("[Crops] " + gobbo.name + " is harvesting crop at " + transform.position);
        gobbo.AddToInventory(Resources.resourceName);
        
        growthIndex = 0;
        spriteRenderer.sprite = Resources.growthStages[growthIndex];
        IsReadyToHarvest = false;

        Debugger.Instance.Log("[Crops] Crop at " + transform.position + " has been harvested. Restarting growth cycle.");
        StartCoroutine(Grow());
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class PlacementUI : MonoBehaviour
{
    public BlueprintSystem blueprintSystem;
    public Transform buttonParent;
    public GameObject buttonPrefab;
    private List<BlueprintData> blueprints = new();

    private void Start()
    {
        blueprintSystem = FindObjectOfType<BlueprintSystem>();
        if (blueprintSystem == null)
        {
            Debug.LogError("[PlacementUI] BlueprintSystem not found in the scene!");
        }
        else
        {
            Debug.Log("[PlacementUI] BlueprintSystem found.");
        }

        BlueprintData[] loadedBlueprints = Resources.LoadAll<BlueprintData>("Blueprints");
        if (loadedBlueprints == null || loadedBlueprints.Length == 0)
        {
            Debug.LogError("[PlacementUI] No BlueprintData found in Resources/Blueprints!");
        }
        else
        {
            Debug.Log("[PlacementUI] Loaded " + loadedBlueprints.Length + " blueprints.");
        }

        foreach (BlueprintData blueprint in loadedBlueprints)
        {
            CreateBlueprintButton(blueprint);
        }
    }

    private void CreateBlueprintButton(BlueprintData blueprint)
    {
        GameObject newButton = Instantiate(buttonPrefab, buttonParent);
        if (newButton == null)
        {
            Debug.LogError("[PlacementUI] Failed to instantiate buttonPrefab!");
            return;
        }
        
        TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText == null)
        {
            Debug.LogError("[PlacementUI] Button text component not found in prefab!");
        }
        
        Image buttonImage = newButton.transform.GetChild(0).GetComponent<Image>();
        if (buttonImage == null)
        {
            Debug.LogError("[PlacementUI] Button image component not found in prefab!");
        }
        
        if (blueprint.blueprintSprite != null)
        {
            buttonImage.sprite = blueprint.blueprintSprite;
        }
        buttonText.text = blueprint.blueprintName;

        Button btn = newButton.GetComponent<Button>();
        if (btn == null)
        {
            Debug.LogError("[PlacementUI] Button component missing in prefab!");
        }
        else
        {
            btn.onClick.AddListener(() =>
            {
                if (blueprintSystem == null)
                {
                    Debug.LogError("[PlacementUI] BlueprintSystem reference is null!");
                    return;
                }
                Debug.Log("[PlacementUI] Button clicked for blueprint: " + blueprint.blueprintName);
                blueprintSystem.SetBlueprint(blueprint);
            });
        }
    }
}

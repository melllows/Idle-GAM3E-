using UnityEngine;

public class BlueprintSystem : MonoBehaviour
{
    public BlueprintData currentBlueprintData;
    private GameObject currentBlueprint;
    public GameObject blueprintPrefab;
    private bool isPlacing = false;

    void Update()
    {
        if (!isPlacing || currentBlueprint == null)
            return;
        
        if (Camera.main == null)
        {
            Debug.LogError("[BlueprintSystem] Camera.main is null!");
            return;
        }
        
        if (GridManager.Instance == null)
        {
            Debug.LogError("[BlueprintSystem] GridManager.Instance is null!");
            return;
        }
        
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int tilePosition = GridManager.Instance.GetNearestPlacementTile(mousePosition);
        Vector3 worldPos = GridManager.Instance.placementTilemap.CellToWorld(new Vector3Int(tilePosition.x, tilePosition.y, 0));
        currentBlueprint.transform.position = worldPos;

        bool canPlace = GridManager.Instance.CanPlaceHere(tilePosition);
        SpriteRenderer sr = currentBlueprint.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("[BlueprintSystem] SpriteRenderer missing on currentBlueprint!");
        }
        else
        {
            sr.color = canPlace ? new Color(1, 1, 1, 0.5f) : new Color(1, 0, 0, 0.5f);
        }

        if (Input.GetMouseButtonDown(0) && canPlace)
        {
            PlaceObject(tilePosition);
        }

        if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
        }
    }

    public void SetBlueprint(BlueprintData blueprint)
    {
        if (blueprint == null)
        {
            Debug.LogError("[BlueprintSystem] SetBlueprint called with null blueprint!");
            return;
        }
        StartPlacement(blueprint);
    }

    public void StartPlacement(BlueprintData blueprint)
    {
        if (isPlacing)
        {
            Debug.Log("[BlueprintSystem] Already placing blueprint, canceling previous placement.");
            CancelPlacement();
        }

        isPlacing = true;
        currentBlueprintData = blueprint;

        currentBlueprint = new GameObject("BlueprintPreview");
        SpriteRenderer renderer = currentBlueprint.AddComponent<SpriteRenderer>();
        renderer.sprite = blueprint.blueprintSprite;
        renderer.color = new Color(1, 1, 1, 0.5f);
        Debug.Log("[BlueprintSystem] Started placement for blueprint: " + blueprint.blueprintName);
    }

    private void PlaceObject(Vector2Int tilePosition)
    {
        Vector3Int cellPosition = new Vector3Int(tilePosition.x, tilePosition.y, 0);
        Vector3 finalPosition = GridManager.Instance.placementTilemap.GetCellCenterWorld(cellPosition);

        GameObject placedObject = Instantiate(currentBlueprintData.finalPrefab, finalPosition, Quaternion.identity, GridManager.Instance.placementTilemap.transform);

        GridManager.Instance.MarkTileOccupied(tilePosition, placedObject);

        if (!currentBlueprintData.isWalkable)
        {
            GridManager.Instance.UpdateWalkability(tilePosition, false);
            Debugger.Instance.Log($"🚧 Placed {currentBlueprintData.blueprintName} at {tilePosition}. Tile is now unwalkable.");
        }
        else
        {
            GridManager.Instance.UpdateWalkability(tilePosition, true);
            Debugger.Instance.Log($"✅ Placed {currentBlueprintData.blueprintName} at {tilePosition}. Tile remains walkable.");
        }

        CancelPlacement();
    }

    private void CancelPlacement()
    {
        isPlacing = false;
        if (currentBlueprint != null)
        {
            Destroy(currentBlueprint);
            currentBlueprint = null;
        }
        Debug.Log("[BlueprintSystem] Cancelled placement.");
    }
}

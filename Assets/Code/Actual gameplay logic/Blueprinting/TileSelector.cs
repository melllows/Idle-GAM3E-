using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TileSelector : MonoBehaviour
{
    public static TileSelector Instance;
    public RectTransform selectionBox;
    
    private Vector2 startMousePosition;
    private bool isDragging = false;
    private float dragThreshold = 5f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        HandleSelectionInputs(); 
        HandleGobboMovement();
    }

    void HandleSelectionInputs()
    {
        if (selectionBox == null)
        {
            Debug.LogError("❌ selectionBox is missing in TileSelector!");
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            startMousePosition = Input.mousePosition;
            isDragging = false;
        }
        else if (Input.GetMouseButton(0))
        {
            if (Vector2.Distance(startMousePosition, Input.mousePosition) > dragThreshold)
            {
                if (!isDragging)
                {
                    isDragging = true;
                    selectionBox.gameObject.SetActive(true);
                    selectionBox.sizeDelta = Vector2.zero;
                    selectionBox.anchoredPosition = startMousePosition;
                }
                ResizeSelectionBox();
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (!isDragging)
            {
                SelectSingleGobbo();
            }
            else
            {
                SelectGobbosInSelectionBox();
            }

            selectionBox.gameObject.SetActive(false);
        }
    }

    void ResizeSelectionBox()
    {
        Vector2 currentMousePosition = Input.mousePosition;
        float width = Mathf.Abs(currentMousePosition.x - startMousePosition.x);
        float height = Mathf.Abs(currentMousePosition.y - startMousePosition.y);

        selectionBox.anchoredPosition = startMousePosition;
        selectionBox.sizeDelta = new Vector2(width, height);

        Vector2 pivot = new Vector2(
            (currentMousePosition.x < startMousePosition.x) ? 1 : 0,
            (currentMousePosition.y < startMousePosition.y) ? 1 : 0
        );
        selectionBox.pivot = pivot;
    }

    void SelectSingleGobbo()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPosition.z = 0;

        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero, Mathf.Infinity, LayerMask.GetMask("Gobbo"));

        if (hit.collider != null)
        {
            SelectableGobbos gobbo = hit.collider.GetComponent<SelectableGobbos>();
            if (gobbo != null)
            {
                SelectionManager.instance.DeselectAll();
                SelectionManager.instance.Select(gobbo);
                Debugger.Instance.Log($"Single Gobbo Selected: {gobbo.name}");
                return;
            }
        }

        Debugger.Instance.Log("No Gobbo found, selecting tile instead.");
        SelectSingleTile(worldPosition);
    }



    void SelectSingleTile(Vector3 worldPosition)
    {
        Debugger.Instance.Log($"Single Tile Clicked at: {worldPosition}");

        // ✅ Deselect all Gobbos when clicking on an empty tile
        SelectionManager.instance.DeselectAll();
    }


  void SelectGobbosInSelectionBox()
    {
        Rect selectionRect = GetScreenSpaceSelectionBounds();
        bool selectedAtLeastOne = false;

        foreach (SelectableGobbos gobbo in SelectionManager.instance.AvailableGobbos)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(gobbo.transform.position);

            if (selectionRect.Contains(screenPos))
            {
                SelectionManager.instance.Select(gobbo);
                selectedAtLeastOne = true;
            }
        }

        if (!selectedAtLeastOne)
        {
            Debugger.Instance.Log("No Gobbos found in selection box, deselecting all.");
            SelectionManager.instance.DeselectAll();
        }
    }

    Rect GetScreenSpaceSelectionBounds()
    {
        Vector2 min = new Vector2(
            Mathf.Min(startMousePosition.x, Input.mousePosition.x),
            Mathf.Min(startMousePosition.y, Input.mousePosition.y)
        );
        Vector2 max = new Vector2(
            Mathf.Max(startMousePosition.x, Input.mousePosition.x),
            Mathf.Max(startMousePosition.y, Input.mousePosition.y)
        );

        return new Rect(min, max - min);
    }

    void HandleGobboMovement()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Debugger.Instance.Log("Right-click detected!");

            if (SelectionManager.instance.selectedGobbos.Count == 0)
            {
                Debug.LogWarning("No Gobbos selected. Right-click ignored.");
                return;
            }

            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPoint.z = 0; // Keep Gobbos in 2D space

            Debugger.Instance.Log($"Right-clicked at: {worldPoint} | Selected Gobbos: {SelectionManager.instance.selectedGobbos.Count}");

            foreach (SelectableGobbos gobbo in SelectionManager.instance.selectedGobbos)
            {
                GobboMovement movement = gobbo.GetComponent<GobboMovement>();

                if (movement != null)
                {
                    Debugger.Instance.Log($"Sending move command to {gobbo.name}");
                    Vector2 targetGridPosition = new Vector2(Mathf.Round(worldPoint.x), Mathf.Round(worldPoint.y));
                    movement.MoveTo(targetGridPosition);

                }
                else
                {
                    Debug.LogError($"Gobbo {gobbo.name} is missing GobboMovement component!");
                }
            }
        }
    }



    Bounds GetSelectionBounds()
    {
        Vector2 min = new Vector2(
            Mathf.Min(startMousePosition.x, Input.mousePosition.x),
            Mathf.Min(startMousePosition.y, Input.mousePosition.y)
        );
        Vector2 max = new Vector2(
            Mathf.Max(startMousePosition.x, Input.mousePosition.x),
            Mathf.Max(startMousePosition.y, Input.mousePosition.y)
        );

        return new Bounds((min + max) / 2, max - min);
    }
}

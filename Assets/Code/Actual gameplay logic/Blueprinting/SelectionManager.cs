using UnityEngine;
using System.Collections.Generic;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager instance;
    public List<SelectableGobbos> AvailableGobbos = new();
    public HashSet<SelectableGobbos> selectedGobbos = new();
    public List<SelectableGobbos> DebugSelectedGobbos = new();

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("❌ Multiple SelectionManager instances detected!");
            Destroy(gameObject);
            return;
        }
        instance = this;
        AutoAssignGobbos();
    }


    private void AutoAssignGobbos()
    {
        AvailableGobbos.AddRange(FindObjectsByType<SelectableGobbos>(FindObjectsSortMode.None));

        //Debugger.Instance.Log($"Assigned {AvailableGobbos.Count} Gobbos to SelectionManager.");
    }

    public void Select(SelectableGobbos gobbo)
    {
        if (!selectedGobbos.Contains(gobbo))
        {
            selectedGobbos.Add(gobbo);
            DebugSelectedGobbos.Add(gobbo);
            gobbo.OnSelected();
            //Debugger.Instance.Log($"Selected Gobbo: {gobbo.name} | Total Selected: {selectedGobbos.Count}");
        }
    }

    public void Deselect(SelectableGobbos gobbo)
    {
        if (selectedGobbos.Remove(gobbo))
        {
            DebugSelectedGobbos.Remove(gobbo);
            gobbo.OnDeselected();
            Debugger.Instance.Log($"Deselected Gobbo: {gobbo.name} | Remaining Selected: {selectedGobbos.Count}");
        }
    }

    public void DeselectAll()
    {
        foreach (SelectableGobbos gobbo in selectedGobbos)
        {
            gobbo.OnDeselected();
        }

        selectedGobbos.Clear();
        DebugSelectedGobbos.Clear();

        Debugger.Instance.Log("All Gobbos Deselected.");
    }
}

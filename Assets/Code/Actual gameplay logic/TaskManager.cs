using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance;
    public List<Task> tasks = new();
    private Dictionary<Task, Gobblyns> assignedTasks = new();
    private Queue<Task> unclaimedTasks = new();

    void Awake() {
        if (Instance != null && Instance != this) {
            Debug.LogError("[TaskManager] Multiple instances detected!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log("[TaskManager] Instance created successfully.");
    }


    public void RegisterTask(Task task) {
        Debugger.Instance.Log("[TaskManager] Registering task at " + task.Position);
        tasks.Add(task);
        unclaimedTasks.Enqueue(task);
        AssignUnclaimedTasks();
    }

    private void AssignTask(Task task)
    {
        Debugger.Instance.Log($"[TaskManager] Looking for an idle Gobbo for task at {task.Position}...");

        Gobblyns closestGobbo = GobboManager.Instance.GetClosestIdleGobbo(task.Position);

        if (closestGobbo != null)
        {
            assignedTasks[task] = closestGobbo;
            Debugger.Instance.Log($"[TaskManager] Assigned task at {task.Position} to {closestGobbo.name}");
            closestGobbo.AssignTask(task);
        }
        else
        {
            Debugger.Instance.Log($"[TaskManager] No idle Gobbos found for task at {task.Position}. Adding to unclaimed tasks.");
            unclaimedTasks.Enqueue(task);
        }
    }

    public void MarkTaskAsCompleted(Task task)
    {
        if (assignedTasks.ContainsKey(task))
        {
            assignedTasks.Remove(task);
        }
        tasks.Remove(task);
        Debugger.Instance.Log($"[TaskManager] Task at {task.Position} completed. Reassigning tasks...");

        AssignUnclaimedTasks();
    }

    public void AssignUnclaimedTasks() {
        Debugger.Instance.Log("[TaskManager] Unclaimed tasks count: " + unclaimedTasks.Count);
        while (unclaimedTasks.Count > 0) {
            Task task = unclaimedTasks.Dequeue();
            Debugger.Instance.Log("[TaskManager] Attempting to assign task at " + task.Position);
            AssignTask(task);
        }
    }
}

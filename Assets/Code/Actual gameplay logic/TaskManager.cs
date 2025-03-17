using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance;
    public List<GameTask> tasks = new();
    private Dictionary<GameTask, Gobblyns> assignedTasks = new();
    private Queue<GameTask> unclaimedTasks = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("[TaskManager] Multiple instances detected!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debugger.Instance.Log("[TaskManager] Instance created successfully.");
    }

    public void RegisterTask(GameTask task)
    {
        Debugger.Instance.Log($"[TaskManager] Registering task at {task.Position}");
        tasks.Add(task);
        unclaimedTasks.Enqueue(task);
        AssignUnclaimedTasks();
    }

    public void MarkTaskAsCompleted(GameTask task)
    {
        if (assignedTasks.ContainsKey(task))
        {
            assignedTasks.Remove(task);
        }
        tasks.Remove(task);
        Debugger.Instance.Log($"[TaskManager] Task at {task.Position} completed. Reassigning tasks...");
        AssignUnclaimedTasks();
    }

    // ✅ Missing method added!
    public void AssignUnclaimedTasks()
    {
        Debugger.Instance.Log($"[TaskManager] Checking for unclaimed tasks...");

        int tasksToProcess = unclaimedTasks.Count;
        List<GameTask> failedToAssign = new();

        for (int i = 0; i < tasksToProcess; i++)
        {
            GameTask task = unclaimedTasks.Dequeue();
            Debugger.Instance.Log($"[TaskManager] Attempting to assign task at {task.Position}");

            if (!TryAssignTask(task))
            {
                failedToAssign.Add(task);
            }
        }

        // Re-enqueue unassigned tasks
        foreach (GameTask task in failedToAssign)
        {
            unclaimedTasks.Enqueue(task);
        }

        Debugger.Instance.Log($"[TaskManager] End of AssignUnclaimedTasks. {failedToAssign.Count} tasks unassigned, will retry later.");
    }

    private bool TryAssignTask(GameTask task)
    {
        Debugger.Instance.Log($"[TaskManager] Looking for an idle Gobbo for task at {task.Position}...");

        Gobblyns closestGobbo = GobboManager.Instance.GetClosestIdleGobbo(task.Position);

        if (closestGobbo != null)
        {
            assignedTasks[task] = closestGobbo;
            Debugger.Instance.Log($"[TaskManager] Assigned task at {task.Position} to {closestGobbo.name}");
            closestGobbo.AssignTask(task);
            return true;
        }
        else
        {
            Debugger.Instance.Log($"[TaskManager] No idle Gobbos found for task at {task.Position}. Task not assigned this round.");
            return false;
        }
    }
}

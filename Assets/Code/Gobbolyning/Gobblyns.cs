using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gobblyns : MonoBehaviour
{  
    public List<string> inventory = new();
    public bool isBusy { get; private set; }
    private GobboMovement movement;
    public Queue<Task> taskQueue = new Queue<Task>();

    void Start()
    {
        GobboManager.Instance.RegisterGobbo(this);
        movement = GetComponent<GobboMovement>();
    }

    public void CancelTask()
    {
        isBusy = false;
        TaskManager.Instance.AssignUnclaimedTasks();
    }

    public void AssignTask(Task task) {
        if (isBusy) {
            Debugger.Instance.Log("[Gobbo " + name + "] Already busy. Queueing task.");
            taskQueue.Enqueue(task);
        } else {
            Debugger.Instance.Log("[Gobbo " + name + "] Accepting task immediately.");
            isBusy = true;
            StartCoroutine(ExecuteTask(task));
        }
    }

    private IEnumerator ExecuteTask(Task task)
    {
        Debugger.Instance.Log($"[Gobbo {name}] Moving towards task at position {task.Position}...");
        movement.MoveTo(task.Position);

        yield return new WaitUntil(() =>
            movement.HasArrived() || 
            Vector2.Distance(transform.position, task.Position) < 0.5f);

        Debugger.Instance.Log($"[Gobbo {name}] Arrived at task location.");

        if(task.targetCrop != null)
        {
            Debugger.Instance.Log($"[Gobbo {name}] Task is a harvest task. Beginning harvest work...");
            yield return movement.WorkOnTask(task.TaskTime);
            task.targetCrop.Harvest(this);
        }
        else
        {
            yield return movement.WorkOnTask(task.TaskTime);
            task.Execute(this);
        }

        TaskManager.Instance.MarkTaskAsCompleted(task);
        isBusy = false;

        if (taskQueue.Count > 0)
        {
            Debugger.Instance.Log($"[Gobbo {name}] Picking up next task from queue.");
            AssignTask(taskQueue.Dequeue());
        }
        else
        {
            Debugger.Instance.Log($"[Gobbo {name}] Checking TaskManager for available tasks.");
            TaskManager.Instance.AssignUnclaimedTasks();
        }
    }


    public void AddToInventory(string item)
    {
        inventory.Add(item);
        Debugger.Instance.Log($"Goblyn collected {item}. Inventory: {string.Join(", ", inventory)}");

        if (inventory.Count >= 3)
        {
            QueueHaulingTask();
        }
    }

    private void QueueHaulingTask()
    {
        Debugger.Instance.Log($"[Gobbo {name}] Attempting to queue a hauling task...");

        // Check if StorageManager is available
        if (StorageManager.Instance == null)
        {
            Debug.LogError($"[Gobbo {name}] StorageManager.Instance is null! Cannot queue hauling task.");
            return;
        }

        StorageBuilding closestStorage = StorageManager.Instance.GetClosestStorage(transform.position);
        if (closestStorage == null)
        {
            Debug.LogWarning($"[Gobbo {name}] No storage found! Cannot queue hauling task.");
            return;
        }

        Vector3 storagePos = closestStorage.transform.position;
        Debugger.Instance.Log($"[Gobbo {name}] Found storage at {storagePos}. Queuing hauling task.");

        StorageBuilding storageRef = closestStorage;

        Task haulingTask = new Task(
            storagePos,
            5,
            (gobbo) =>
            {
                if (storageRef != null)
                {
                    Debugger.Instance.Log($"[Gobbo {name}] Arrived at storage. Depositing items...");
                    storageRef.DepositItems(gobbo);
                }
                else
                {
                    Debug.LogError($"[Gobbo {name}] Storage reference is null during hauling task!");
                }
                gobbo.inventory.Clear();
            }
        );

        AssignTask(haulingTask);
    }
}

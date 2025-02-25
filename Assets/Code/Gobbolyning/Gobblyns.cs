using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gobblyns : MonoBehaviour
{  
    public List<string> inventory = new();
    public bool isBusy = false;
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

        StorageBuilding closestStorage = StorageManager.Instance.GetClosestStorage(transform.position);

        Vector3 storagePos = closestStorage.transform.position;
        Vector2Int storageTile = GridManager.Instance.GetNearestPathTile(storagePos);
        Vector2Int walkableTile = StorageManager.Instance.FindClosestWalkableTile(storageTile);

        if (!GridManager.Instance.IsWalkable(walkableTile))
        {
            Debug.LogError($"❌ [Gobbo {name}] No walkable tile near storage! Task cancelled.");
            return;
        }

        Debugger.Instance.Log($"[Gobbo {name}] Found valid dropoff tile {walkableTile}. Queuing hauling task.");

        StorageBuilding storageRef = closestStorage;
        Task haulingTask = new Task(
            GridManager.Instance.placementTilemap.CellToWorld(new Vector3Int(walkableTile.x, walkableTile.y, 0)),
            5,
            (gobbo) =>
            {
                if (storageRef != null)
                {
                    Debugger.Instance.Log($"[Gobbo {name}] Arrived at storage. Depositing items...");

                    foreach (var item in gobbo.inventory)
                    {
                        GobboResources resource = GobboResources.GetResource(item);
                        if (resource != null)
                        {
                            StorageManager.Instance.AddResource(resource, 1);
                        }
                        else
                        {
                            Debug.LogError($"[Gobbo {name}] Resource '{item}' not found in GobboResources.");
                        }
                    }
                    
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

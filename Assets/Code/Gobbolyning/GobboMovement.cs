using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class GobboMovement : MonoBehaviour
{
    public float speed = 3f;
    private Queue<Vector2> path = new();
    private bool isMoving = false;
    public Task currentTask;

    [System.Serializable]
    public class Task
    {
        public Vector2 targetPosition;
    }

    private Animator animator;
    public ParticleSystem particleSystem;
    private LineRenderer lineRenderer;

    void Start()
    {
        animator = GetComponent<Animator>();
        particleSystem = GetComponentInChildren<ParticleSystem>();

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.cyan;
        lineRenderer.endColor = Color.cyan;
        lineRenderer.positionCount = 0;
    }

    public void MoveTo(Vector2 targetPosition)
    {
        if (isMoving) {
            Debug.Log("[GobboMovement] " + name + " is already moving. New target: " + targetPosition);
            return;
        }
        
        if (isMoving) return;

        //Debugger.Instance.Log($"[GobboMovement] {name} is moving to {targetPosition}");

        Vector2Int start = GridManager.Instance.GetNearestPathTile(transform.position);
        Vector2Int target = GridManager.Instance.GetNearestPathTile(targetPosition);

        //Debugger.Instance.Log($"[GobboMovement] Moving from {start} to {target}");

        List<Vector2Int> newPath = Pathfinding.FindPath(start, target);

        if (newPath.Count == 0)
        {
            //Debug.LogWarning($"[GobboMovement] No valid path found to {target}! Gobbo cannot move.");
            return;
        }

        //Debugger.Instance.Log($"[GobboMovement] Path found: {string.Join(" -> ", newPath)}");

        path.Clear();
        List<Vector3> worldPositions = new();

        foreach (Vector2Int gridPos in newPath)
        {
            Vector2 worldPos = GridManager.Instance.pathfindingTilemap.CellToWorld(new Vector3Int(gridPos.x, gridPos.y, 0));
            path.Enqueue(worldPos);
            worldPositions.Add(worldPos);
        }

        Debug.Log("[GobboMovement] " + name + " starting move to " + targetPosition);


        lineRenderer.positionCount = worldPositions.Count;
        lineRenderer.SetPositions(worldPositions.ToArray());

        animator.SetBool("isIntrested", true);
        StartCoroutine(FollowPath());
    }


    private IEnumerator FollowPath()
    {
        isMoving = true;
        animator.SetBool("isWalking", true);
        particleSystem.Play();

        while (path.Count > 0)
        {
            Vector2 nextPos = path.Dequeue();
            //Debugger.Instance.Log($"Moving to: {nextPos}");

            while (Vector2.Distance(transform.position, nextPos) > 0.05f)
            {
                transform.position = Vector2.MoveTowards(transform.position, nextPos, speed * Time.deltaTime);
                yield return null;
            }
        }

        lineRenderer.positionCount = 0;
        isMoving = false;
        animator.SetBool("isWalking", false);
        particleSystem.Stop();

        //Debugger.Instance.Log("Gobbo has reached destination.");

        yield return new WaitForSeconds(0.2f);

        if (currentTask != null)
        {
            StartCoroutine(WorkOnTask(3f));
        }
    }

    public IEnumerator WorkOnTask(float taskTime)
    {
        animator.SetBool("isWorking", true);
        yield return new WaitForSeconds(taskTime);
        animator.SetBool("isWorking", false);
    }

    public void AssignTask(Task task)
    {
        if (currentTask != null && isMoving)
        {
            StopCoroutine(FollowPath());
            isMoving = false;
        }

        currentTask = task;
        MoveTo(task.targetPosition);

        //Debugger.Instance.Log("Task assigned to Gobbo.");
    }

    public bool HasArrived()
    {
        if (currentTask == null) return false;
        return path.Count == 0 && Vector2.Distance(transform.position, currentTask.targetPosition) < 0.05f;
    }

    private void RecalculatePath()
    {
        if (currentTask != null)
        {
            MoveTo(currentTask.targetPosition);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GobboMovement : MonoBehaviour
{
    public float speed = 3f;
    private Queue<Vector2> path = new();
    public bool isMoving = false;
    public Task currentTask;

    private Animator animator;
    public ParticleSystem particleSystem;
    private LineRenderer lineRenderer;

    private Coroutine currentMovementCoroutine;

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

    void Update()
    {
        if (!isMoving && !animator.GetBool("isInterested") && !animator.GetBool("isWorking"))
        {
            animator.SetBool("beenIdle", true);
        }
        else
        {
            animator.SetBool("beenIdle", false);
        }
    }

    public void MoveTo(Vector2 targetPosition)
    {
        if (isMoving)
        {
            Debugger.Instance.Log($"[GobboMovement] {name} is already moving. New target: {targetPosition}");
            return;
        }

        Vector2Int start = GridManager.Instance.GetNearestPathTile(transform.position);
        Vector2Int target = GridManager.Instance.GetNearestPathTile(targetPosition);

        List<Vector2Int> newPath = Pathfinding.FindPath(start, target);

        if (newPath.Count == 0)
        {
            Debug.LogWarning($"[GobboMovement] No valid path found to {target}! Gobbo cannot move.");
            return;
        }

        path.Clear();
        List<Vector3> worldPositions = new();

        foreach (Vector2Int gridPos in newPath)
        {
            Vector2 worldPos = GridManager.Instance.pathfindingTilemap.CellToWorld(new Vector3Int(gridPos.x, gridPos.y, 0));
            path.Enqueue(worldPos);
            worldPositions.Add(worldPos);
        }

        Debugger.Instance.Log($"[GobboMovement] {name} starting move to {targetPosition}");

        lineRenderer.positionCount = worldPositions.Count;
        lineRenderer.SetPositions(worldPositions.ToArray());

        animator.SetBool("isInterested", true);

        if (animator.GetBool("beenIdle"))
        {
            StartCoroutine(WaitAndStartPath());
        }
        else
        {
            animator.SetBool("isInterested", false);
            StartFollowPath();
        }
    }

    private IEnumerator WaitAndStartPath()
    {
        yield return new WaitForSeconds(0.5f);
        animator.SetBool("isInterested", false);
        StartFollowPath();
    }

    private void StartFollowPath()
    {
        if (currentMovementCoroutine != null)
        {
            StopCoroutine(currentMovementCoroutine);
        }

        currentMovementCoroutine = StartCoroutine(FollowPath());
    }

    public void InterruptPath(Vector2 targetPosition)
    {
        if (isMoving && currentMovementCoroutine != null)
        {
            StopCoroutine(currentMovementCoroutine);
            currentMovementCoroutine = null;
            isMoving = false;
            path.Clear();
        }
        MoveTo(targetPosition); // ✅ Restart movement with new target
    }

    private IEnumerator FollowPath()
    {
        isMoving = true;
        animator.SetBool("isWalking", true);
        particleSystem.Play();

        Debugger.Instance.Log($"🚶 [GobboMovement] {name} following path...");

        int loopCounter = 0;

        while (path.Count > 0)
        {
            if (++loopCounter > 500)
            {
                Debug.LogError($"❌ [GobboMovement] {name} is stuck! Canceling movement.");
                isMoving = false;
                yield break;
            }

            Vector2 nextPos = path.Dequeue();
            Debugger.Instance.Log($"➡ [GobboMovement] {name} moving to {nextPos}");

            while (Vector2.Distance(transform.position, nextPos) > 0.05f)
            {
                transform.position = Vector2.MoveTowards(transform.position, nextPos, speed * Time.deltaTime);
                yield return null;
            }
        }

        Debugger.Instance.Log($"✅ [GobboMovement] {name} reached destination.");
        isMoving = false;
        animator.SetBool("isWalking", false);
        particleSystem.Stop();
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
            if (currentMovementCoroutine != null)
            {
                StopCoroutine(currentMovementCoroutine);
                currentMovementCoroutine = null;
                isMoving = false;
            }
        }
        currentTask = task;
        MoveTo(task.Position);
    }

    public bool HasArrived()
    {
        if (currentTask == null) return false;
        return path.Count == 0 || Vector2.Distance(transform.position, currentTask.Position) < 0.1f;
    }

    private void RecalculatePath()
    {
        if (currentTask != null)
        {
            MoveTo(currentTask.Position);
        }
    }
}

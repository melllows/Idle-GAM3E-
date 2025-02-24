using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.XR;

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

    public float idleDelay;
    public float idleTimer = 0f;


    private Animator animator;
    public ParticleSystem particleSystem;
    private LineRenderer lineRenderer;

    private Coroutine currentMovementCoroutine;

    void Start()
    {
        idleDelay = Random.Range(15f, 30f);
        animator = GetComponent<Animator>();
        particleSystem = GetComponentInChildren<ParticleSystem>();

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.cyan;
        lineRenderer.endColor = Color.cyan;
        lineRenderer.positionCount = 0;

        Gobblyns gobbo = GetComponent<Gobblyns>();
    }

    void Update()
    {
        Gobblyns gobbo = GetComponent<Gobblyns>();
        if (!isMoving && !animator.GetBool("isIntrested") && !animator.GetBool("isWorking"))
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleDelay && !animator.GetBool("beenIdle"))
            {
                animator.SetBool("beenIdle", true);
                Debugger.Instance.Log("[GobboMovement] " + name + " has been idle for " + idleDelay + " seconds.");
            }
        }
        else
        {
            idleTimer = 0f;
            if (animator.GetBool("beenIdle"))
            {
                animator.SetBool("beenIdle", false);
                Debugger.Instance.Log("[GobboMovement] " + name + " is no longer idle.");
            }
        }
    }


    public void Interruptpath(Vector2 targetPosition)
    {
        if(isMoving && currentMovementCoroutine != null)
        {
            StopCoroutine(currentMovementCoroutine);
            currentMovementCoroutine = null;
            isMoving = false;
            path.Clear();
        }
        MoveTo(targetPosition);
    }

    public void MoveTo(Vector2 targetPosition)
    {
        if (isMoving) {
            Debugger.Instance.Log("[GobboMovement] " + name + " is already moving. New target: " + targetPosition);
            return;
        }

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
        currentMovementCoroutine = StartCoroutine(FollowPath());
    }


    private IEnumerator FollowPath()
    {
        isMoving = true;
        animator.SetBool("isWalking", true);
        particleSystem.Play();

        float rotationSpeed = 1500f;

        while (path.Count > 0)
        {
            Vector2 nextPos = path.Dequeue();
            //Debugger.Instance.Log($"Moving to: {nextPos}");

            Quaternion targetRotation;

            if(nextPos.x - transform.position.x < 0)
            {
                targetRotation = Quaternion.Euler(0, 180, 0);
            }
            else if(nextPos.x - transform.position.x > 0)
            {
                targetRotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                targetRotation = transform.rotation;
            }

            while (Vector2.Distance(transform.position, nextPos) > 0.05f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                transform.position = Vector2.MoveTowards(transform.position, nextPos, speed * Time.deltaTime);
                yield return null;
            }
                //Vector2 direction = (nextPos - (Vector2)transform.position).normalized;
        }

        lineRenderer.positionCount = 0;
        isMoving = false;
        animator.SetBool("isWalking", false);
        particleSystem.Stop();

        //Debugger.Instance.Log("Gobbo has reached destination.");

        yield return new WaitForSeconds(0.2f);

        if (currentTask == null)
        {
            StartCoroutine(WorkOnTask(3f));
        }

        currentMovementCoroutine = null;
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
        currentTask = task;
        MoveTo(task.targetPosition);

        //Debugger.Instance.Log("Task assigned to Gobbo.");
        }
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

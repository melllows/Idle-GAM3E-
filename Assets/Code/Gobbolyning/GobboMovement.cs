using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using AStar;

public class GobboMovement : MonoBehaviour
{
    public float speed = 3f;
    private Queue<Vector2> path = new();
    private bool isMoving = false;

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

    public IEnumerator WorkOnTask(float taskTime)
    {
        Debugger.Instance.Log($"[GobboMovement] Performing task for {taskTime} seconds.");
        animator.SetBool("isWorking", true);
        yield return new WaitForSeconds(taskTime);
        animator.SetBool("isWorking", false);
        Debugger.Instance.Log("[GobboMovement] Task completed.");
    }

    public void MoveTo(Vector2 targetPosition)
    {
        Debugger.Instance.Log($"[GobboMovement] Calculating path to {targetPosition}");

        Vector2Int start = GridManager.Instance.GetNearestPathTile(transform.position);
        Vector2Int target = GridManager.Instance.GetNearestPathTile(targetPosition);

        // Get walkable map dimensions
        bool[,] walkableMap = GridManager.Instance.GetWalkableMap();
        int width = walkableMap.GetLength(1);
        int height = walkableMap.GetLength(0);

        // Prevent out-of-bounds errors
        if (start.x < 0 || start.y < 0 || target.x < 0 || target.y < 0 ||
            start.x >= width || start.y >= height || target.x >= width || target.y >= height)
        {
            Debugger.Instance.Log($"[GobboMovement] Invalid path request! Start: {start}, Target: {target}, Map Size: {width}x{height}");
            return;
        }

        // Generate the path using A* (Synchronous method)
        (int, int)[] newPath = AStarPathfinding.GeneratePathSync(start.x, start.y, target.x, target.y, walkableMap, true, false);

        if (newPath.Length == 0)
        {
            Debugger.Instance.Log($"[GobboMovement] No valid path found to {target}! Gobbo cannot move.");
            return;
        }

        Debugger.Instance.Log($"[GobboMovement] Path found: {string.Join(" -> ", newPath)}");

        path.Clear();
        List<Vector3> worldPositions = new();

        foreach ((int x, int y) in newPath)
        {
            Vector2 worldPos = GridManager.Instance.pathfindingTilemap.CellToWorld(new Vector3Int(x, y, 0));
            path.Enqueue(worldPos);
            worldPositions.Add(worldPos);
        }

        lineRenderer.positionCount = worldPositions.Count;
        lineRenderer.SetPositions(worldPositions.ToArray());

        animator.SetBool("isIntrested", true);

        if (!isMoving) StartCoroutine(FollowPath());
    }


    private IEnumerator FollowPath()
    {
        Debugger.Instance.Log("[GobboMovement] Following path.");
        isMoving = true;
        animator.SetBool("isWalking", true);
        particleSystem.Play();

        while (path.Count > 0)
        {
            Vector2 nextPos = path.Dequeue();
            Debugger.Instance.Log($"[GobboMovement] Moving to: {nextPos}");

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

        Debugger.Instance.Log("[GobboMovement] Gobbo has reached destination.");
    }

    public void InterruptPath(Vector2 targetPosition)
    {
        Debugger.Instance.Log($"[GobboMovement] Interrupting current path. New target: {targetPosition}");
        StopAllCoroutines();
        path.Clear();
        MoveTo(targetPosition);
    }

    public bool HasArrived()
    {
        return path.Count == 0;
    }
}

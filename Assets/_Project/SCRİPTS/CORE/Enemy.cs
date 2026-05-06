using UnityEngine;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Seeker))]
public class Enemy : MonoBehaviour
{
    private enum GuardState {Idle, CalculatingPath, Moving, Reverting};
    [SerializeField] private GuardState currentState = GuardState.Idle;

    [Header("Hareket Ayarlari")]
    public float moveSpeed = 3f;
    public float nextWaypointDistance = .1f;

    private TransformableBlock targetBlock;

    private Seeker seeker;
    private Rigidbody2D rb2d;
    private Path path;
    private int currentWaypoint = 0;

    void Start()
    {
        seeker = gameObject.GetComponent<Seeker>();
        rb2d = gameObject.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        switch (currentState)
        {
            case GuardState.Idle:
                FindTarget();
                break;
            case GuardState.CalculatingPath:
                break;
            case GuardState.Reverting:
                break;
        }
    }

    void FixedUpdate()
    {
        if(currentState == GuardState.Moving)
        {
            MoveAlongPath();
        }
    }

    private void FindTarget()
    {
        TransformableBlock[] allBlocks = FindObjectsByType<TransformableBlock>(FindObjectsSortMode.None);
        TransformableBlock closestGuard = null;
        TransformableBlock closestBlock = null;

        float minGuardDist = Mathf.Infinity;
        float minBlockDist = Mathf.Infinity;

        foreach(var block in allBlocks)
        {
            if (!block.isTransformed)
            {
                continue;
            }

            float dist = Vector3.Distance(transform.position, block.transform.position);

            if(block.isGuard && dist < minGuardDist)
            {
                closestGuard = block;
                minGuardDist = dist;
            }else if (!block.isGuard && dist < minBlockDist)
            {
                closestBlock = block;
                minBlockDist = dist;
            }
        }

        targetBlock = closestGuard != null ? closestGuard : closestBlock;

        if(targetBlock != null)
        {
            currentState = GuardState.CalculatingPath;
            seeker.StartPath(transform.position, targetBlock.transform.position, OnPathComplete);
        }
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
            currentState = GuardState.Moving;
        }
        else
        {
            Debug.LogWarning("Yol bulunamadi: " + p.errorLog);
            currentState = GuardState.Idle;
        }
    }

    private void MoveAlongPath()
    {
        if(path == null) return;

        if(currentWaypoint >= path.vectorPath.Count)
        {
            StartCoroutine(RevertAction());
            return;
        }

        Vector2 targetPosition = path.vectorPath[currentWaypoint];
        Vector2 newPosition = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        rb2d.MovePosition(newPosition);

        float distanceToWaypoint = Vector3.Distance(transform.position, targetPosition);

        if(distanceToWaypoint < nextWaypointDistance)
        {
            currentWaypoint++;
        }
    }

    private IEnumerator RevertAction()
    {
        currentState = GuardState.Reverting;

        yield return new WaitForSeconds(.5f);

        if(targetBlock != null && targetBlock.isTransformed)
        {
            targetBlock.RevertToOriginal();
        }

        path = null;
        targetBlock = null;
        currentState = GuardState.Idle;
    }
}
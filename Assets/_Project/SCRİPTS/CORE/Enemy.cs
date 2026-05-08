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

    private Animator animator;
    private string currentAnimState = "Idle";
    private Vector2 movementDir = Vector2.zero;

    void Start()
    {
        seeker = gameObject.GetComponent<Seeker>();
        rb2d = gameObject.GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        switch (currentState)
        {
            case GuardState.Idle:
                movementDir = Vector2.zero;
                FindTarget();
                break;
            case GuardState.CalculatingPath:
                movementDir = Vector2.zero;
                break;
            case GuardState.Reverting:
                movementDir = Vector2.zero;
                break;
        }
        
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if(currentState == GuardState.Moving)
        {
            MoveAlongPath();
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        string newState = "Idle";

        // Player'daki gibi Yön önceliğini belirliyoruz, Yukarı ve Aşağı animasyonları ters tetikleniyor
        if (movementDir.x > 0.01f) newState = "Right_Walk";
        else if (movementDir.x < -0.01f) newState = "Left_Walk";
        else if (movementDir.y > 0.01f) newState = "Down_Walk"; // Yukarıya (y>0) giderken Down_Walk tetikleniyor
        else if (movementDir.y < -0.01f) newState = "Up_Walk";  // Aşağıya (y<0) giderken Up_Walk tetikleniyor

        if (newState == "Idle")
        {
            // Hedefe ulaştığında veya durduğunda animasyonu son karesinde dondur
            animator.speed = 0f;
        }
        else
        {
            animator.speed = 1f; // Hareket varken normal hızında oynat

            // Sadece yön değiştiğinde yeni trigger'ı tetikle
            if (newState != currentAnimState)
            {
                animator.ResetTrigger("Right_Walk");
                animator.ResetTrigger("Left_Walk");
                animator.ResetTrigger("Up_Walk");
                animator.ResetTrigger("Down_Walk");

                animator.SetTrigger(newState);
                currentAnimState = newState;
            }
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
        movementDir = (targetPosition - (Vector2)transform.position).normalized;
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

        // Eğer hedefimiz hala geçerliyse ve hala dönüştürülmüş durumdaysa
        if(targetBlock != null && targetBlock.isTransformed)
        {
            // ESKİ KOD: Sadece dokunduğu bloğu düzeltiyordu
            // targetBlock.RevertToOriginal();

            // YENİ KOD: Dokunduğu bloğun "şu anki türüne" sahip TÜM dönüştürülmüş blokları düzeltir
            GlobalTransformationManager.RevertAllBlocksOfCurrentType(targetBlock.currentItemType);
        }

        path = null;
        targetBlock = null;
        currentState = GuardState.Idle;
    }
}
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    private Rigidbody2D rb;

    enum EnemyState : byte { Idle, Patrolling, Following, Chasing }
    [SerializeField] private EnemyState currentState = EnemyState.Idle;

    private Vector2 targetDestination = Vector2.zero;

    [SerializeField] private float maxSwimSpeed = 8f;
    [SerializeField] private float swimForce = 10f;
    [SerializeField] private float drag = 2f;

    [SerializeField] private float rotationSpeed = 240f;

    [SerializeField] private float sprintMultiplier = 1.8f;
    [SerializeField] private float sprintForceMultiplier = 1.5f;
    private bool sprinting = false;

    [SerializeField] private float detectionRange = 16f;
    [SerializeField] private float chaseDetectionRangeMultiplier = 1.5f;
    [SerializeField] private LayerMask detectionLayerMask;
    [SerializeField] private float eyeHeight = 0.2f;
    [SerializeField] private float eyeOffset = 0.6f;
    private Vector2 eyePosition = Vector2.zero;

    [SerializeField] private float patrolWidth = 4f;
    [SerializeField] private float patrolHeight = 4f;
    [SerializeField] private float patrolTimerMultiplier = 2f;
    private float patrolTimer = 0f;
    private Vector2 patrolPosition = Vector2.zero;
    private bool patrolStart = false;

    public GameObject player;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");
        eyePosition = Vector2.up * eyeHeight + Vector2.right * eyeOffset;
        StartPatrol();
    }

    void Update()
    {
        HandleEnemyState();
    }

    private void HandleEnemyState()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                if (CheckForPlayer()) currentState = EnemyState.Following;
                break;
            case EnemyState.Patrolling:
                Patrol();
                if (CheckForPlayer()) currentState = EnemyState.Following;
                break;
            case EnemyState.Following:
                if (CheckForPlayer()) FollowPlayer();
                else StartPatrol();
                break;
            case EnemyState.Chasing:
                if (CheckForPlayer()) FollowPlayer();
                else StartPatrol();
                break;
        }
        MoveToTarget(maxSwimSpeed);
    }

    private void StartPatrol()
    {
        currentState = EnemyState.Patrolling; 
        patrolStart = true;
    }
    private void Patrol()
    {
        if (patrolStart) { patrolPosition = transform.position; patrolStart = false; this.patrolTimer = 0f; };
        patrolTimer += Time.deltaTime * patrolTimerMultiplier;
        patrolTimer %= 360;
        float x = Mathf.Sin(patrolTimer) * patrolWidth;
        float y = Mathf.Sin(patrolTimer * 2) * patrolHeight;
        targetDestination = new Vector2(patrolPosition.x + x, patrolPosition.y + y);
    }

    private void FollowPlayer()
    {
        targetDestination = (Vector2)player.transform.position - ((Vector2)transform.position + eyePosition);
    }

    private bool CheckForPlayer()
    {
        Vector2 directionToPlayer = ((Vector2)player.transform.position - ((Vector2)transform.position + eyePosition)).normalized;
        if (Mathf.Abs(Vector2.SignedAngle(transform.right, directionToPlayer)) > 100) return false;
        
        float currentDetectionRange = currentState == EnemyState.Following || currentState == EnemyState.Chasing ? detectionRange * chaseDetectionRangeMultiplier : detectionRange;
        
        RaycastHit2D[] hit = Physics2D.RaycastAll((Vector2)transform.position + eyePosition, directionToPlayer, currentDetectionRange, detectionLayerMask);
        Debug.DrawRay((Vector2)transform.position + eyePosition, directionToPlayer * currentDetectionRange, Color.red, 0.01f);
        RaycastHit2D hitToCheck = new()
        {
            distance = 9999f,
        };
        for (int i = 0; i < hit.Length; i++)
        {
            if (!hit[i].collider.CompareTag("Enemy")) if (hit[i].distance < hitToCheck.distance) hitToCheck = hit[i];
        }
        if (hitToCheck == false) return false;
        if (hitToCheck.collider.CompareTag("Player"))
        {
            //Debug.Log("Player detected");
            return true;
        }
        return false;
    }

    private void MoveToTarget(float maxSpeed)
    {
        if (targetDestination.sqrMagnitude < 0.01f) return; 

        Debug.Log(targetDestination);

        targetDestination = targetDestination.normalized;
        float movementAngle = Mathf.Atan2(targetDestination.y, targetDestination.x) * Mathf.Rad2Deg;
        float newAngle = Mathf.MoveTowardsAngle(rb.rotation, movementAngle, rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newAngle);

        float currentMaxSpeed = currentState == EnemyState.Chasing ? maxSpeed * sprintMultiplier : maxSpeed;
        float currentForce = currentState == EnemyState.Chasing ? swimForce * sprintForceMultiplier : swimForce; 
        
        float distance = targetDestination.magnitude;
        float targetSpeed = Mathf.Min(currentMaxSpeed, distance * currentForce);
        Vector2 targetVelocity = targetDestination * targetSpeed;
        Vector2 velocityDifference = targetVelocity - rb.linearVelocity;
        rb.AddForce(velocityDifference * currentForce);
        rb.AddForce(-rb.linearVelocity * drag);
    }
}
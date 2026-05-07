using UnityEngine;
using UnityEngine.AI;

public class DragonBoss : Enemy
{
    [Header("Dragon Boss Move")]
    public float turnSpeed = 80f;
    public float turnDuration = 1.2f;

    [Header("Target")]
    public float playerDetectRange = 15f;
    public float targetLoseRange = 25f;
    public float faceFinishAngle = 5f;
    public float faceCooldown = 1.2f;

    [Header("Roar")]
    public float roarDuration = 2.5f;

    private int currentMoveType = -1;
    private Transform lockedTarget;
    private float faceCooldownTimer = 0f;
    private bool hasRoared = false;

    public bool HasRoared => hasRoared;

    protected override void Start()
    {
        base.Start();

        agent.updateRotation = false;
        agent.speed = PatrolSpeed;
    }

    protected override void EnemyAIUpdate()
    {
        if (_isDead) return;

        if (faceCooldownTimer > 0f)
            faceCooldownTimer -= Time.deltaTime;
    }

    public void SetMoveType(int type)
    {
        if (currentMoveType == type) return;

        currentMoveType = type;
        animator.SetInteger("MoveType", type);
    }

    public void StopMove()
    {
        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    public void Idle()
    {
        StopMove();
        SetMoveType(0);
    }

    public bool GetRandomPatrolPoint(out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * PatrolRadius;

            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = transform.position;
        return false;
    }

    public bool HasPlayerInRange()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) return false;

        float dist = Vector3.Distance(transform.position, player.position);

        if (lockedTarget != null)
        {
            if (dist <= targetLoseRange)
                return true;

            lockedTarget = null;
            return false;
        }

        if (dist <= playerDetectRange)
        {
            lockedTarget = player;
            return true;
        }

        return false;
    }

    public bool HasLockedTarget()
    {
        return lockedTarget != null;
    }

    public Transform GetLockedTarget()
    {
        return lockedTarget;
    }

    public bool CanFacePlayer()
    {
        return faceCooldownTimer <= 0f;
    }

    public void StartFaceCooldown()
    {
        faceCooldownTimer = faceCooldown;
    }

    public void SetRoared()
    {
        hasRoared = true;
    }
}
using UnityEngine;
using UnityEngine.AI;

public class DragonBoss : Enemy
{
    public DragonBossStateMachine StateMachine { get; private set; }

    public DragonBossIdleState IdleState { get; private set; }
    public DragonBossPatrolState PatrolState { get; private set; }
    public DragonBossTurnLeftState TurnLeftState { get; private set; }
    public DragonBossTurnRightState TurnRightState { get; private set; }

    public DragonBossFaceTargetState FaceTargetState { get; private set; }

    [Header("Dragon Boss Move")]
    public float idleTime = 2f;
    public float turnSpeed = 80f;
    public float turnDuration = 1.2f;

    [Header("Target")]
    public float playerDetectRange = 15f;
    private Transform playerTarget;
    [Header("Face Target")]
    public float faceCooldown = 1.2f;
    public float faceFinishAngle = 5f;
    [Header("Target Lock")]
    public float targetLoseRange = 25f; 
    //변수들
    private int currentMoveType = -1;
    private float faceCooldownTimer = 0f;
    private Transform lockedTarget;
    protected override void Awake()
    {
        base.Awake();

        StateMachine = new DragonBossStateMachine();

        IdleState = new DragonBossIdleState(this, StateMachine);
        PatrolState = new DragonBossPatrolState(this, StateMachine);
        TurnLeftState = new DragonBossTurnLeftState(this, StateMachine);
        TurnRightState = new DragonBossTurnRightState(this, StateMachine);

        StateMachine.Initialize(IdleState);

        FaceTargetState = new DragonBossFaceTargetState(this, StateMachine);
        playerTarget = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    protected override void Start()
    {
        base.Start();

        agent.updateRotation = false;
        agent.speed = PatrolSpeed;
    }

    protected override void EnemyAIUpdate()
    {
        if (_isDead) return;
        if (StateMachine.CurrentState == null) return;

        if (faceCooldownTimer > 0f)
            faceCooldownTimer -= Time.deltaTime;

        StateMachine.CurrentState.LogicUpdate();
    }

    public void SetMoveType(int type)
    {
        if (currentMoveType == type) return;

        currentMoveType = type;
        animator.SetInteger("MoveType", type);
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

    public Transform GetLockedTarget()
    {
        return lockedTarget;
    }
    //타겟 고정!!!
    public bool HasLockedTarget()
    {
        return lockedTarget != null;
    }

    public Transform GetPlayerTarget()
    {
        return playerTarget;
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

    public bool CanFacePlayer()
    {
        return faceCooldownTimer <= 0f;
    }

    public void StartFaceCooldown()
    {
        faceCooldownTimer = faceCooldown;
    }

    //BT GRAPH
    public void BT_Idle()
    {
        Debug.Log("BT_Idle 호출됨");
        StateMachine.ChangeState(IdleState);
    }

    public void BT_Patrol()
    {
        Debug.Log("BT_Patrol 호출됨");
        StateMachine.ChangeState(PatrolState);
    }

    public void BT_TurnLeft()
    {
        Debug.Log("BT_TurnLeft 호출됨");
        StateMachine.ChangeState(TurnLeftState);
    }

    public void BT_TurnRight()
    {
        Debug.Log("BT_TurnRight 호출됨");
        StateMachine.ChangeState(TurnRightState);
    }

    public void BT_FacePlayer()
    {
        Debug.Log("BT_FacePlayer 호출됨");

        if (!CanFacePlayer())
            return;

        if (!HasPlayerInRange())
            return;

        FaceTargetState.SetTarget(GetLockedTarget());
        StateMachine.ChangeState(FaceTargetState);
    }
}
using UnityEngine;
using UnityEngine.AI;

public class DragonBoss : Enemy
{
    public DragonBossStateMachine StateMachine { get; private set; }

    public DragonBossIdleState IdleState { get; private set; }
    public DragonBossPatrolState PatrolState { get; private set; }
    public DragonBossTurnLeftState TurnLeftState { get; private set; }
    public DragonBossTurnRightState TurnRightState { get; private set; }

    [Header("Dragon Boss Move")]
    public float idleTime = 2f;
    public float turnSpeed = 80f;
    public float turnDuration = 1.2f;

    protected override void Awake()
    {
        base.Awake();

        StateMachine = new DragonBossStateMachine();

        IdleState = new DragonBossIdleState(this, StateMachine);
        PatrolState = new DragonBossPatrolState(this, StateMachine);
        TurnLeftState = new DragonBossTurnLeftState(this, StateMachine);
        TurnRightState = new DragonBossTurnRightState(this, StateMachine);
    }

    protected override void Start()
    {
        base.Start();

        agent.updateRotation = false;
        agent.speed = patrolSpeed;

        StateMachine.Initialize(IdleState);
    }

    protected override void EnemyAIUpdate()
    {
        if (_isDead) return;

        StateMachine.CurrentState.LogicUpdate();
    }

    public void SetMoveType(int type)
    {
        animator.SetInteger("MoveType", type);
    }

    public bool GetRandomPatrolPoint(out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * patrolRadius;

            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = transform.position;
        return false;
    }
}
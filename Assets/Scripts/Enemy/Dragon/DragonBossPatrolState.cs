using UnityEngine;

public class DragonBossPatrolState : DragonBossState
{
    private Vector3 targetPos;

    public DragonBossPatrolState(DragonBoss boss, DragonBossStateMachine stateMachine)
        : base(boss, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        boss.agent.isStopped = false;
        boss.agent.speed = boss.PatrolSpeed;

        boss.SetMoveType(1);

        if (boss.GetRandomPatrolPoint(out targetPos))
        {
            boss.agent.SetDestination(targetPos);
        }
        else
        {
            stateMachine.ChangeState(boss.IdleState);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        RotateToDestination();

        if (!boss.agent.pathPending &&
            boss.agent.remainingDistance <= boss.agent.stoppingDistance + 0.3f)
        {
            stateMachine.ChangeState(boss.IdleState);
        }
    }

    private void RotateToDestination()
    {
        Vector3 dir = boss.agent.steeringTarget - boss.transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);

        boss.transform.rotation = Quaternion.Slerp(
            boss.transform.rotation,
            targetRot,
            Time.deltaTime * 3f
        );
    }

    public override void Exit()
    {
        boss.agent.isStopped = true;
        boss.agent.ResetPath();
        boss.SetMoveType(0);
    }
}
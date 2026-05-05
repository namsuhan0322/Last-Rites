using UnityEngine;

public class DragonBossPatrolState : DragonBossState
{
    public DragonBossPatrolState(DragonBoss boss, DragonBossStateMachine stateMachine)
        : base(boss, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        boss.agent.isStopped = false;
        boss.agent.speed = boss.PatrolSpeed;

        if (boss.GetRandomPatrolPoint(out Vector3 point))
        {
            boss.agent.SetDestination(point);
        }
        else
        {
            stateMachine.ChangeState(boss.IdleState);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        UpdateMoveAnimation();
        RotateToMoveDirection();

        if (!boss.agent.pathPending &&
            boss.agent.remainingDistance <= boss.agent.stoppingDistance + 0.2f)
        {
            stateMachine.ChangeState(boss.IdleState);
        }
    }

    private void UpdateMoveAnimation()
    {
        Vector3 dir = boss.agent.desiredVelocity;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f)
        {
            boss.SetMoveType(0);
            return;
        }

        float angle = Vector3.SignedAngle(
            boss.transform.forward,
            dir.normalized,
            Vector3.up
        );

        if (angle > 15f)
        {
            boss.SetMoveType(3); // 오른쪽으로 돌면서 걷기
        }
        else if (angle < -15f)
        {
            boss.SetMoveType(2); // 왼쪽으로 돌면서 걷기
        }
        else
        {
            boss.SetMoveType(1); // 앞으로 걷기
        }
    }

    private void RotateToMoveDirection()
    {
        Vector3 dir = boss.agent.desiredVelocity;
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
        boss.SetMoveType(0);
    }
}
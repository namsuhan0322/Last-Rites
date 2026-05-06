using UnityEngine;

public class DragonBossFaceTargetState : DragonBossState
{
    private Transform target;

    public DragonBossFaceTargetState(DragonBoss boss, DragonBossStateMachine stateMachine)
        : base(boss, stateMachine)
    {
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    public override void Enter()
    {
        base.Enter();

        boss.agent.isStopped = true;
        boss.agent.ResetPath();
        boss.agent.velocity = Vector3.zero;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (target == null)
        {
            stateMachine.ChangeState(boss.IdleState);
            return;
        }

        Vector3 dir = target.position - boss.transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f)
        {
            stateMachine.ChangeState(boss.IdleState);
            return;
        }

        float signedAngle = Vector3.SignedAngle(
            boss.transform.forward,
            dir.normalized,
            Vector3.up
        );

        // 거의 정면이면 회전 끝
        if (Mathf.Abs(signedAngle) <= boss.faceFinishAngle)
        {
            boss.SetMoveType(0);
            boss.StartFaceCooldown();
            stateMachine.ChangeState(boss.IdleState);
            return;
        }

        // 오른쪽이 가까우면 오른쪽 턴
        if (signedAngle > 0f)
            boss.SetMoveType(3);
        else
            boss.SetMoveType(2);

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);

        boss.transform.rotation = Quaternion.RotateTowards(
            boss.transform.rotation,
            targetRot,
            boss.turnSpeed * Time.deltaTime
        );
    }

    public override void Exit()
    {
        boss.SetMoveType(0);
    }
}

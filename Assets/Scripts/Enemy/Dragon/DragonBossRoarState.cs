using UnityEngine;

public class DragonBossRoarState : DragonBossState
{
    public DragonBossRoarState(DragonBoss boss, DragonBossStateMachine stateMachine)
        : base(boss, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        Debug.Log("RoarState Enter");

        boss.agent.isStopped = true;
        boss.agent.ResetPath();
        boss.agent.velocity = Vector3.zero;

        boss.SetMoveType(4);
        boss.SetRoared();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (stateTimer >= boss.roarDuration)
        {
            boss.StartFaceCooldown();
            stateMachine.ChangeState(boss.IdleState);
        }
    }

    public override void Exit()
    {
        boss.SetMoveType(0);
    }
}
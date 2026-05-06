using UnityEngine;

public class DragonBossIdleState : DragonBossState
{
    public DragonBossIdleState(DragonBoss boss, DragonBossStateMachine stateMachine)
        : base(boss, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        boss.agent.isStopped = true;
        boss.agent.ResetPath();
        boss.agent.velocity = Vector3.zero;

        boss.SetMoveType(0);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

    }
}
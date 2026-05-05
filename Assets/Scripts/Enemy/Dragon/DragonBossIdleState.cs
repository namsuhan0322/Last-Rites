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

        boss.SetMoveType(0); // Idle
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (stateTimer >= boss.idleTime)
        {
            int rand = Random.Range(0, 3);

            if (rand == 0)
                stateMachine.ChangeState(boss.PatrolState);
            else if (rand == 1)
                stateMachine.ChangeState(boss.TurnLeftState);
            else
                stateMachine.ChangeState(boss.TurnRightState);
        }
    }
}
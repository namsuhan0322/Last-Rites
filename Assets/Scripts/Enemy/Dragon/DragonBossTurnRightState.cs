using UnityEngine;

public class DragonBossTurnRightState : DragonBossState
{
    public DragonBossTurnRightState(DragonBoss boss, DragonBossStateMachine stateMachine)
        : base(boss, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        boss.agent.isStopped = true;
        boss.agent.ResetPath();

        boss.SetMoveType(5); // 제자리 오른쪽 턴
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        boss.transform.Rotate(Vector3.up, boss.turnSpeed * Time.deltaTime);

        if (stateTimer >= boss.turnDuration)
        {
            stateMachine.ChangeState(boss.IdleState);
        }
    }

    public override void Exit()
    {
        boss.SetMoveType(0);
    }
}
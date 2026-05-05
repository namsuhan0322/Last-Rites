using UnityEngine;

public abstract class DragonBossState
{
    protected DragonBoss boss;
    protected DragonBossStateMachine stateMachine;

    protected float stateTimer;

    public DragonBossState(DragonBoss boss, DragonBossStateMachine stateMachine)
    {
        this.boss = boss;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter()
    {
        stateTimer = 0f;
    }

    public virtual void LogicUpdate()
    {
        stateTimer += Time.deltaTime;
    }

    public virtual void Exit()
    {
    }
}
using UnityEngine;

public abstract class PlayerState
{
    protected PlayerController _player;
    protected PlayerStateMachine _stateMachine;

    public PlayerState(PlayerController player, PlayerStateMachine stateMachine)
    {
        _player = player;
        _stateMachine = stateMachine;
    }

    public virtual void Enter() { }
    public virtual void HandleInput() { }
    public virtual void LogicUpdate() { }
    public virtual void PhysicsUpdate() { }
    public virtual void Exit() { }
}
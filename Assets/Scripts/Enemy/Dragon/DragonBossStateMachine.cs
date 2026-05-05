public class DragonBossStateMachine
{
    public DragonBossState CurrentState { get; private set; }

    public void Initialize(DragonBossState startState)
    {
        CurrentState = startState;
        CurrentState.Enter();
    }

    public void ChangeState(DragonBossState newState)
    {
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}
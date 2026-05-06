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
        if (newState == null)
            return;

        if (CurrentState != null)
            CurrentState.Exit();

        CurrentState = newState;
        CurrentState.Enter();
    }
}
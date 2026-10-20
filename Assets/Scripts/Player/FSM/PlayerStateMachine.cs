/// <summary>
/// [역할] 현재 상태 하나를 들고, 바꿀 때 이전 상태 Exit → 교체 → 새 상태 Enter 를 부른다.
/// [이유] 전환 순서를 한곳에서 지켜, 한 프레임 안 상태 연쇄 순서가 바뀌지 않게 한다.
/// [근거] 명세 3.4, 결정 R13
/// </summary>
public class PlayerStateMachine
{
    public PlayerState CurrentState { get; private set; }

    public void Initialize(PlayerState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    public void ChangeState(PlayerState newState)
    {
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}
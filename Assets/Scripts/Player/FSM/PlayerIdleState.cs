/// <summary>
/// [역할] 대기 상태: 우클릭 → 이동, 좌클릭 → 공격, 회피·스킬 입력. 제자리에서 중력만.
/// [이유] 대기만의 입력 처리만 남기고, 대기·이동에 같던 평타 시작은 PlayerState.TryStartBasicAttack을 쓴다.
/// [근거] 명세 3.4
/// </summary>
public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        _player.Anim.SetFloat(PlayerAnimParams.Move, 0f);
        _player.Agent.ResetPath();
    }

    public override void HandleInput()
    {
        // 순서·연쇄는 지금과 같다: 이동으로 바뀌어도 이어서 공격·회피·스킬을 본다 (R13)
        if (PlayerInputReader.MoveHeld) _stateMachine.ChangeState(_player.MoveState);
        TryStartBasicAttack();
        _player.TryRunBufferedSkillOrDodge();
    }

    public override void PhysicsUpdate()
    {
        _player.Motor.StopAndApplyGravity();
    }
}

using UnityEngine;

/// <summary>
/// [역할] 스턴 상태: 3초 동안 아무것도 못 함 (큰 패턴 피격 또는 강인도 0).
/// [이유] 스턴 길이만 이 상태가 정하고, 멈춤은 PlayerState.StopForReaction을 쓴다.
/// [근거] 명세 3.4, 결정 R14, 조작감 C7
/// </summary>
public class PlayerStunState : PlayerState
{
    private const float StunDuration = 3.0f;

    private float _stateTimer;

    public PlayerStunState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override bool IsFullBodyAction => true;
    public override bool BlocksInputBuffer => true;   // 조작감 C7⑤
    public override bool IsIncapacitated => true;

    public override void Enter()
    {
        StopForReaction();
        _player.Anim.SetTrigger(PlayerAnimParams.IsStun);

        _stateTimer = 0f;
    }

    public override void LogicUpdate()
    {
        _player.Motor.StopAndApplyGravity();
        _stateTimer += Time.deltaTime;

        if (_stateTimer >= StunDuration)
        {
            if (!_player.Stats.IsDead)
            {
                _stateMachine.ChangeState(_player.IdleState);
            }
        }
    }

    public override void Exit()
    {
        _player.Anim.ResetTrigger(PlayerAnimParams.IsStun);
    }
}

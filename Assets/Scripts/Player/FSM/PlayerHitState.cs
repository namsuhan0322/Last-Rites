using UnityEngine;

/// <summary>
/// [역할] 피격 경직 상태: 강도에 따라 0.5/0.8/1.2초 경직, 경직 + 0.2초 무적(연속 피격 방지).
/// [이유] 경직 길이·무적 시간만 이 상태가 정하고, 멈춤은 PlayerState.StopForReaction을 쓴다.
/// [근거] 명세 3.4, 결정 R14, 조작감 C7
/// </summary>
public class PlayerHitState : PlayerState
{
    private const float HeavySeverity = 1.0f;         // 이 강도 이상 = 강함/크리티컬
    private const float MediumSeverity = 0.5f;        // 이 강도 이상 = 중간
    private const float HeavyStunTime = 1.2f;
    private const float MediumStunTime = 0.8f;
    private const float LightStunTime = 0.5f;
    private const float ExtraInvincibleTime = 0.2f;   // 경직 시간 + 0.2초 무적 (다구리 방지)

    private float _hitSeverity;
    private float _stateTimer;
    private float _stunDuration;

    public PlayerHitState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override bool IsFullBodyAction => true;
    public override bool BlocksInputBuffer => true;   // 조작감 C7⑤

    public void SetSeverity(float severity)
    {
        _hitSeverity = severity;
    }

    public override void Enter()
    {
        StopForReaction();
        _player.Anim.SetFloat(PlayerAnimParams.HitPower, _hitSeverity);
        _player.Anim.SetTrigger(PlayerAnimParams.IsHit);

        if (_hitSeverity >= HeavySeverity)
            _stunDuration = HeavyStunTime;
        else if (_hitSeverity >= MediumSeverity)
            _stunDuration = MediumStunTime;
        else
            _stunDuration = LightStunTime;

        _player.Stats.SetInvincibleForSeconds(_stunDuration + ExtraInvincibleTime);
        _stateTimer = 0f;
    }

    public override void LogicUpdate()
    {
        _stateTimer += Time.deltaTime;

        _player.Motor.StopAndApplyGravity();

        if (_stateTimer >= _stunDuration)
        {
            if (!_player.Stats.IsDead)
            {
                _stateMachine.ChangeState(_player.IdleState);
            }
        }
    }

    public override void Exit()
    {
        _player.Anim.ResetTrigger(PlayerAnimParams.IsHit);
    }
}

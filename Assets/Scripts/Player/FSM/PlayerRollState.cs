using UnityEngine;

/// <summary>
/// [역할] 구르기 상태: 루트모션 이동, 무적 구간, 공격 예약 연결, 이동 캔슬.
/// [이유] 구르기 타이밍(무적 구간·공격 예약·캔슬)을 상태 안 상수로 모아 한눈에 보이게 한다.
/// [근거] 명세 3.4, 결정 R14, 조작감 C7 / 이상한 동작 Q3(회피 후 공격 딜레이 미적용) 유지
/// </summary>
public class PlayerRollState : PlayerState
{
    private const float AttackChainTime = 0.85f;     // 85% 이후 예약된 공격으로 연결
    private const float InvincibleEndTime = 0.65f;   // 65% 이후 무적 해제, 이동 입력 받음
    private const float RollEndTime = 0.9f;          // 90% 이후 대기로
    private const float EndGraceInvincible = 0.5f;   // 대기로 끝날 때 0.5초 짧은 무적
    private const float NotRollTimeout = 1.5f;       // 구르기 애니가 아닌 채로 1.5초면 대기로

    private float _stateTimer;

    public PlayerRollState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override bool IsFullBodyAction => true;
    public override bool UsesRootMotion => true;

    public override void Enter()
    {
        _player.ResetDashTimer();

        _player.Stats.SetInvincible(true);
        _stateTimer = 0f;
        _player.Anim.applyRootMotion = true;

        _player.Motor.RotateToMouseImmediate();
        _player.ForceDisableAllActionEffects();

        _player.Anim.SetTrigger(PlayerAnimParams.Roll);
        _player.Agent.ResetPath();
    }

    public override void LogicUpdate()
    {
        _stateTimer += Time.deltaTime;

        AnimatorStateInfo stateInfo = _player.Anim.GetCurrentAnimatorStateInfo(PlayerAnimParams.BaseLayer);
        if (stateInfo.IsName(PlayerAnimParams.StateRoll) || stateInfo.IsTag(PlayerAnimParams.TagRoll))
        {
            float normalizedTime = stateInfo.normalizedTime;
            // 평타 선입력이나 좌클릭 누르고 있음 → 85%부터 1타 (스킬·회피 선입력은 구르기가 끝나 대기로 간 뒤 실행, C7)
            if (normalizedTime >= AttackChainTime &&
                (_player.InputBuffer.Kind == BufferedActionKind.Attack || PlayerInputReader.AttackHeld))
            {
                _player.Stats.SetInvincible(false);
                _player.InputBuffer.ClearIfAttack();
                _stateMachine.ChangeState(_player.AttackState);
                return;
            }

            if (normalizedTime >= InvincibleEndTime)
            {
                _player.Stats.SetInvincible(false);

                if (PlayerInputReader.MoveHeld)
                {
                    _stateMachine.ChangeState(_player.MoveState);
                    return;
                }
                else if (normalizedTime >= RollEndTime)
                {
                    _player.Stats.SetInvincibleForSeconds(EndGraceInvincible);
                    _stateMachine.ChangeState(_player.IdleState);
                    return;
                }
            }
        }
        else
        {
            if (_stateTimer >= NotRollTimeout)
            {
                _stateMachine.ChangeState(_player.IdleState);
            }
        }
    }

    public override void Exit()
    {
        _player.Anim.applyRootMotion = false;
        _player.Agent.velocity = Vector3.zero;
        _player.postRollAttackTimer = 0f;
    }
}

using UnityEngine;

/// <summary>
/// [역할] 스킬 상태: 스킬 애니 재생, 40% 이후 다른 스킬·회피·이동 캔슬(선입력), 마지막 타격이 끝나면 평타 연결, 80% 이후 대기.
/// [이유] 스킬만의 캔슬·연결 시점만 남기고, 공격·스킬 공통 시작은 PlayerState.BeginActionAnimation에 둔다.
/// [근거] 명세 3.4, 결정 R14, 조작감 C3·C4·C7 / Q1은 버그 수정 3단계에서 고침(B9)
/// </summary>
public class PlayerSkillState : PlayerState
{
    private const float CancelTime = 0.4f;          // 40% 이후 다른 스킬·회피·이동으로 캔슬 가능
    private const float AttackChainFallback = 0.55f;    // 타격 이벤트가 없는 스킬(R 버프)의 평타 연결 시점 (예전 고정값, C3)
    private const float SkillEndTime = 0.8f;        // 80% 이후 대기로
    private const float NotSkillTimeout = 0.5f;     // 스킬 애니가 아닌 채로 0.5초면 대기로
    private const float CrossFadeTime = 0.1f;       // 대기/이동 애니로 0.1초 섞어 넘어감

    private float _stateTimer;

    public PlayerSkillState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override bool IsFullBodyAction => true;
    public override bool CanDealDamage => true;

    public override void Enter()
    {
        // 스킬 트리거 이름은 TryUseSkill이 CurrentSkillAnim에 기록해 둔다
        BeginActionAnimation(Animator.StringToHash(_player.CurrentSkillAnim));

        _stateTimer = 0f;

        _player.globalSkillTimer = _player.globalSkillDelay;
    }

    public override void LogicUpdate()
    {
        _player.Motor.StopAndApplyGravity();
        _stateTimer += Time.deltaTime;

        if (_player.Anim.IsInTransition(PlayerAnimParams.BaseLayer)) return;

        AnimatorStateInfo stateInfo = _player.Anim.GetCurrentAnimatorStateInfo(PlayerAnimParams.BaseLayer);

        if (stateInfo.IsTag(PlayerAnimParams.TagSkill))
        {
            float normalizedTime = stateInfo.normalizedTime;
            float attackChainAt = Mathf.Min(_player.CancelPoints.CurrentHitEnd(AttackChainFallback), SkillEndTime);
            ChainDecision decision = ActionChainRule.Decide(normalizedTime, CancelTime, attackChainAt,
                _player.InputBuffer.Kind, PlayerInputReader.AttackHeld);

            // 선입력 스킬·회피: 40%부터 (그 전에 누른 것도 기억, C4·C7). 공통 딜레이면 다음 프레임에 다시
            if (decision == ChainDecision.RunBuffered && _player.TryRunBufferedSkillOrDodge())
            {
                return;
            }

            // 이동 캔슬: 40%부터 (그대로)
            if (normalizedTime >= CancelTime && (PlayerInputReader.MoveHeld || PlayerInputReader.MovePressed))
            {
                _player.Anim.CrossFade(PlayerAnimParams.StateIdleMove, CrossFadeTime);
                _stateMachine.ChangeState(_player.MoveState);
                return;
            }

            // 평타: 마지막 타격이 끝나는 순간 바로 1타 (C3. 넘기기는 PlayerAttackState.Enter가 섞어서 함)
            if (decision == ChainDecision.StartAttack)
            {
                _player.InputBuffer.ClearIfAttack();
                _stateMachine.ChangeState(_player.AttackState);
                return;
            }

            if (normalizedTime >= SkillEndTime)
            {
                _player.Anim.CrossFade(PlayerAnimParams.StateIdleMove, CrossFadeTime);
                _stateMachine.ChangeState(_player.IdleState);
            }
        }
        else
        {
            if (_stateTimer > NotSkillTimeout) _stateMachine.ChangeState(_player.IdleState);
        }
    }

    public override void Exit()
    {
        _player.Anim.ResetTrigger(_player.CurrentSkillAnim);
        _player.DisableWeaponCollider();
    }
}

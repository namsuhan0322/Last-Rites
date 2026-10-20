using UnityEngine;

/// <summary>
/// [역할] 평타 상태: 콤보(타격이 끝나면 다음 타, 선입력), 후딜레이 이동 캔슬, 회피·스킬 캔슬, 스킬 애니에서 1타로 섞어 넘기기.
/// [이유] 평타만의 규칙(콤보·캔슬 시점)만 남기고, 공격·스킬 공통 시작은 PlayerState.BeginActionAnimation에 둔다.
/// [근거] 명세 3.4, 결정 R14, R30, 조작감 C5·C6·C7
/// </summary>
public class PlayerAttackState : PlayerState
{
    private const float MinCheckTime = 0.1f;        // 들어오고 0.1초는 판정하지 않음 (이전 애니가 남아 있을 수 있어서)
    private const float MoveCancelTime = 0.6f;      // 공격 애니 60% 이후 이동으로 후딜레이 캔슬
    private const float ComboTime = 0.95f;          // 95% 이후 콤보 연결 또는 종료
    private const float NotAttackTimeout = 0.5f;    // 공격 애니가 아닌 채로 0.5초 지나면 대기로
    private const float SkillToAttackBlend = 0.1f;  // 스킬 애니 → 1타로 섞는 시간(초). 애니메이터에 이 전환이 없어 코드가 넘긴다 (C6)

    private float _stateTimer;

    public PlayerAttackState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override bool IsFullBodyAction => true;
    public override bool CanDealDamage => true;

    public override void Enter()
    {
        BeginActionAnimation(PlayerAnimParams.Attack);

        // 스킬 애니에서 바로 넘어올 때: 애니메이터에 스킬 → 1타 전환이 없으므로 직접 섞어 넘기고 방금 건 트리거는 지운다.
        // 트리거를 남기면 1타에 들어가자마자 1타 → 2타 전환(종료 시점 없음)이 바로 일어난다 (C6)
        if (_player.Anim.GetCurrentAnimatorStateInfo(PlayerAnimParams.BaseLayer).IsTag(PlayerAnimParams.TagSkill))
        {
            _player.Anim.ResetTrigger(PlayerAnimParams.Attack);
            _player.Anim.CrossFadeInFixedTime(PlayerAnimParams.StateAttack1, SkillToAttackBlend, PlayerAnimParams.BaseLayer);
        }

        _stateTimer = 0f;
    }

    public override void HandleInput()
    {
        // 평타 중 스킬·회피 선입력은 언제든 바로 (지금과 같음). 평타 선입력은 LogicUpdate가 타격 끝에 쓴다
        if (_player.TryRunBufferedSkillOrDodge()) return;

        if (PlayerInputReader.MovePressed)
        {
            _player.Anim.ResetTrigger(PlayerAnimParams.Attack);
            _stateMachine.ChangeState(_player.MoveState);
        }
    }

    public override void LogicUpdate()
    {
        _player.Motor.StopAndApplyGravity();
        _stateTimer += Time.deltaTime;

        if (_player.Anim.IsInTransition(PlayerAnimParams.BaseLayer)) return;

        AnimatorStateInfo stateInfo = _player.Anim.GetCurrentAnimatorStateInfo(PlayerAnimParams.BaseLayer);

        if (stateInfo.IsTag(PlayerAnimParams.TagAttack))
        {
            if (_stateTimer < MinCheckTime) return;

            float normalizedTime = stateInfo.normalizedTime;

            // [공격 중] 후딜레이 캔슬
            if (normalizedTime >= MoveCancelTime)
            {
                if (PlayerInputReader.MovePressed || PlayerInputReader.MoveHeld)
                {
                    _stateMachine.ChangeState(_player.MoveState);
                    return;
                }
            }

            // [공격 중] 콤보: 평타 선입력이나 좌클릭 누르고 있음 → 이번 타격이 끝난 뒤 다음 타 (C5, 시점은 C11 슬라이더).
            // 기다리는 스킬·회피가 있으면 평타로 가지 않는다 (C7②, 실행은 HandleInput)
            float chainAt = CurrentComboChainPoint();
            ChainDecision decision = ActionChainRule.Decide(normalizedTime, 0f, chainAt,
                _player.InputBuffer.Kind, PlayerInputReader.AttackHeld);
            if (decision == ChainDecision.StartAttack)
            {
                _player.InputBuffer.ClearIfAttack();
                _stateMachine.ChangeState(_player.AttackState);
                return;
            }

            // [공격 중] 이을 입력이 없으면 95%에서 대기로 (그대로)
            if (normalizedTime >= ComboTime)
            {
                _stateMachine.ChangeState(_player.IdleState);
            }
        }
        else
        {
            if (_stateTimer > NotAttackTimeout)
            {
                _stateMachine.ChangeState(_player.IdleState);
            }
        }
    }

    /// 지금 평타 애니에서 다음 타로 넘어가는 진행도: 타격 끝(슬라이더 0) ~ 예전 95%(슬라이더 1) 사이 (C5·C11). 테스트 표시도 쓴다
    public float CurrentComboChainPoint()
    {
        float hitEnd = Mathf.Min(_player.CancelPoints.CurrentHitEnd(ComboTime), ComboTime);
        return Mathf.Lerp(hitEnd, ComboTime, _player.comboChainDelay);
    }

    public override void Exit()
    {
        _player.Anim.ResetTrigger(PlayerAnimParams.Attack);
        _player.DisableWeaponCollider();
        _player.ForceDisableAllActionEffects();
    }
}

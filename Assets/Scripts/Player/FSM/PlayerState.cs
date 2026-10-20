using UnityEngine;

/// <summary>
/// [역할] 플레이어 상태의 공통 틀. 상태마다 다른 처리(Enter·HandleInput·LogicUpdate·PhysicsUpdate·Exit)와,
///        여러 상태가 같이 쓰는 시작 코드(보조 함수), 컨트롤러가 묻는 상태 성격(속성)을 둔다.
/// [이유] 공격·스킬 / 피격·스턴·사망 / 대기·이동에 같은 코드가 반복됐고, 컨트롤러가 상태를 6곳에서 하나하나 비교했다.
/// [근거] 명세 3.4, 결정 R13(상태 연쇄·호출 위치 유지), R29(상태 속성)
/// </summary>
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

    #region 상태 성격 (컨트롤러가 상태를 하나하나 비교하던 것을 대신함)
    /// 몸 전체를 쓰는 상태라 상체 레이어를 끈다 (사망·스턴·피격·공격·구르기·스킬)
    public virtual bool IsFullBodyAction => false;
    /// 히트박스·액션 이펙트를 켜도 되는 상태 (공격·스킬)
    public virtual bool CanDealDamage => false;
    /// 피격 반응·스턴·포션을 막는 상태 (스턴·사망)
    public virtual bool IsIncapacitated => false;
    /// 루트모션으로 움직이는 상태 (구르기)
    public virtual bool UsesRootMotion => false;
    /// 이 상태에서는 선입력을 모으지 않고 비운다 (피격·스턴·사망 — 풀린 뒤 예전 입력이 나가지 않게, 조작감 C7⑤)
    public virtual bool BlocksInputBuffer => false;
    #endregion

    #region 공통 보조 함수
    /// 공격·스킬 시작: 무적 해제 → 무기 꺼내기 → 경로 초기화 → 이동값 0 → 상체 레이어 끄기 → 마우스 방향 회전 → 트리거 → 공속
    protected void BeginActionAnimation(int triggerHash)
    {
        _player.Stats.SetInvincible(false);

        // 칼 뽑는 애니메이션 이벤트가 스킵되었을 때를 대비한 강제 동기화
        if (_player.VisualManager != null)
            _player.VisualManager.DrawWeapon();

        _player.Agent.ResetPath();
        _player.Anim.SetFloat(PlayerAnimParams.Move, 0f);
        _player.Anim.SetLayerWeight(PlayerAnimParams.UpperBodyLayer, 0f);   // 공격·스킬은 전신

        _player.Motor.RotateToMouseImmediate();

        _player.Anim.ResetTrigger(triggerHash);
        _player.Anim.SetTrigger(triggerHash);

        // 공속 = 무기 공속 × 버프 배율. 예전 공격 상태의 GetEffectiveAttackSpeed(false)도 항상 AtkSpeedModifier를 돌려줬다
        float baseAtkSpd = _player.CurrentWeapon != null ? _player.CurrentWeapon.Atk_Spd : 1f;
        _player.Anim.SetFloat(PlayerAnimParams.AttackSpd, baseAtkSpd * _player.AtkSpeedModifier);
    }

    /// 피격·스턴·사망 시작: 경로 초기화 → 속도 0 → 상체 레이어 끄기 → 액션 이펙트 끄기
    protected void StopForReaction()
    {
        _player.Agent.ResetPath();
        _player.Agent.velocity = Vector3.zero;
        _player.Anim.SetLayerWeight(PlayerAnimParams.UpperBodyLayer, 0f);
        _player.ForceDisableAllActionEffects();
    }

    /// 대기·이동에서 평타 선입력: 전투 중이고 회피 후 딜레이가 끝났으면 공격 상태로. 못 하면 버린다 (조작감 C7③).
    /// 상태가 바뀌어도 부른 쪽은 이어서 남은 입력을 처리한다 (R13. 단 선입력은 한 칸이라 같은 프레임의 회피와 겹치면 회피만 — C8)
    protected void TryStartBasicAttack()
    {
        if (_player.InputBuffer.Kind != BufferedActionKind.Attack) return;
        _player.InputBuffer.Clear();

        if (_player.InCombat && _player.postRollAttackTimer <= 0f)
        {
            _stateMachine.ChangeState(_player.AttackState);
        }
    }
    #endregion
}

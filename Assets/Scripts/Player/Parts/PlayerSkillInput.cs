using UnityEngine;

/// <summary>
/// [역할] 회피·스킬 입력: ① 매 프레임 처음, 누른 키를 선입력 한 칸에 모은다 ② 상태가 허락할 때 선입력의 회피·스킬을 실행한다.
///        실행: 회피(쿨이면 버림) / 스킬은 전투 중일 때만, E는 보호막 중이면 막힘, 공통 스킬 딜레이면 남겨 두고 다음 프레임.
/// [이유] 키를 그 프레임에만 읽던 방식에서, 일찍 누른 입력도 기억했다가 끊을 수 있는 순간 실행하는 선입력으로 바꾼다(로스트아크식).
/// [근거] 리팩토링 명세 3.4 PlayerSkillInput, 조작감 명세 3.1·3.2, 결정 C7 / Q1(쿨 중 스킬 키도 정보 기록)은 버그 수정 3단계에서 고침(B9)
/// </summary>
public class PlayerSkillInput
{
    private readonly PlayerController _owner;
    private readonly TutorialGate _tutorial;
    private readonly SkillTutorial _skillTutorial;   // 튜토리얼 씬에서만 있음. Q 사용을 알려 준다
    private readonly ActionInputBuffer _buffer;

    public PlayerSkillInput(PlayerController owner, TutorialGate tutorial, SkillTutorial skillTutorial, ActionInputBuffer buffer)
    {
        _owner = owner;
        _tutorial = tutorial;
        _skillTutorial = skillTutorial;
        _buffer = buffer;
    }

    /// 매 프레임 상태 처리 전에: 이번 프레임에 누른 평타·스킬·회피를 선입력에 넣는다 (튜토리얼 스킬 잠금 중이면 스킬은 빼고, C7⑥)
    public void Collect()
    {
        _buffer.Collect(PlayerInputReader.AttackPressed, PlayerInputReader.SkillPressed,
            PlayerInputReader.DashPressed, _tutorial.SkillsLocked);
    }

    /// 선입력에 회피·스킬이 있으면 실행한다. 상태가 회피·스킬로 바뀌었으면 true.
    /// 실패(회피 쿨, 적 없음, 보호막 중 E, 쿨타임·스태미나)는 버리고, 공통 스킬 딜레이만 남겨 둔다 (C7③)
    public bool TryRunBuffered()
    {
        if (_buffer.Kind == BufferedActionKind.Dodge)
        {
            _buffer.Clear();
            if (_owner.DashTimer > 0) return false;   // 회피 쿨 중이면 버림
            _owner.StateMachine.ChangeState(_owner.RollState);
            return true;
        }

        if (_buffer.Kind != BufferedActionKind.Skill) return false;

        SkillSlot slot = _buffer.Slot;

        if (!_owner.InCombat)
        {
            Debug.Log("주변에 적이 없어 스킬을 사용할 수 없습니다!");
            _buffer.Clear();
            return false;
        }

        if (slot == SkillSlot.E && _owner.isWizardShieldActive)
        {
            Debug.Log("보호막이 이미 유지되고 있어 스킬을 쓸 수 없습니다!");
            _buffer.Clear();
            return false;
        }

        // 공통 스킬 딜레이(R은 보지 않음)가 남았으면 버리지 않고 다음 프레임에 다시 (짧은 스킬끼리 연결, C7③ 예외)
        if (slot != SkillSlot.R && _owner.globalSkillTimer > 0f) return false;

        _buffer.Clear();
        if (!TryCast(slot)) return false;

        if (slot == SkillSlot.Q) _skillTutorial?.OnPlayerUsedQSkill();
        return true;
    }

    /// 무기의 그 칸 값으로 사용 판정 → 통과하면 스킬 상태로. R은 R값·버프 실행 판정을 쓴다
    private bool TryCast(SkillSlot slot)
    {
        SkillSpec skill = _owner.CurrentWeapon.GetSkill(slot);
        bool ok = slot == SkillSlot.R
            ? _owner.TryUse_RSkill(KeyCode.R, PlayerAnimParams.SkillR, skill.Value, skill.Cooldown, skill.Stamina)
            : _owner.TryUseSkill(PlayerInputReader.SkillKey(slot), AnimName(slot), skill.Damage, skill.Cooldown, skill.Stamina);
        if (!ok) return false;

        _owner.StateMachine.ChangeState(_owner.SkillState);
        return true;
    }

    private static string AnimName(SkillSlot slot)
    {
        switch (slot)
        {
            case SkillSlot.Q: return PlayerAnimParams.SkillQ;
            case SkillSlot.W: return PlayerAnimParams.SkillW;
            case SkillSlot.E: return PlayerAnimParams.SkillE;
            case SkillSlot.R: return PlayerAnimParams.SkillR;
            default: return PlayerAnimParams.SkillV;
        }
    }
}

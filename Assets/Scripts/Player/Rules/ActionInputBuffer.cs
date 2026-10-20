using System;

/// <summary>
/// [역할] 선입력 종류: 없음, 평타, 스킬(칸은 Slot에), 회피.
/// [이유] 선입력 한 칸에 무엇이 들었는지 이름으로 나눠, 상태마다 실행 시점 규칙을 엔진 없이 테스트한다.
/// [근거] 조작감 명세 3.1, 결정 C5·C7
/// </summary>
public enum BufferedActionKind
{
    None,
    Attack,
    Skill,
    Dodge
}

/// <summary>
/// [역할] 선입력 한 칸: 행동 중 누른 다음 행동(평타·스킬·회피) 하나를 기억한다. 새 입력이 덮어쓰고, 쓰면 비운다.
/// [이유] 지금은 스킬 40% 전에 누른 키를 그 프레임에만 읽고 버려 다시 눌러야 했다. 일찍 누른 입력을 끊을 수 있는 순간 실행한다(로스트아크식, C2).
/// [근거] 조작감 명세 3.1·3.2, 결정 C5·C7 (① 한 프레임 우선순위 ④ 시간 제한 없음 ⑥ 튜토리얼 스킬 잠금)
/// </summary>
public class ActionInputBuffer
{
    public BufferedActionKind Kind { get; private set; }
    public SkillSlot Slot { get; private set; }   // Kind가 Skill일 때만 의미 있음

    public bool HasSkillOrDodge => Kind == BufferedActionKind.Skill || Kind == BufferedActionKind.Dodge;

    public void PushAttack() => Set(BufferedActionKind.Attack, SkillSlot.Q);
    public void PushSkill(SkillSlot slot) => Set(BufferedActionKind.Skill, slot);
    public void PushDodge() => Set(BufferedActionKind.Dodge, SkillSlot.Q);
    public void Clear() => Set(BufferedActionKind.None, SkillSlot.Q);

    /// 평타 선입력일 때만 비운다 (평타로 넘어갈 때. 기다리는 스킬·회피는 남긴다)
    public void ClearIfAttack()
    {
        if (Kind == BufferedActionKind.Attack) Clear();
    }

    /// 한 프레임에 누른 키를 모은다. 우선순위가 낮은 것부터 넣어 가장 높은 것 하나만 남긴다:
    /// 회피 > Q > W > E > R > V > 평타 (지금 입력 검사 순서와 같음, C7①). 아무것도 안 눌렀으면 그대로 둔다.
    public void Collect(bool attackPressed, Func<SkillSlot, bool> skillPressed, bool dodgePressed, bool skillsLocked)
    {
        if (attackPressed) PushAttack();

        if (!skillsLocked)   // 튜토리얼 스킬 잠금 중에는 스킬 키를 모으지 않는다 (C7⑥)
        {
            for (int i = (int)SkillSlot.V; i >= (int)SkillSlot.Q; i--)
            {
                SkillSlot slot = (SkillSlot)i;
                if (skillPressed(slot)) PushSkill(slot);
            }
        }

        if (dodgePressed) PushDodge();
    }

    private void Set(BufferedActionKind kind, SkillSlot slot)
    {
        Kind = kind;
        Slot = slot;
    }
}

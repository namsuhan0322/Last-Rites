/// <summary>
/// [역할] 스킬 사용 판정 결과.
/// [이유] 실패 이유를 나눠, 검사 순서와 "사용에 성공했을 때만 스킬 정보 기록" 규칙을 테스트한다.
/// [근거] 명세 3.2 SkillUseRule, 버그 수정 명세 B9
/// </summary>
public enum SkillUseResult
{
    BlockedByGlobalDelay,   // 공통 스킬 딜레이가 남음 → 아무것도 기록하지 않음
    NotEnoughStamina,       // 스태미나 부족 → 아무것도 기록하지 않음
    OnCooldown,             // 쿨타임 중 → 실패, 아무것도 기록하지 않음 (예전에는 기록했음 — Q1·Q5, 버그 수정 3단계에서 고침)
    Ready                   // 사용 가능
}

/// <summary>
/// [역할] 스킬을 지금 쓸 수 있는지 판정한다.
/// [이유] PlayerController.TryUseSkill / TryUse_RSkill 의 검사 순서를 엔진 없이 테스트하기 위해.
/// [근거] 명세 3.2 SkillUseRule (순서: 공통 딜레이 → 스태미나 → 쿨타임, R은 공통 딜레이 생략)
/// </summary>
public static class SkillUseRule
{
    /// checkGlobalDelay: Q/W/E/V는 true, R은 false (지금 코드의 R은 공통 딜레이를 보지 않음)
    public static SkillUseResult Evaluate(bool checkGlobalDelay, float globalDelayRemaining, float currentStamina, int staminaCost, bool isCooldownReady)
    {
        if (checkGlobalDelay && globalDelayRemaining > 0f) return SkillUseResult.BlockedByGlobalDelay;
        if (currentStamina < staminaCost) return SkillUseResult.NotEnoughStamina;
        return isCooldownReady ? SkillUseResult.Ready : SkillUseResult.OnCooldown;
    }

    /// 스킬 애니 이름·데미지(R은 R값)를 기록해야 하는 결과인지. 사용에 성공했을 때만 (Q1·Q5 고침, 버그 수정 B9)
    public static bool RecordsSkillInfo(SkillUseResult result) => result == SkillUseResult.Ready;
}

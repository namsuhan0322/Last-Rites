using System;
using System.Collections.Generic;

/// <summary>
/// [역할] 애니메이션 클립의 이벤트(함수 이름, 0~1 시점)에서 "타격이 끝나는 시점"을 찾는다.
///        마지막 DisableWeaponCollider → 없으면 마지막 타격·생성 이벤트 → 없으면 기본값.
/// [이유] 무기·스킬마다 끊는 시점 숫자를 따로 맞추지 않고, 애니에 이미 있는 타격 이벤트로 자동 계산한다.
/// [근거] 조작감 명세 2.3·3.1, 결정 C3·C5
/// </summary>
public static class CancelPointRule
{
    public const string HitEndEvent = "DisableWeaponCollider";

    // 타격 시작·범위 공격 생성 이벤트 (끄기 이벤트가 없는 클립에서 쓴다)
    private static readonly string[] HitStartEvents =
    {
        "EnableWeaponCollider", "UseExcaliburSkill",
        "FireWizardSkill_Q", "FireWizardSkill_W", "FireWizardSkill_E", "FireWizardSkill_V"
    };

    public static float HitEnd(IReadOnlyList<(string Name, float Time)> events, float fallback)
    {
        float lastEnd = -1f;
        float lastHit = -1f;

        for (int i = 0; i < events.Count; i++)
        {
            (string name, float time) = events[i];
            if (name == HitEndEvent)
            {
                if (time > lastEnd) lastEnd = time;
            }
            else if (Array.IndexOf(HitStartEvents, name) >= 0)
            {
                if (time > lastHit) lastHit = time;
            }
        }

        if (lastEnd >= 0f) return lastEnd;
        if (lastHit >= 0f) return lastHit;
        return fallback;
    }
}

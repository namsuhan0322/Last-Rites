/// <summary>
/// [역할] 지금 애니메이터 상태가 어떤 타격인지 (데미지 공식 선택용).
/// [이유] 애니메이터 판정(엔진)과 공식(순수 계산)을 나눠, 공식만 엔진 없이 테스트하기 위해.
/// [근거] 명세 3.2 PlayerDamageCalculator
/// </summary>
public enum AttackHitKind
{
    Combo1,     // 애니 상태 "Attack1"
    Combo2,     // "Attack2"
    Combo3,     // "Attack3" (막타)
    Skill,      // 태그 "Skill"
    Other       // 그 밖
}

/// <summary>
/// [역할] 플레이어가 주는 데미지 공식.
/// [이유] EnableWeaponCollider · UseExcaliburSkill · SpawnMagicSkill 세 곳에 복사돼 있던 계산을 한 곳으로 모으고
///        엔진 없이 테스트하기 위해.
/// [근거] 명세 3.2, 결정 R8, 이상한 동작 Q2·Q6 (지금 동작 유지)
/// </summary>
public static class PlayerDamageCalculator
{
    public const float ComboMultiplier = 1.0f;        // 1·2타
    public const float FinisherMultiplier = 1.2f;     // 3타(막타)는 1.2배
    public const float SkillHitMultiplier = 2.0f;     // 히트박스형 스킬
    public const float SpearProjectileRatio = 0.5f;   // 창 버프 추가 투사체는 데미지의 절반

    /// 히트박스(마법사는 평타 마법탄)에 실을 데미지 = ((기본 + 무기) × 배율) + 고정치.
    /// 1타의 고정치도 Combo_1이라, 강화가 없으면 무기 공격력(Combo_1)과 겹쳐 두 번 들어간다 (Q2).
    public static int WeaponHit(int baseAttack, int weaponAttack, AttackHitKind kind, int combo1, int combo2, int combo3, int skillDamage)
    {
        float multiplier = 1.0f;
        int fixedBonus = 0;
        switch (kind)
        {
            case AttackHitKind.Combo1: multiplier = ComboMultiplier; fixedBonus = combo1; break;
            case AttackHitKind.Combo2: multiplier = ComboMultiplier; fixedBonus = combo2; break;
            case AttackHitKind.Combo3: multiplier = FinisherMultiplier; fixedBonus = combo3; break;
            case AttackHitKind.Skill: multiplier = SkillHitMultiplier; fixedBonus = skillDamage; break;
        }

        // 계산 순서·자료형(int+int → ×float → +int)은 지금 코드와 같게 둔다 (float 반올림 결과가 같아야 함)
        float finalDamage = ((baseAttack + weaponAttack) * multiplier) + fixedBonus;
        return RuleMath.RoundToInt(finalDamage);
    }

    /// 대검 R 버프 중이면 × R값
    public static int ApplyGreatSwordBuff(int damage, bool buffActive, float rValue)
        => buffActive ? RuleMath.RoundToInt(damage * rValue) : damage;

    /// 생성형 스킬(엑스칼리버·마법사 Q/W/E/V): 기본·무기 공격력 없이 스킬 데미지만, 대검 버프만 적용 (Q6)
    public static int SpawnedSkill(int skillDamage, bool greatSwordBuffActive, float rValue)
        => ApplyGreatSwordBuff(skillDamage, greatSwordBuffActive, rValue);

    public static int SpearProjectile(int damage) => RuleMath.RoundToInt(damage * SpearProjectileRatio);
}

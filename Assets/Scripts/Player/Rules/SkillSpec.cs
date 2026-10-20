/// <summary>
/// [역할] 스킬 한 칸의 값 묶음: 데미지(Q/W/E/V), R값(R), 쿨타임, 스태미나.
/// [이유] WeaponSO의 Q_Dmg·Q_Cool·Q_Stamina … 15개 필드를 칸 단위로 읽기 위해 (필드·에셋은 그대로, 결정 R2).
/// [근거] 명세 3.5, 결정 R32
/// </summary>
public readonly struct SkillSpec
{
    public readonly int Damage;      // Q/W/E/V 스킬 데미지 (R은 0)
    public readonly float Value;     // R 스킬 값 R_Val (Q/W/E/V는 0)
    public readonly float Cooldown;
    public readonly int Stamina;

    public SkillSpec(int damage, float value, float cooldown, int stamina)
    {
        Damage = damage;
        Value = value;
        Cooldown = cooldown;
        Stamina = stamina;
    }
}

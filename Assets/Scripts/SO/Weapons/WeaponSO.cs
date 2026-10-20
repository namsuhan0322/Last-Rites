using UnityEngine;

/// <summary>
/// [역할] 무기 데이터: 평타 콤보 데미지, 스킬 Q/W/E/V 데미지·쿨·스태미나, R 버프 값, 애니메이터, 아이콘.
///        값은 BGDatabase 변환기(BgDbToWeaponSoConverterWindow)가 채운다.
/// [이유] GetSkill로 칸 단위 읽기를 더해, 쓰는 쪽이 Q/W/E/V 필드를 하나하나 고르지 않게 한다 (필드는 그대로).
/// [근거] 명세 3.5, 결정 R2, R32
/// </summary>
[CreateAssetMenu(fileName = "WeaponSO", menuName = "WeaponSO")]
public class WeaponSO : ScriptableObject
{
    public int WeaponID;
    public string name;
    public WeaponType weaponType;
    public float Atk_Spd;
    public int Combo_1;
    public int Combo_2;
    public int Combo_3;
    public int Q_Dmg;
    public float Q_Cool;
    public int Q_Stamina;
    public int W_Dmg;
    public float W_Cool;
    public int W_Stamina;
    public int E_Dmg;
    public float E_Cool;
    public int E_Stamina;
    public float R_Val;
    public float R_Cool;
    public int R_Stamina;
    public int V_Dmg;
    public float V_Cool;
    public int V_Stamina;

    public Sprite weaponIcon;
    [TextArea(3, 5)] public string description;

    public BuffSkill_SO R_Skill_Logic;
    public AnimatorOverrideController weaponAnimator;

    [Tooltip("적을 명중시켰을 때 나는 무기 특유의 소리")]
    public string hitSoundID;

    /// 칸별 스킬 값 읽기. 필드·에셋·변환기는 그대로 두고 읽는 쪽만 묶는다 (결정 R2)
    public SkillSpec GetSkill(SkillSlot slot)
    {
        switch (slot)
        {
            case SkillSlot.Q: return new SkillSpec(Q_Dmg, 0f, Q_Cool, Q_Stamina);
            case SkillSlot.W: return new SkillSpec(W_Dmg, 0f, W_Cool, W_Stamina);
            case SkillSlot.E: return new SkillSpec(E_Dmg, 0f, E_Cool, E_Stamina);
            case SkillSlot.R: return new SkillSpec(0, R_Val, R_Cool, R_Stamina);
            default: return new SkillSpec(V_Dmg, 0f, V_Cool, V_Stamina);
        }
    }
}

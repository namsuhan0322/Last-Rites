using UnityEngine;

[CreateAssetMenu(fileName = "GreatSword_Buff", menuName = "Weapon/R_Skills/GreatSword_Buff")]
public class GreatSwordBuff_SO : BuffSkill_SO
{
    public override void Execute(PlayerController player, float rVal)
    {
        Debug.Log($"[{skillName}] 발동! R_Val({rVal})배 데미지 뻥튀기 및 슈퍼 아머 적용!");

        // 플레이어에게 슈퍼 아머 상태 부여 (HitState 무시 로직 활성화)
        player.HasRBuff = true;

        // 다음 1회 타격 데미지 증가 세팅 (rVal을 배수로 사용)
        player.CurrentSkillVal = rVal;

        // 붉은색 투기 이펙트 켜기
        player.EnableREffect();
    }

    public override void EndBuff(PlayerController player)
    {
        Debug.Log($"[{skillName}] 버프 종료!");

        player.HasRBuff = false;
    }
}
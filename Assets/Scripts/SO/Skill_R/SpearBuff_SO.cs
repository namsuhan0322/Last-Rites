using UnityEngine;

/// <summary>
/// [역할] 창 R: duration(8)초 동안 평타마다 데미지 50% 투사체 추가(HasSpearBuff), R 아우라.
/// [이유] 끝내기는 부모 공통 코루틴 RunForDuration을 쓴다.
/// [근거] 명세 3.5 / 기획서와 다름 D4
/// </summary>
[CreateAssetMenu(fileName = "Spear_Buff", menuName = "Weapon/R_Skills/Spear_Buff")]
public class SpearBuff_SO : BuffSkill_SO
{
    [Header("창 버프 설정")]
    public float duration = 8f;

    public override void Execute(PlayerController player, float rVal)
    {
        player.HasSpearBuff = true;
        player.EnableREffect();

        player.StartCoroutine(RunForDuration(player, duration));   // 지속 시간 뒤 EndBuff (부모 공통)
    }

    public override void EndBuff(PlayerController player)
    {
        player.HasSpearBuff = false;
        player.DisableREffect();
    }
}
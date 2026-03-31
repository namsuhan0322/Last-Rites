using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "SwordShield_Buff", menuName = "Weapon/R_Skills/SwordShield_Buff")]
public class SwordShieldBuff_SO : BuffSkill_SO
{
    [Header("검방 버프 설정")]
    public float duration = 5f;

    public override void Execute(PlayerController player, float rVal)
    {
        Debug.Log($"[{skillName}] 발동! {duration}초간 데미지 70% 감소 및 반격 활성화!");

        // 데미지 감소 및 반격 플래그 켜기
        player.HasShieldBuff = true;

        player.StartCoroutine(BuffTimer(player));
    }

    private IEnumerator BuffTimer(PlayerController player)
    {
        yield return new WaitForSeconds(duration);
        EndBuff(player);
    }

    public override void EndBuff(PlayerController player)
    {
        Debug.Log($"[{skillName}] 지속 시간 종료. 철벽 방어 해제.");

        player.HasShieldBuff = false;
    }
}
using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "TwinSwords_Buff", menuName = "Weapon/R_Skills/TwinSwords_Buff")]
public class TwinSwordsBuff_SO : BuffSkill_SO
{
    [Header("쌍검 버프 설정")]
    public float duration = 10f;
    public float atkSpeedMultiplier = 1.3f;
    public float moveSpeedMultiplier = 1.2f;

    public override void Execute(PlayerController player, float rVal)
    {
        Debug.Log($"[{skillName}] 발동! {duration}초간 이속/공속 증가, 회피 스태미나 감소!");

        // 공속 및 이속 증가 적용
        player.Anim.speed = atkSpeedMultiplier;
        player.Agent.speed = player.Stats.MoveSpeed * moveSpeedMultiplier;

        // 플레이어에 쌍검 버프 상태 켜기 (스태미나 감소용 플래그)
        player.HasTwinBuff = true; 

        player.StartCoroutine(BuffTimer(player));
    }

    private IEnumerator BuffTimer(PlayerController player)
    {
        yield return new WaitForSeconds(duration);
        EndBuff(player);
    }

    public override void EndBuff(PlayerController player)
    {
        Debug.Log($"[{skillName}] 지속 시간 종료. 스탯 롤백!");

        // 스탯 원래대로 복구
        player.Anim.speed = 1.0f;
        player.Agent.speed = player.Stats.MoveSpeed;

        player.HasTwinBuff = false;
    }
}
using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Spear_Buff", menuName = "Weapon/R_Skills/Spear_Buff")]
public class SpearBuff_SO : BuffSkill_SO
{
    [Header("창 버프 설정")]
    public float duration = 8f;
    public float hitboxLengthMultiplier = 1.2f;

    // 원래 히트박스 크기를 저장해둘 변수
    private Vector3 originalHitboxScale;

    public override void Execute(PlayerController player, float rVal)
    {
        Debug.Log($"[{skillName}] 발동! {duration}초간 방어구 관통 및 사거리 증가!");

        // 방어력 무시 플래그 켜기
        player.HasSpearBuff = true;

        // 히트박스 길이(Z축) 늘리기
        if (player.Hitbox != null)
        {
            originalHitboxScale = player.Hitbox.transform.localScale;
            Vector3 expandedScale = originalHitboxScale;
            expandedScale.z *= hitboxLengthMultiplier; // 앞뒤 길이를 늘림
            player.Hitbox.transform.localScale = expandedScale;
        }

        player.StartCoroutine(BuffTimer(player));
    }

    private IEnumerator BuffTimer(PlayerController player)
    {
        yield return new WaitForSeconds(duration);
        EndBuff(player);
    }

    public override void EndBuff(PlayerController player)
    {
        Debug.Log($"[{skillName}] 지속 시간 종료.");

        player.HasSpearBuff = false;

        // 히트박스 원래 크기로 복구
        if (player.Hitbox != null)
        {
            player.Hitbox.transform.localScale = originalHitboxScale;
        }
    }
}
using UnityEngine;

/// <summary>
/// [역할] 검방 R: duration(5)초 동안 HasShieldBuff를 켠다. 이 플래그를 읽는 곳은 아직 없다(피해 감소·반격 미구현).
/// [이유] 끝내기는 부모 공통 코루틴 RunForDuration을 쓴다.
/// [근거] 명세 3.5 / 미구현 D3 (지금은 마법사 무기의 R에 연결됨)
/// </summary>
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

        player.StartCoroutine(RunForDuration(player, duration));   // 지속 시간 뒤 EndBuff (부모 공통)
    }

    public override void EndBuff(PlayerController player)
    {
        Debug.Log($"[{skillName}] 지속 시간 종료. 철벽 방어 해제.");

        player.HasShieldBuff = false;
    }
}
using UnityEngine;
using System.Collections; // 코루틴을 위해 추가

/// <summary>
/// [역할] 대검 R(광전사): duration초 동안 슈퍼아머·데미지 ×R값(HasGreatSwordBuff), 공속 ×1.3, 1초마다 HP 5 감소.
/// [이유] 1초마다 HP를 깎아야 해서 부모 공통 코루틴(RunForDuration) 대신 자기 코루틴을 쓴다.
/// [근거] 명세 3.5, 결정 R17 / 기획서와 다름 D5, 무기 교체해도 유지 Q14
/// </summary>
[CreateAssetMenu(fileName = "GreatSword_Buff", menuName = "Weapon/R_Skills/GreatSword_Buff")]
public class GreatSwordBuff_SO : BuffSkill_SO
{
    [Header("광전사 세팅")]
    public float duration = 10.0f;                  // 지속 시간
    public float attackSpeedMultiplier = 1.3f;      // 공속 증가 배율
    public int hpDrainPerSecond = 5;                // 초당 체력 소모량

    public override void Execute(PlayerController player, float rVal)
    {
        player.StartCoroutine(BerserkerRoutine(player, rVal));
    }

    private IEnumerator BerserkerRoutine(PlayerController player, float rVal)
    {
        player.HasGreatSwordBuff = true;
        player.CurrentSkillVal = rVal;
        player.AtkSpeedModifier = attackSpeedMultiplier;
        player.EnableREffect();

        float timer = duration;

        while (timer > 0f)
        {
            if (player.Stats.IsDead) break;

            yield return new WaitForSeconds(1.0f);

            player.Stats.DrainHP(hpDrainPerSecond);

            if (DamageEffectManager.Instance != null)
            {
                Vector3 textPos = player.transform.position + (Vector3.up * 1.5f);
                DamageEffectManager.Instance.ShowDamage(textPos, -hpDrainPerSecond, Color.red);
            }

            timer -= 1.0f;
        }

        EndBuff(player);
    }

    public override void EndBuff(PlayerController player)
    {
        player.HasGreatSwordBuff = false;
        player.AtkSpeedModifier = 1.0f;

        player.DisableREffect();
    }
}
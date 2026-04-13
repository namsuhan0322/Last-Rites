using UnityEngine;
using System.Collections; // 코루틴을 위해 추가

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
        player.HasRBuff = true;
        player.CurrentSkillVal = rVal;
        player.AtkSpeedModifier = attackSpeedMultiplier;
        player.EnableREffect();

        float timer = duration;

        while (timer > 0f)
        {
            if (player.Stats.IsDead) break;

            yield return new WaitForSeconds(1.0f);

            player.Stats.DrainHP(hpDrainPerSecond);
            timer -= 1.0f;
        }

        EndBuff(player);
    }

    public override void EndBuff(PlayerController player)
    {
        player.HasRBuff = false;
        player.AtkSpeedModifier = 1.0f;

        player.DisableREffect();
    }
}
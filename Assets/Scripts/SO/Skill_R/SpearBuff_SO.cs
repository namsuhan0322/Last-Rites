using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Spear_Buff", menuName = "Weapon/R_Skills/Spear_Buff")]
public class SpearBuff_SO : BuffSkill_SO
{
    [Header("창 버프 설정")]
    public float duration = 8f;

    public override void Execute(PlayerController player, float rVal)
    {
        player.HasSpearBuff = true;
        player.EnableREffect();

        player.StartCoroutine(BuffTimer(player));
    }

    private IEnumerator BuffTimer(PlayerController player)
    {
        yield return new WaitForSeconds(duration);
        EndBuff(player);
    }

    public override void EndBuff(PlayerController player)
    {
        player.HasSpearBuff = false;
        player.DisableREffect();
    }
}
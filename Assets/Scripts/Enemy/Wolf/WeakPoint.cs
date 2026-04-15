using UnityEngine;

public class WeakPoint : Actor
{
    private WolfBoss boss;

    public void Init(int hp, WolfBoss owner)
    {
        boss = owner;
        InitActor(hp);
    }

    public override void TakeDamage(int damage, float severityOverride = -1f, bool isHeavyAttack = false, bool showDamageText = true)
    {
        base.TakeDamage(damage, severityOverride, isHeavyAttack, false);

         if (boss != null)
         {
             boss.TakeDamage(damage, severityOverride, isHeavyAttack, false);
         }
    }

    protected override void Die()
    {
        base.Die();

        if (boss != null)
        {
            boss.OnWeakPointBreak();
        }

        gameObject.SetActive(false);
    }
}
using UnityEngine;

public class WeakPoint : Actor
{
    private WolfBoss boss;
    public WeakPointUI ui;
    bool isInitialized = false;
    public void Init(int hp, WolfBoss owner)
    {
        boss = owner;

        if (!isInitialized)
        {
            InitActor(hp);
            ui.Init(hp);
            isInitialized = true;
        }
        else
        {
            ui.SetHP(_currentHP);
        }
    }

    public override void TakeDamage(int damage, float severityOverride = -1f, bool isHeavyAttack = false, bool showDamageText = true)
    {
        base.TakeDamage(damage, severityOverride, isHeavyAttack, false);

        ui.SetHP(_currentHP); 

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
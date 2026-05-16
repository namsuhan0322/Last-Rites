using UnityEngine;

public class WeakPoint : Actor
{
    private Actor boss;
    public WeakPointUI ui;

    private bool isInitialized = false;

    public void Init(int hp, Actor owner)
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
            boss.TakeDamage(damage, severityOverride, isHeavyAttack, false);
    }

    protected override void Die()
    {
        base.Die();

        DragonBoss dragon = boss as DragonBoss;
        if (dragon != null)
            dragon.OnHeadWeakPointBreak();

        WolfBoss wolf = boss as WolfBoss;
        if (wolf != null)
            wolf.OnWeakPointBreak();

        gameObject.SetActive(false);
    }
}
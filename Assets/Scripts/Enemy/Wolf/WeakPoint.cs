using UnityEngine;

public class WeakPoint : Actor
{
    private WolfBoss boss;

    public void Init(int hp, WolfBoss owner)
    {
        boss = owner;
        InitActor(hp);
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
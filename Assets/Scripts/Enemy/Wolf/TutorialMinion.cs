using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMinion : Enemy
{
    public SkillTutorial skillTutorial;

    bool triggered = false;

    protected override void Awake()
    {
        base.Awake();

        if (skillTutorial == null)
            skillTutorial = FindFirstObjectByType<SkillTutorial>();

        OnStun += HandleStunTutorial;
    }

    void HandleStunTutorial()
    {
        skillTutorial?.OnEnemyStunned();
    }

    public override void TakeDamage(int damage)
    {
        if (_isDead) return;

        base.TakeDamage(damage);

        if (_isDead) return;

        if (!triggered)
        {
            triggered = true;

            skillTutorial?.OnFirstHitEnemy();
        }
    }
}

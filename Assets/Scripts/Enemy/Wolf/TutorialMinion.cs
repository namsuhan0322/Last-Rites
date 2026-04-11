using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMinion : Enemy
{
    public SkillTutorial skillTutorial;
    public TutorialSystem tutorialSystem;

    bool triggered = false;

    int damageCount = 0;
    static bool missionCleared = false;
    public void KillMinion()
    {
        Die();
    }

    protected override void Awake()
    {
        base.Awake();

        missionCleared = false;

        if (skillTutorial == null)
            skillTutorial = FindFirstObjectByType<SkillTutorial>();

        if (tutorialSystem == null)
            tutorialSystem = FindFirstObjectByType<TutorialSystem>();

        OnStun += HandleStunTutorial;
    }

    void HandleStunTutorial()
    {
        skillTutorial?.OnEnemyStunned();
    }

    public override void TakeDamage(int damage, float severityOverride = -1f, bool isHeavyAttack = false)
    {
        if (_isDead) return;

        if (tutorialSystem != null && tutorialSystem.tutorialPlaying)
            return;

        base.TakeDamage(damage);

        if (_isDead) return;

        if (!triggered)
        {
            triggered = true;
            skillTutorial?.OnFirstHitEnemy();
        }

        if (missionCleared) return;

        damageCount++;

        if (damageCount >= 8)
        {
            missionCleared = true;

            tutorialSystem.ShowMission("스킬 튜토리얼 완료");
            tutorialSystem.StartBossPhase();
        }
    }
}

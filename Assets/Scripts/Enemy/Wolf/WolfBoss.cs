using UnityEngine;
using static BossPhase;
using System.Collections;
using System.Collections.Generic;

public class WolfBoss : Enemy
{
    [Header("보스 페이즈")]
    public BossPhase currentPhase = BossPhase.Phase1;
    public float phase2HpPercent = 0.6f;
    public float phase3HpPercent = 0.3f;

    [Header("콤보 공격")]
    public float comboDelay = 0.5f;
    public float comboRecovery = 1.5f;

    int comboIndex = 0;
    bool isComboAttacking = false;
    bool isPhaseChanging = false;

    protected override void Update()
    {
        base.Update();

        if (_isDead) return;

        UpdatePhase();
        UpdateIdleState();

        if (isPhaseChanging)
        {
            agent.isStopped = true;
            return;
        }
    }

    void UpdateIdleState()
    {
        if (agent == null) return;
        if (isAttacking || isPhaseChanging) return;

        bool isMoving = agent.velocity.magnitude > 0.1f;

        if (!isMoving)
        {
            animator.SetBool("Run_P1", false);
            animator.SetBool("Run_P2", false);

            if (currentPhase == BossPhase.Phase1)
            {
                animator.SetBool("Phase1Idle", true);
                animator.SetBool("Phase2Idle", false);
            }
            else
            {
                animator.SetBool("Phase1Idle", false);
                animator.SetBool("Phase2Idle", true);
            }
        }
        else
        {
            animator.SetBool("Phase1Idle", false);
            animator.SetBool("Phase2Idle", false);

            if (currentPhase == BossPhase.Phase1)
            {
                animator.SetBool("Run_P1", true);
                animator.SetBool("Run_P2", false);
            }
            else
            {
                animator.SetBool("Run_P1", false);
                animator.SetBool("Run_P2", true);
            }
        }
    }

    void UpdatePhase()
    {
        if (isPhaseChanging) return;

        float hpPercent = (float)_currentHP / _maxHP;

        if (currentPhase == BossPhase.Phase1 && hpPercent <= phase2HpPercent)
        {
            StartCoroutine(ChangeToPhase2());
        }
        else if (currentPhase == BossPhase.Phase2 && hpPercent <= phase3HpPercent)
        {
            StartCoroutine(ChangeToPhase3());
        }
    }

    IEnumerator ChangeToPhase2()
    {
        isPhaseChanging = true;
        isAttacking = true;
        agent.isStopped = true;

        animator.SetTrigger("PhaseRoar");

        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName("PhaseRoar")
        );
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
        );

        currentPhase = BossPhase.Phase2;
        attackTimer = 0f;

        isAttacking = false;
        isPhaseChanging = false;
        isComboAttacking = false; 

        agent.isStopped = false;
    }

    IEnumerator ChangeToPhase3()
    {
        isPhaseChanging = true;
        isAttacking = true;
        agent.isStopped = true;

        animator.SetTrigger("PhaseRoar");

        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName("PhaseRoar")
        );

        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
        );

        currentPhase = BossPhase.Phase3;
        attackTimer = 0f;

        isAttacking = false;
        isPhaseChanging = false;
        isComboAttacking = false; 

        agent.isStopped = false;
    }

    protected override void TryAttack()
    {
        if (isPhaseChanging) return;
        if (isComboAttacking) return; 

        base.TryAttack();
    }

    protected override void Attack()
    {
        if (isComboAttacking) return;

        StartCoroutine(ComboAttack());
    }

    IEnumerator ComboAttack()
    {
        isAttacking = true;
        isComboAttacking = true;

        agent.isStopped = true;
        agent.updateRotation = false;

        agent.velocity = Vector3.zero;
        agent.ResetPath();

        Vector3 dir = currentTarget.position - transform.position;
        dir.y = 0;
        attackDirection = dir.normalized;

        transform.rotation = Quaternion.LookRotation(attackDirection);

        if (currentPhase == BossPhase.Phase1)
        {
            animator.SetTrigger("AttackReady_P1");
        }
        else
        {
            animator.SetTrigger("AttackReady_P2");
        }

        yield return new WaitForSeconds(1.5f);

        if (currentPhase == BossPhase.Phase1)
        {
            animator.SetTrigger("Attack1_P1");
            yield return new WaitForSeconds(comboDelay);

            animator.SetTrigger("Attack2_P1");
            yield return new WaitForSeconds(comboDelay);

            animator.SetTrigger("Attack3_P1");
        }
        else
        {
            animator.SetTrigger("Attack1_P2");
            yield return new WaitForSeconds(comboDelay);

            animator.SetTrigger("Attack2_P2");
            yield return new WaitForSeconds(comboDelay);

            animator.SetTrigger("Attack3_P2");
        }

        yield return new WaitForSeconds(comboRecovery);

        agent.updateRotation = true;
        agent.isStopped = false;

        isComboAttacking = false;
        EndAttack();

        attackTimer = attackCooldown;
    }

    public override void TakeDamage(int damage, float severityOverride = -1f)
    {
        if (isHit || _isDead) return;
        base.TakeDamage(damage, severityOverride);

        EndHit();
    }
}

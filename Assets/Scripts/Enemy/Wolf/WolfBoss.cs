using UnityEngine;
using static BossPhase;
using System.Collections;
using System.Collections.Generic;

public class WolfBoss : Enemy
{
    [Header("보스 페이즈")]
    public BossPhase currentPhase = BossPhase.Phase1;
    public float phase2HpPercent = 0.6f;

    [Header("Phase1 콤보")]
    public float comboDelay_P1 = 0.7f;
    public float comboRecovery_P1 = 2.0f;

    [Header("1페이지 할퀴기 스킬")]
    public float slashRange = 4f;
    public float slashAngle = 120f;
    public int slashDamage = 20;
    public float slashCooldown = 3f;

    public GameObject slashIndicatorPrefab;

    float slashTimer = 0f;
    GameObject slashIndicator;

    [Header("Phase2 콤보")]
    public float comboDelay_P2 = 0.3f;
    public float comboRecovery_P2 = 1.0f;

    int comboIndex = 0;
    bool isComboAttacking = false;
    bool isPhaseChanging = false;

    protected override void Awake()
    {
        base.Awake();

        slashIndicator = Instantiate(slashIndicatorPrefab, transform);
        slashIndicator.SetActive(false);
    }

    protected override void Update()
    {
        attackTimer -= Time.deltaTime;
        slashTimer -= Time.deltaTime;
        if (_isDead) return;

        if (attackTimer > 0f || isPhaseChanging || isComboAttacking)
        {
            agent.isStopped = true;

            animator.SetBool("Run_P1", false);
            animator.SetBool("Run_P2", false);

            return;
        }

        base.Update();

        UpdatePhase();
        UpdateIdleState();
    }

    void UpdateIdleState()
    {
        if (agent == null) return;
        if (isAttacking || isPhaseChanging) return;

        bool isMoving = !agent.isStopped && agent.velocity.magnitude > 0.1f;

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
    }

    IEnumerator ChangeToPhase2()
    {
        isPhaseChanging = true;
        isAttacking = true;
        agent.isStopped = true;

        animator.SetTrigger("PhaseRoar");

        yield return new WaitForSeconds(2.0f);

        currentPhase = BossPhase.Phase2;
        attackTimer = 0f;

        isAttacking = false;
        isPhaseChanging = false;
        isComboAttacking = false; 

        agent.isStopped = false;
    }

    protected override void TryAttack()
    {
        if (isPhaseChanging || isComboAttacking) return;
        if (currentTarget == null) return;

        float dist = Vector3.Distance(transform.position, currentTarget.position);

        // ⭐ 1페이즈 할퀴기 우선
        if (currentPhase == BossPhase.Phase1 && slashTimer <= 0f && dist <= slashRange)
        {
            StartCoroutine(Slash());
            return;
        }

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

        float delay = (currentPhase == BossPhase.Phase1) ? comboDelay_P1 : comboDelay_P2;
        float recovery = (currentPhase == BossPhase.Phase1) ? comboRecovery_P1 : comboRecovery_P2;

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
            yield return new WaitForSeconds(delay);

            animator.SetTrigger("Attack2_P1");
            yield return new WaitForSeconds(delay);

            animator.SetTrigger("Attack3_P1");
        }
        else
        {
            animator.SetTrigger("Attack1_P2");
            yield return new WaitForSeconds(delay);

            animator.SetTrigger("Attack2_P2");
            yield return new WaitForSeconds(delay);

            animator.SetTrigger("Attack3_P2");
        }

        yield return new WaitForSeconds(recovery);

        agent.updateRotation = true;
        agent.isStopped = false;

        isComboAttacking = false;
        EndAttack();

        attackTimer = attackCooldown;
    }

    IEnumerator Slash()
    {
        isAttacking = true;
        isComboAttacking = true;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.updateRotation = false;

        Vector3 dir = currentTarget.position - transform.position;
        dir.y = 0;
        attackDirection = dir.normalized;
        transform.rotation = Quaternion.LookRotation(attackDirection);

        ShowSlashIndicator();

        yield return new WaitForSeconds(1.5f);

        slashIndicator.SetActive(false);

        animator.SetTrigger("Slash");

        yield return new WaitForSeconds(0.3f);

        DealSlashDamage();

        yield return new WaitForSeconds(1.0f);

        slashTimer = slashCooldown;

        isComboAttacking = false;
        EndAttack();

        agent.isStopped = false;
        agent.updateRotation = true;
    }

    void ShowSlashIndicator()
    {
        slashIndicator.SetActive(true);

        float diameter = slashRange * 2f;

        slashIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);

        Vector3 pos = transform.position + attackDirection * (slashRange * 0.5f);
        pos.y += 0.05f;

        slashIndicator.transform.position = pos;

        slashIndicator.transform.rotation =
            Quaternion.LookRotation(attackDirection) * Quaternion.Euler(90f, 0, 0);
    }

    void DealSlashDamage()
    {
        Vector3 center = transform.position + attackDirection * (slashRange * 0.5f);

        Collider[] hits = Physics.OverlapSphere(
            center,
            slashRange,
            targetLayer
        );

        foreach (var hit in hits)
        {
            Actor actor = hit.GetComponent<Actor>();
            if (actor == null || actor == this) continue;

            Vector3 toTarget = (actor.transform.position - transform.position).normalized;

            float dot = Vector3.Dot(attackDirection, toTarget);

            float cos = Mathf.Cos(slashAngle * 0.5f * Mathf.Deg2Rad);

            if (dot >= cos)
            {
                actor.TakeDamage(slashDamage, 1f);
            }
        }
    }

    public override void TakeDamage(int damage, float severityOverride = -1f)
    {
        if (isHit || _isDead) return;
        base.TakeDamage(damage, severityOverride);

        EndHit();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ElitePhase
{
    Phase1,
    Phase2,
    Phase3
}

public class WolfElite : Enemy
{
    [Header("페이즈 설정")]
    public ElitePhase currentPhase = ElitePhase.Phase1;

    public float phase2HpPercent = 0.6f;   //페이즈 변환 퍼센트

    bool isPhaseChanging = false;

    [Header("스킬 쿨타임")]
    public float stompCooldown = 5f;
    public float doubleStompCooldown = 8f;

    float stompTimer = 0f;
    float doubleStompTimer = 0f;

    bool isSkillAttacking = false;

    protected override void Update()
    {
        base.Update();

        if (_isDead) return;

        UpdatePhase();
        UpdateSkillCooldowns();
    }

    void UpdatePhase()
    {
        if (currentPhase == ElitePhase.Phase1)
        {
            float hpPercent = (float)_currentHP / _maxHP;

            if (hpPercent <= phase2HpPercent)
            {
                StartCoroutine(ChangeToPhase2());
            }
        }
    }

    //페이즈2 변환
    IEnumerator ChangeToPhase2()
    {
        isPhaseChanging = true;
        agent.isStopped = true;

        currentPhase = ElitePhase.Phase2;

        animator.SetTrigger("PhaseRoar");

        yield return new WaitForSeconds(2f); // 포효 애니 길이

        agent.isStopped = false;
        isPhaseChanging = false;
    }


    //스킬 쿹라임
    void UpdateSkillCooldowns()
    {
        stompTimer -= Time.deltaTime;
        doubleStompTimer -= Time.deltaTime;
    }

    //공겫히도
    protected override void TryAttack()
    {
        if (currentTarget == null) return;
        if (isSkillAttacking || isPhaseChanging) return;

        float dist = Vector3.Distance(transform.position, currentTarget.position);
        if (dist > attackRange) return;

        switch (currentPhase)
        {
            case ElitePhase.Phase1:
                TryBasicAttack();
                break;

            case ElitePhase.Phase2:
                TryPhase2Attack();
                break;
        }
    }

    //기본 공격
    void TryBasicAttack()
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer > 0f)
        {
            SetPhaseIdle();
            return;
        }

        RotateToTarget();
        animator.SetTrigger("BasicAttack");

        attackTimer = attackCooldown;
    }

    //페이즈2 공격
    void TryPhase2Attack()
    {
        if (stompTimer <= 0f)
        {
            StartCoroutine(Stomp());
            return;
        }

        if (doubleStompTimer <= 0f)
        {
            StartCoroutine(DoubleStomp());
            return;
        }

        TryBasicAttack();
    }

    //오른발 내려찍기
    IEnumerator Stomp()
    {
        isSkillAttacking = true;
        agent.isStopped = true;

        RotateToTarget();
        animator.SetTrigger("Stomp");

        stompTimer = stompCooldown;

        yield return new WaitForSeconds(1.5f);

        agent.isStopped = false;
        isSkillAttacking = false;
    }

    //양발 내려찍기
    IEnumerator DoubleStomp()
    {
        isSkillAttacking = true;
        agent.isStopped = true;

        RotateToTarget();
        animator.SetTrigger("DoubleStomp");

        doubleStompTimer = doubleStompCooldown;

        yield return new WaitForSeconds(2f);

        agent.isStopped = false;
        isSkillAttacking = false;
    }


    //페이즈 변환
    void SetPhaseIdle()
    {
        if (currentPhase == ElitePhase.Phase1)
        {
            animator.SetBool("Phase1Idle", true);
            animator.SetBool("Phase2Idle", false);
        }
        else if (currentPhase == ElitePhase.Phase2)
        {
            animator.SetBool("Phase1Idle", false);
            animator.SetBool("Phase2Idle", true);
        }
    }
}

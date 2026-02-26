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

    protected override void Awake()
    {
        base.Awake();

        animator.SetBool("Phase1Idle", true);
        animator.SetBool("Phase2Idle", false);
    }

    //업데이트
    protected override void Update()
    {
        base.Update();

        if (_isDead) return;

        attackTimer -= Time.deltaTime;  

        UpdatePhase();
        UpdateSkillCooldowns();
        UpdateIdleState();
    }

    //페이즈변환업데이트
    void UpdatePhase()
    {
        if (currentPhase != ElitePhase.Phase1) return;
        if (isPhaseChanging) return;

        float hpPercent = (float)_currentHP / _maxHP;

        if (hpPercent <= phase2HpPercent)
        {
            StartCoroutine(ChangeToPhase2());
        }
    }

    //페이즈2 변환
    IEnumerator ChangeToPhase2()
    {
        isAttacking = true;
        isPhaseChanging = true;
        agent.isStopped = true;

        animator.SetTrigger("PhaseRoar");

        yield return null;
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(state.length);

        currentPhase = ElitePhase.Phase2;

        animator.SetBool("Phase1Idle", false);
        animator.SetBool("Phase2Idle", true);

        isAttacking = false;
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
        if (isAttacking || isSkillAttacking || isPhaseChanging) return;

        float dist = Vector3.Distance(transform.position, currentTarget.position);
        if (dist > attackRange) return;

        if (currentPhase == ElitePhase.Phase2)
        {
            TryPhase2Attack();
            return;
        }

        if (attackTimer <= 0f)
        {
            StartCoroutine(EliteBasicAttack());
        }
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
        base.TryAttack();
    }

    //오른발 내려찍기
    IEnumerator Stomp()
    {
        isAttacking = true;
        isSkillAttacking = true;
        agent.isStopped = true;

        RotateToTarget();
        animator.SetTrigger("Stomp");

        yield return new WaitForSeconds(1.5f);

        yield return new WaitForSeconds(2f); // 후딜

        isAttacking = false;
        isSkillAttacking = false;
    }

    //양발 내려찍기
    IEnumerator DoubleStomp()
    {
        isAttacking = true;
        isSkillAttacking = true;
        agent.isStopped = true;

        RotateToTarget();
        animator.SetTrigger("DoubleStomp");

        doubleStompTimer = doubleStompCooldown;

        yield return new WaitForSeconds(2f);

        actionLockTimer = 2f;

        isAttacking = false;
        agent.isStopped = false;
        isSkillAttacking = false;
    }

    //Idle 변환 스테이트
    void UpdateIdleState()
    {
        if (agent == null) return;
        if (isAttacking || isSkillAttacking || isPhaseChanging) return; 

        bool isMoving = agent.velocity.magnitude > 0.1f;

        if (!isMoving && !agent.pathPending)
        {
            animator.SetBool("Walk", false);
            animator.SetBool("Run", false);

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
        else
        {
            animator.SetBool("Phase1Idle", false);
            animator.SetBool("Phase2Idle", false);
        }
    }


    //기본공격
    IEnumerator EliteBasicAttack()
    {
        isAttacking = true;
        agent.isStopped = true;

        attackTimer = attackCooldown;

        RotateToTarget();
        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(1.24f); // 공격 애니 길이

        attackTimer = attackCooldown;
        agent.isStopped = false;
        isAttacking = false;
    }

    //데미지 받기
    public override void TakeDamage(int damage)
    {
        if (isHit || _isDead) return;
        base.TakeDamage(damage);

        EndHit();
    }
}

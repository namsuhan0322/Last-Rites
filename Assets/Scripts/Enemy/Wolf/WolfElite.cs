using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BossPhase;

public class WolfElite : Enemy
{
    [Header("페이즈 설정")]
    public BossPhase currentPhase = BossPhase.Phase1;
    public float phase2HpPercent = 0.6f;   //페이즈 변환 퍼센트


    [Header("스킬")]
    public float stompCooldown = 5f;
    public float doubleStompCooldown = 8f;
    public GameObject stompIndicatorPrefab;
  
    float stompTimer = 0f;
    float doubleStompTimer = 0f;

    bool isSkillAttacking = false;
    bool isPhaseChanging = false;
    GameObject stompIndicator;


    //플레이어를 바라보고 있나?
    bool IsFacingTarget()
    {
        if (currentTarget == null) return false;

        Vector3 dir = (currentTarget.position - transform.position).normalized;

        float dot = Vector3.Dot(transform.forward, dir);

        return dot > 0.6f;
    }

    protected override void Awake()
    {
        base.Awake();

        animator.SetBool("Phase1Idle", true);
        animator.SetBool("Phase2Idle", false);

        stompIndicator = Instantiate(stompIndicatorPrefab, transform);
        stompIndicator.SetActive(false);   
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
        if (currentPhase != BossPhase.Phase1) return;
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

        currentPhase = BossPhase.Phase2;

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

        if (!IsFacingTarget())
        {
            RotateToTarget();
            return;
        }

        if (currentPhase == BossPhase.Phase2)
        {
            TryPhase2Attack();
            return;
        }

        if (attackTimer <= 0f)
        {
            Attack();
        }
    }
    //페이즈2 공격
    void TryPhase2Attack()
    {
        bool canStomp = stompTimer <= 0f;
        bool canDouble = doubleStompTimer <= 0f;
        bool canBasic = attackTimer <= 0f;

        if (canStomp || canDouble || canBasic)
        {
            float rand = Random.value;

            if (canDouble && rand < 0.33f)
            {
                StartCoroutine(DoubleStomp());
                return;
            }

            if (canStomp && rand < 0.66f)
            {
                StartCoroutine(Stomp());
                return;
            }

            if (canBasic)
            {
                Attack();
                return;
            }
        }
    }
    //오른발 내려찍기
    IEnumerator Stomp()
    {
        isAttacking = true;
        isSkillAttacking = true;
        agent.isStopped = true;

        stompTimer = stompCooldown;  

        RotateToTarget();
        animator.SetTrigger("Stomp");

        yield return new WaitForSeconds(1.5f);

        yield return new WaitForSeconds(2f);

        attackTimer = 1.5f;

        isAttacking = false;
        isSkillAttacking = false;
        agent.isStopped = false;
    }

    //양발 내려찍기
    IEnumerator DoubleStomp()
    {
        isAttacking = true;
        isSkillAttacking = true;
        agent.isStopped = true;

        RotateToTarget();

        stompIndicator.SetActive(true);
        stompIndicator.transform.localPosition = Vector3.zero;
        stompIndicator.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        stompIndicator.transform.localScale = Vector3.one * attackRange * 2f;

        yield return new WaitForSeconds(0.8f);

        stompIndicator.SetActive(false);

        animator.SetTrigger("DoubleStomp");
        doubleStompTimer = doubleStompCooldown;

        yield return new WaitForSeconds(2f);

        attackTimer = 2.0f;
        isAttacking = false;
        isSkillAttacking = false;
        agent.isStopped = false;
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

            if (currentPhase == BossPhase.Phase1)
            {
                animator.SetBool("Phase1Idle", true);
                animator.SetBool("Phase2Idle", false);
            }
            else if (currentPhase == BossPhase.Phase2)
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
    protected override void Attack()
    {
        isAttacking = true;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        RotateToTarget();

        animator.SetTrigger("Attack"); // 엘리트 공격 애니메이션

        attackTimer = attackCooldown;
    }

    //데미지 받기
    public override void TakeDamage(int damage, float severityOverride = -1f)
    {
        if (isHit || _isDead) return;
        base.TakeDamage(damage);

        EndHit();
    }
}

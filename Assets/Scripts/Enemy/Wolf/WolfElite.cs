using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BossPhase;

public class WolfElite : Enemy
{
    [Header("페이즈 설정")]
    public BossPhase currentPhase = BossPhase.Phase1;
    public float phase2HpPercent = 0.6f;   //2페이즈 변환 퍼센트
    public float phase3HpPercent = 0.3f;   //3페이즈 변환 퍼센트

    [Header("찍기 스킬")]
    public int doubleStompDamage = 15;
    public float stompCooldown = 5f;
    public float doubleStompCooldown = 8f;
    public GameObject stompIndicatorPrefab;
   
    [Header("돌진 스킬")]
    public float chargeCooldown = 10f;
    public float chargeDistance = 10f;
    public float chargeSpeed = 20f;
    public GameObject chargeIndicatorPrefab;

    float chargeTimer = 0f;
    GameObject chargeIndicator;

    bool isCharging = false;
    float stompTimer = 0f;
    float doubleStompTimer = 0f;
    bool isSkillAttacking = false;
    bool isPhaseChanging = false;
    GameObject stompIndicator;
    bool isRecovering = false;
    public float skillRecoveryTime = 2f;

    public float rotateSpeed = 5f;

    //플레이어를 바라보고 있나?
    bool IsFacingTarget()
    {
        if (currentTarget == null) return false;

        Vector3 dir = (currentTarget.position - transform.position).normalized;

        float dot = Vector3.Dot(transform.forward, dir);

        return dot > 0.95f;
    }

    protected override void Awake()
    {
        base.Awake();

        animator.SetBool("Phase1Idle", true);
        animator.SetBool("Phase2Idle", false);

        stompIndicator = Instantiate(stompIndicatorPrefab, transform);
        stompIndicator.SetActive(false);

        chargeIndicator = Instantiate(chargeIndicatorPrefab, transform);
        chargeIndicator.SetActive(false);
    }

    //업데이트
    protected override void Update()
    {
        if (_isDead)
            return;

        base.Update();

        if (_isDead)
            return;

        attackTimer -= Time.deltaTime;

        UpdatePhase();
        UpdateSkillCooldowns();
        UpdateIdleState();

        if (isAttacking || isSkillAttacking || isPhaseChanging || isRecovering)
        {
            StopAllMovement();
            return;
        }

        if (!isAttacking && !isSkillAttacking && !isRecovering)
        {
            RotateToTargetSmooth();
        }
    }

    void StopAllMovement()
    {
        if (agent == null) return;

        agent.updateRotation = false;

        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);
    }


    //페이즈변환업데이트
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

    //페이즈2 변환  
    IEnumerator ChangeToPhase2()
    {
        isAttacking = true;
        isPhaseChanging = true;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();

        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);

        animator.SetTrigger("PhaseRoar");

        yield return new WaitForSeconds(2f);

        currentPhase = BossPhase.Phase2;

        animator.SetBool("Phase1Idle", false);
        animator.SetBool("Phase2Idle", true);

        isAttacking = false;
        isPhaseChanging = false;
        agent.isStopped = false;
    }
    //페이즈3 변환
    IEnumerator ChangeToPhase3()
    {
        isAttacking = true;
        isPhaseChanging = true;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();

        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);

        animator.SetTrigger("PhaseRoar");

        yield return new WaitForSeconds(2f);

        currentPhase = BossPhase.Phase3;

        isAttacking = false;
        isPhaseChanging = false;
        agent.isStopped = false;
    }  
    //스킬 쿹라임
    void UpdateSkillCooldowns()
    {
        stompTimer -= Time.deltaTime;
        doubleStompTimer -= Time.deltaTime;
        chargeTimer -= Time.deltaTime; 
    }
    
    //공격시도
    protected override void TryAttack()
    {
        if (_isDead) return;
        if (currentTarget == null) return;
        if (isAttacking || isSkillAttacking || isPhaseChanging || isRecovering)
        {
            StopAllMovement();
            agent.isStopped = true;
            agent.updateRotation = false;

            return;
        }

        float dist = Vector3.Distance(transform.position, currentTarget.position);
        if (dist > attackRange) return;

        if (!IsFacingTarget())
        {
            RotateToTarget();
            return;
        }

        if (currentPhase == BossPhase.Phase3)
        {
            TryPhase3Attack();
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
    //페이즈3 공격
    void TryPhase3Attack()
    {
        if (_isDead) return;
        if (isAttacking || isSkillAttacking || isRecovering || isPhaseChanging) return;

        List<System.Action> patterns = new List<System.Action>();

        if (chargeTimer <= 0f)
            patterns.Add(() => StartCoroutine(Charge()));

        if (doubleStompTimer <= 0f)
            patterns.Add(() => StartCoroutine(DoubleStomp()));

        if (stompTimer <= 0f)
            patterns.Add(() => StartCoroutine(Stomp()));

        if (attackTimer <= 0f)
            patterns.Add(() => Attack());

        if (patterns.Count == 0) return;

        int index = Random.Range(0, patterns.Count);
        patterns[index].Invoke();
    }
    //오른발 내려찍기
    IEnumerator Stomp()
    {
        agent.updateRotation = false;
        Debug.DrawRay(transform.position, attackDirection * 3f, Color.red, 1f);
        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);

        animator.SetBool("Phase1Idle", currentPhase == BossPhase.Phase1);
        animator.SetBool("Phase2Idle", currentPhase == BossPhase.Phase2);

        isAttacking = true;
        isSkillAttacking = true;
        agent.isStopped = true;
        stompTimer = stompCooldown;
        agent.velocity = Vector3.zero;
        agent.ResetPath();

        Vector3 dir = currentTarget.position - transform.position;
        dir.y = 0;

        attackDirection = dir.normalized;
        transform.rotation = Quaternion.LookRotation(attackDirection);
        Debug.DrawRay(transform.position, attackDirection * 3f, Color.red, 1f);

        animator.SetTrigger("Stomp");

        yield return new WaitForSeconds(1.5f);

        yield return StartCoroutine(Recover(2.0f));

        attackTimer = 1.5f;

        isAttacking = false;
        isSkillAttacking = false;
        agent.isStopped = false;
    }
    //양발 내려찍기
    IEnumerator DoubleStomp()
    {
        agent.updateRotation = false;

        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);

        animator.SetBool("Phase1Idle", currentPhase == BossPhase.Phase1);
        animator.SetBool("Phase2Idle", currentPhase == BossPhase.Phase2);

        if (_isDead) yield break;
        isAttacking = true;
        isSkillAttacking = true;
        doubleStompTimer = doubleStompCooldown;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();

        RotateToTarget();
        stompIndicator.SetActive(true);
        stompIndicator.transform.localPosition = Vector3.zero;
        stompIndicator.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        stompIndicator.transform.localScale = Vector3.one * attackRange * 2f;

        yield return new WaitForSeconds(0.8f);

        stompIndicator.SetActive(false);

        animator.SetTrigger("DoubleStomp");

        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(Recover(4.0f));

        attackTimer = 2.0f;
        isAttacking = false;
        isSkillAttacking = false;
        agent.isStopped = false;
    }
    //양발찍기 데미지
    void DealDoubleStompDamage()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            attackRange,
            targetLayer
        );

        foreach (var hit in hits)
        {
            Actor actor = hit.GetComponent<Actor>();

            if (actor == null || actor == this) continue;

            actor.TakeDamage(30, 1f);
        }
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

        agent.updateRotation = false; 

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();

        Vector3 dir = currentTarget.position - transform.position;
        dir.y = 0;

        attackDirection = dir.normalized;
        transform.rotation = Quaternion.LookRotation(attackDirection);

        animator.SetTrigger("Attack");
        attackTimer = attackCooldown;

        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(1.2f); 

        yield return StartCoroutine(Recover(3f));

        isAttacking = false;
    }


    //데미지 받기
    public override void TakeDamage(int damage, float severityOverride = -1f, bool isHeavyAttack = false, bool showDamageText = true)
    {
        if (isHit || _isDead) return;
        base.TakeDamage(damage, severityOverride);

        EndHit();
    }
  
    //돌진 스킬
    IEnumerator Charge()
    {
        agent.updateRotation = false;


        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);

        if (_isDead) yield break;

        isAttacking = true;
        isSkillAttacking = true;
        chargeTimer = chargeCooldown;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;  
        agent.ResetPath();

        Vector3 dir = (currentTarget.position - transform.position).normalized;
        dir.y = 0;

        transform.rotation = Quaternion.LookRotation(dir);

        animator.SetTrigger("ChargeReady");

        chargeIndicator.SetActive(true);

        Vector3 forwardOffset = dir * (chargeDistance * 0.5f);
        chargeIndicator.transform.position = transform.position + forwardOffset;
        chargeIndicator.transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(90f, 0, 0);
        chargeIndicator.transform.localScale = new Vector3(2f, chargeDistance, 1f);

        float t = 0;
        Renderer rend = chargeIndicator.GetComponentInChildren<Renderer>();
        Color baseColor = rend.material.color;

        while (t < 1.5f)
        {
            if (_isDead) yield break;
            t += Time.deltaTime;

            float alpha = Mathf.Lerp(0.2f, 0.7f, t / 1.5f);
            rend.material.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

            yield return null;
        }

        chargeIndicator.SetActive(false);
        PlayWolfDashSound();
        animator.SetTrigger("Charge");
        yield return new WaitForSeconds(0.1f);

        float moved = 0f;
        bool hasHit = false;

        while (moved < chargeDistance)
        {
            if (_isDead) yield break;
            float step = chargeSpeed * Time.deltaTime;
            transform.position += dir * step;
            moved += step;

            if (!hasHit)
            {
                Collider[] hits = Physics.OverlapSphere(
                   transform.position,
                   1.5f,
                   targetLayer
                   );

                foreach (var hit in hits)
                {
                    Actor actor = hit.GetComponent<Actor>();

                    if (actor == null || actor == this) continue;

                    actor.TakeDamage(20, 1f);
                    hasHit = true;
                    break;
                }
            }

            yield return null;
        }


        yield return StartCoroutine(Recover(2.0f));

        isAttacking = false;
        isSkillAttacking = false;
        agent.isStopped = false;
    }

    //공격 후딜 함수
    IEnumerator Recover(float time)
    {
        isRecovering = true;

        agent.updateRotation = false;
        agent.isStopped = true;

        yield return new WaitForSeconds(time);

        isRecovering = false;

        agent.updateRotation = true; 

        isAttacking = false;
        isSkillAttacking = false;
        agent.isStopped = false;
    }

    protected override bool IsRecovering()
    {
        return isRecovering || isSkillAttacking || isPhaseChanging;
    }

    //애니메이션 이벤트 데미지 함수
    public void OnDoubleStompHit()
    {
        DealDoubleStompDamage();
    }

    public void RotateToTargetSmooth()
    {
        if (currentTarget == null) return;
        if (isAttacking || isSkillAttacking || isRecovering) return;

        Vector3 dir = currentTarget.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        float dist = dir.magnitude;

        float speed = Mathf.Lerp(1f, 5f, dist / 10f);

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * speed
        );
    }

    public void PlayWolfAttackSound()
    {
        SoundManager.Instance.PlaySound("EliteWolfAttack");
    }

    public void PlayWolfAttack2Sound()
    {
        SoundManager.Instance.PlaySound("EliteWolfAttack2");
    }

    public void PlayWolfDieSound()
    {
        SoundManager.Instance.PlaySound("EliteWolfDie");
    }

    public void PlayWolfRoarSound()
    {
        SoundManager.Instance.PlaySound("EliteWolfRoar");
    }

    public void PlayWolfDashSound()
    {
        SoundManager.Instance.PlaySound("WolfDash");
    }
}
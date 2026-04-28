using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BossPhase;

public class TutorialBoss : Enemy
{
    [Header("페이즈 설정")]
    public BossPhase currentPhase = BossPhase.Phase1;
    public float phase2HpPercent = 0.6f;   //페이즈 변환 퍼센트

    [Header("튜토리얼 연출")]
    public float spawnDelay = 1f;

    bool introFinished = false;

    [Header("EnemyData")]
    public EnemyData bossData;

    [Header("스킬")]
    public float stompCooldown = 5f;
    public float doubleStompCooldown = 8f;
    public GameObject stompIndicatorPrefab;
    [Header("돌진 패턴")]
    public GameObject chargeIndicatorPrefab;
    public float chargeDistance = 10f;
    public float chargeSpeed = 15f;
    public float chargeLockTime = 1.2f;
    public float chargeStartDelay = 0.3f;
    public float chargeIndicatorBaseLength = 7f;
    public int chargeDamage = 25;
    public float chargeEndDelay = 1.2f;

    [Header("돌진할때 나오는 벽")]
    [SerializeField] GameObject chargeWallPrefab;
    [SerializeField] float chargeWallSideOffset = 3f;
    [SerializeField] float chargeWallForwardOffset = 2f;

    GameObject spawnedChargeWall;

    GameObject chargeIndicator;
    bool isCharging = false;

    float stompTimer = 0f;
    float doubleStompTimer = 0f;
    bool isStunned = false;
    bool isSkillAttacking = false;
    bool isPhaseChanging = false;
    GameObject stompIndicator;
    bool skillTutorialTriggered = false;

    //플레이어를 바라보고 있나?
    bool IsFacingTarget()
    {
        if (currentTarget == null) return false;

        Vector3 dir = (currentTarget.position - transform.position).normalized;

        float dot = Vector3.Dot(transform.forward, dir);

        return dot > 0.6f;
    }

    protected override void Start()
    {
        base.Start();

        if (data == null && bossData != null)
            Init(null, bossData);

        StartCoroutine(IntroRoutine());
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

    IEnumerator IntroRoutine()
    {
        agent.isStopped = true;
        isAttacking = true;

        yield return new WaitForSeconds(spawnDelay);

        TutorialSystem ts = FindFirstObjectByType<TutorialSystem>();

        ts?.ShowMission("보스를 물리치시오"); 

        animator.SetTrigger("PhaseRoar");

        yield return new WaitForSeconds(3f);

        agent.isStopped = false;
        isAttacking = false;

        introFinished = true;

        ts?.EndBossIntro();
    }

    //업데이트
    protected override void Update()
    {
        if (_isDead) return;
        if (!introFinished) return;
        if (isStunned) return;
        // 스킬 중이면 부모 이동 AI 실행 금지
        if (isAttacking || isSkillAttacking || isPhaseChanging || isCharging)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            UpdateIdleState();
            return;
        }



        base.Update();

        attackTimer -= Time.deltaTime;

        UpdatePhase();
        UpdateSkillCooldowns();

        TryAttack();

        UpdateIdleState();
    }

    //페이즈변환업데이트
    void UpdatePhase()
    {
        if (isPhaseChanging) return;
        if (currentPhase != BossPhase.Phase1) return;

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
        yield return new WaitForSeconds(3f);

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

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();

        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);

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
                isSkillAttacking = true;
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
        agent.isStopped = true;

        RotateToTarget();

        stompIndicator.SetActive(true);

        SkillTutorial skillTutorial = FindFirstObjectByType<SkillTutorial>();
        skillTutorial?.StartBossDodgeTutorial();

        stompIndicator.transform.localPosition = Vector3.zero;
        stompIndicator.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        stompIndicator.transform.localScale = Vector3.one * attackRange * 2f;

        yield return new WaitForSeconds(0.8f);

        animator.SetTrigger("DoubleStomp");
        doubleStompTimer = doubleStompCooldown;
        stompIndicator.SetActive(false);
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(TutorialCharge()); //바로 다음 돌진 패턴 나오게

        attackTimer = 2.0f;
        isAttacking = false;
        isSkillAttacking = false;
        agent.isStopped = false;


    }

    //튜토리얼 전용 돌진
    IEnumerator TutorialCharge()
    {
        if (_isDead || isPhaseChanging) yield break;

        isCharging = true;
        isAttacking = true;
        isSkillAttacking = true;

        agent.isStopped = true;
        agent.updateRotation = false;
        agent.velocity = Vector3.zero;
        agent.ResetPath();

        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);

        animator.SetTrigger("ChargeReady");

        chargeIndicator.SetActive(true);
        SpawnChargeSideWall();

        yield return StartCoroutine(StartChargeTutorialDelayed());

        float timer = 0f;

        while (timer < chargeLockTime)
        {
            if (_isDead || isPhaseChanging)
            {
                chargeIndicator.SetActive(false);
                isCharging = false;
                yield break;
            }

            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();

            timer += Time.deltaTime;

            if (currentTarget != null)
            {
                Vector3 dir = currentTarget.position - transform.position;
                dir.y = 0f;

                if (dir.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir);

                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRot,
                        Time.deltaTime * 6f
                    );

                    Vector3 forwardOffset = transform.forward * (chargeDistance * 0.5f);
                    chargeIndicator.transform.position = transform.position + forwardOffset;
                    chargeIndicator.transform.rotation = Quaternion.LookRotation(transform.forward);

                    float scaleZ = chargeDistance / chargeIndicatorBaseLength;
                    chargeIndicator.transform.localScale = new Vector3(2f, 1f, scaleZ);
                }
            }

            yield return null;
        }

        Vector3 finalDir = transform.forward;

        chargeIndicator.SetActive(false);

        yield return new WaitForSeconds(chargeStartDelay);

        animator.SetTrigger("Charge");

        yield return new WaitForSeconds(0.2f);

        float moved = 0f;
        bool hasHit = false;

        while (moved < chargeDistance)
        {
            float step = chargeSpeed * Time.deltaTime;

            transform.position += finalDir * step;
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
                    if (actor == null || actor == this || actor.IsDead) continue;

                    actor.TakeDamage(chargeDamage, 1f);
                    hasHit = true;
                    break;
                }
            }

            Collider[] envHits = Physics.OverlapSphere(
      transform.position,
      1.2f,
      LayerMask.GetMask("Environment")
  );

            if (envHits.Length > 0)
            {
                isCharging = false;
                isAttacking = false;
                isSkillAttacking = false;

                agent.isStopped = true;
                agent.velocity = Vector3.zero;

                StartCoroutine(TutorialStunRoutine(4f));
                yield break;
            }

            yield return null;
        }

        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        yield return new WaitForSeconds(chargeEndDelay);

        if (spawnedChargeWall != null)
        {
            Destroy(spawnedChargeWall);
        }

        isCharging = false;
        agent.updateRotation = true;
    }

    //튜토리얼 스턴 루틴
    IEnumerator TutorialStunRoutine(float duration)
    {
        isStunned = true;

        isAttacking = true;
        isSkillAttacking = false;
        isCharging = false;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.updateRotation = false;

        animator.SetBool("Stun", true);

        yield return new WaitForSeconds(duration);

        animator.SetBool("Stun", false);


        isStunned = false;

        isAttacking = false;
        isSkillAttacking = false;
        isCharging = false;

        agent.isStopped = false;
        agent.updateRotation = true;
    }

    //튜토리얼 돌진 딜레이
    IEnumerator StartChargeTutorialDelayed()
    {
        yield return new WaitForSeconds(0.9f); 

        SkillTutorial skillTutorial = FindFirstObjectByType<SkillTutorial>();

        bool tutorialDone = false;

        if (skillTutorial != null)
        {
            skillTutorial.StartChargeTutorial(() =>
            {
                tutorialDone = true;
            });

            yield return new WaitUntil(() => tutorialDone);
        }
    }

    //보스 양옆에 벽 소환
    void SpawnChargeSideWall()
    {
        if (chargeWallPrefab == null || currentTarget == null) return;

        // 기존 벽 있으면 제거
        if (spawnedChargeWall != null)
        {
            Destroy(spawnedChargeWall);
        }

        Vector3 toPlayer = currentTarget.position - transform.position;
        toPlayer.y = 0f;

        float sideDot = Vector3.Dot(toPlayer.normalized, transform.right);

        // 플레이어가 오른쪽에 있으면 왼쪽에 소환
        // 플레이어가 왼쪽에 있으면 오른쪽에 소환
        Vector3 sideDir = sideDot > 0f ? -transform.right : transform.right;

        Vector3 spawnPos =
            transform.position +
            transform.forward * chargeWallForwardOffset +
            sideDir * chargeWallSideOffset;

        Quaternion spawnRot = Quaternion.LookRotation(transform.forward) * chargeWallPrefab.transform.rotation;

        spawnedChargeWall = Instantiate(chargeWallPrefab, spawnPos, spawnRot);
    }

    //Idle 변환 스테이트
    void UpdateIdleState()
    {
        if (agent == null) return;
        if (isAttacking || isSkillAttacking || isPhaseChanging)
        {
            animator.SetBool("Walk", false);
            animator.SetBool("Run", false);
            return;
        }

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

        animator.SetTrigger("Attack"); 

        attackTimer = attackCooldown;
    }

    //데미지 받기
    public override void TakeDamage(int damage, float severityOverride = -1f, bool isHeavyAttack = false, bool showDamageText = true)
    {
        if (isHit || _isDead) return;

        base.TakeDamage(damage, severityOverride);

        if (!skillTutorialTriggered)
        {
            skillTutorialTriggered = true;

            SkillTutorial skillTutorial = FindFirstObjectByType<SkillTutorial>();
            skillTutorial?.OnFirstHitEnemy();
        }

        EndHit();
    }

    protected override void Die()
    {
        if (_isDead) return;

        base.Die();

        StartCoroutine(BossDieRoutine());
    }

    IEnumerator BossDieRoutine()
    {
        TutorialSystem ts = FindFirstObjectByType<TutorialSystem>();

        if (ts != null)
            yield return ts.StartCoroutine(ts.FadeOutMission());

        yield return new WaitForSeconds(1f);

        ts?.ShowTutorialComplete();
    }

    public void HideStompIndicator()
    {
        if (stompIndicator != null)
            stompIndicator.SetActive(false);
    }

    public void OnDoubleStompHit()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            attackRange,
            targetLayer
        );

        foreach (var hit in hits)
        {
            Actor actor = hit.GetComponent<Actor>();
            if (actor == null || actor == this || actor.IsDead) continue;

            actor.TakeDamage(attackDamage, 1f);
        }
    }

    public void SetTutorialFreeze(bool freeze)
    {
        agent.isStopped = freeze;

        if (freeze)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;

            isAttacking = true;
            isSkillAttacking = true;
            isCharging = false;

            animator.SetBool("Walk", false);
            animator.SetBool("Run", false);

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
            isAttacking = false;
            isSkillAttacking = false;
            agent.isStopped = false;
        }
    }
}


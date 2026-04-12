using UnityEngine;
using static BossPhase;
using System.Collections;
using System.Collections.Generic;

public class WolfBoss : Enemy
{
    [Header("멍때리는 시간")]
    [Tooltip("내려찍기 멍때리기")]
    [SerializeField] float stompDelay = 3f;
    [Tooltip("점프공격 멍때리기")]
    [SerializeField] float jump2Delay = 12f;
    [Tooltip("일반공격 멍때리기")]
    [SerializeField] float normalDelay = 2f;
    [Tooltip("할퀴기 멍때리기")]
    [SerializeField] float slashDelay = 2f;
    [Tooltip("돌진 멍때리기")]
    [SerializeField] float chargeDelay = 2f;
    [Tooltip("암흑탄 공격 멍때리기")]
    [SerializeField] float darkDelay = 2f;
    [Tooltip("스핀 멍때리기")]
    [SerializeField] float spinDelay = 2f;
    [Tooltip("포효후 멍때리기")]
    [SerializeField] float RoarDelay = 2f;

    [Header("부위파괴")]
    [Header("오른팔 부위파괴 설정")]
    [SerializeField] GameObject RightHandPointCollider;
    [SerializeField] int RightHandPointHP = 100;
    [SerializeField] float breakDownTime = 5f;   // 눕고 있는 시간
    [SerializeField] float breakAnimTime = 1.5f; // 넘어지는 시간
    [SerializeField] float getUpTime = 2f;       // 일어나는 시간
    [Header("부위파괴 할퀴기 디버프")]
    [SerializeField] float brokenHandAnimSpeed = 0.6f;   
    [SerializeField] float brokenHandAttackSpeedMultiplier = 0.6f;


    [Header("보스 페이즈")]
    public BossPhase currentPhase = BossPhase.Phase1;
    [Tooltip("페이즈2변환 hp퍼센트")]
    public float phase2HpPercent = 0.6f;

    [Header("1페이지 할퀴기 스킬")]
    [Tooltip("할퀴기 앞쪽 범위")]
    public float slashRange = 4f;
    [Tooltip("할퀴기 양옆 범위")]
    public float slashAngle = 120f;
    [Tooltip("할퀴기 데미지")]
    public int slashDamage = 20;
    [Tooltip("할퀴기 대기시간")]
    public float slashCooldown = 3f;
    public GameObject slashIndicatorPrefab;
    public Transform clawSpawnPoint; // 손 위치
    [SerializeField] float slashBaseAngle = 90f;

    [Header("1페이지 점프 공격")]
    [Tooltip("점프공격범위")]
    public float jumpAttackRange = 5f;
    [Tooltip("점프공격데미지")]
    public int jumpAttackDamage = 30;
    [Tooltip("점프 하고나서 기다리는 시간")]
    public float jumpDelay = 2.5f;
    [Tooltip("점프공격 대기시간")]
    public float jumpCooldown = 8f;
    [Tooltip("점프공격 이펙트 보정값")]
    [SerializeField] float jumpIndicatorScaleMultiplier = 0.7f;
    public GameObject jumpIndicatorPrefab;
    [SerializeField] GameObject modelRoot;


    [Header("1페이지 돌진 스킬")]
    [Tooltip("돌진대기시간")]
    public float chargeCooldown = 10f;
    [Tooltip("돌진거리")]
    public float chargeDistance = 10f;
    [Tooltip("돌진속도")]
    public float chargeSpeed = 20f;
    [Header("돌진 전 딜레이(후)")]
    [SerializeField] float chargeStartDelay = 0.5f;
    [Tooltip("돌진하기전기다리는시간")]
    public float chargeLockTime = 2f;
    [SerializeField] float chargeIndicatorBaseLength = 7f;
    public GameObject chargeIndicatorPrefab;

    [Header("Phase2 휘두르기")]
    [Tooltip("휘두르기범위")]
    public float spinAttackRange = 5f;
    [Tooltip("휘두르기데미지")]
    public int spinAttackDamage = 35;
    [Tooltip("휘두르기 대기시간")]
    public float spinCooldown = 6f;
    public GameObject spinIndicatorPrefab;

    [Header("Phase2 암흑탄")]
    public GameObject darkProjectilePrefab;
    public Transform firePoint;
    [Tooltip("암흑탄속도")]
    public float projectileSpeed = 10f;
    [Tooltip("암흑탄퍼지는각도")]
    public float spreadAngle = 50f;
    [Tooltip("암흑탄 살아있는 시간")]
    public float projectileLifeTime = 3f;
    [Tooltip("암흑탄 대기시간")]
    public float darkShotCooldown = 5f;
    public Transform headTransform;

    [Header("Phase2 내려찍기")]
    [Tooltip("내려찍기 범위")]
    public float stompRange = 6f;
    [Tooltip("내려찍기 데미지")]
    public int stompDamage = 50;
    [Tooltip("내려찍기 대기시간")]
    public float stompCooldown = 8f;
    [Tooltip("내려찍기 위험표시시간")]
    public float stompWarningTime = 2.5f;
    public GameObject stompIndicatorPrefab;
    [Header("내려찍기 장판 연출")]
    [SerializeField] AnimationCurve stompFillCurve;
    [SerializeField] float stompGrowTime = 2.5f;

    [Header("Vfx")]
    public GameObject roarVFXPrefab;
    public GameObject clawVFXPrefab;
    public GameObject biteVFXPrefab;
    public GameObject spinVFXPrefab;
    public GameObject jumpVFXPrefab;

    //변수들
    float jumpTimer = 0f;
    GameObject jumpIndicator;
    float slashTimer = 0f;
    GameObject slashIndicator;
    bool hasStartedCombat = false;
    bool isInvincible = false;
    float chargeTimer = 0f;
    bool isCharging = false;
    GameObject chargeIndicator;
    public float stunDuration = 5f;
    bool isStuned = false;
    float stompTimer = 0f;
    GameObject stompIndicator;
    float darkShotTimer = 0f;
    float spinTimer = 0f;
    GameObject spinIndicator;
    int comboIndex = 0;
    bool isComboAttacking = false;
    bool isPhaseChanging = false;
    Vector3 jumpTargetPos;
    bool isLocked = false;
    bool isRightHandBroken = false;

    protected override void Awake()
    {
        base.Awake();

        slashIndicator = Instantiate(slashIndicatorPrefab, transform);
        slashIndicator.SetActive(false);

        jumpIndicator = Instantiate(jumpIndicatorPrefab);
        jumpIndicator.SetActive(false);

        chargeIndicator = Instantiate(chargeIndicatorPrefab, transform);
        chargeIndicator.SetActive(false);

        spinIndicator = Instantiate(spinIndicatorPrefab, transform);
        spinIndicator.SetActive(false);

        stompIndicator = Instantiate(stompIndicatorPrefab, transform);
        stompIndicator.SetActive(false);
    }

    protected override void Update()
    {
        attackTimer -= Time.deltaTime;
        slashTimer -= Time.deltaTime;
        jumpTimer -= Time.deltaTime;
        chargeTimer -= Time.deltaTime;
        spinTimer -= Time.deltaTime;
        darkShotTimer -= Time.deltaTime;
        stompTimer -= Time.deltaTime;

        if (_isDead) return;

        UpdatePhase();

        if (attackTimer > 0f || isPhaseChanging || isComboAttacking)
        {
            agent.isStopped = true;

            animator.SetBool("Run_P1", false);
            animator.SetBool("Run_P2", false);

            return;
        }

        if (isStuned)
        {
            agent.isStopped = true;
            return;
        }

        base.Update();
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

    //페이즈 업데이트
    void UpdatePhase()
    {
        if (isPhaseChanging) return;

        float hpPercent = (float)_currentHP / _maxHP;

        if (currentPhase == BossPhase.Phase1
            && hpPercent <= phase2HpPercent
            && !isAttacking
            && !isComboAttacking)
        {
            StartCoroutine(ChangeToPhase2());
        }
    }

    //페이즈2변환
    IEnumerator ChangeToPhase2()
    {
        isPhaseChanging = true;
        isAttacking = true;
        agent.isStopped = true;

        slashIndicator.SetActive(false);
        jumpIndicator.SetActive(false);
        chargeIndicator.SetActive(false);
        spinIndicator.SetActive(false);

        animator.ResetTrigger("AttackReady_P1");
        animator.ResetTrigger("Attack1_P1");
        animator.ResetTrigger("Attack2_P1");
        animator.ResetTrigger("Attack3_P1");
        animator.SetTrigger("PhaseRoar");

        yield return StartCoroutine(IdleDelayRoutine(RoarDelay));

        currentPhase = BossPhase.Phase2;

        slashTimer = 0f;
        jumpTimer = 0f;


        attackTimer = 0f;

        isAttacking = false;
        isPhaseChanging = false;
        isComboAttacking = false; 

        agent.isStopped = false;
    }

    //공격시도 (스킬 포함)
    protected override void TryAttack()
    {
        if (isPhaseChanging || isComboAttacking || isStuned) return;
        if (currentTarget == null) return;

        if (!hasStartedCombat)
        {
            hasStartedCombat = true;

            animator.SetTrigger("FirstRoar");

            attackTimer = 2f;
            return;
        }

        float dist = Vector3.Distance(transform.position, currentTarget.position);
        if (currentPhase == BossPhase.Phase1)
        {
            List<System.Action> patterns = new List<System.Action>();

            if (jumpTimer <= 0f)
                patterns.Add(() => StartCoroutine(JumpAttack()));

            if (slashTimer <= 0f && dist <= slashRange)
                patterns.Add(() => StartCoroutine(Slash()));

            if (chargeTimer <= 0f)
                patterns.Add(() => StartCoroutine(Charge()));

            patterns.Add(() => base.TryAttack());

            if (patterns.Count == 0) return;

            int index = Random.Range(0, patterns.Count);
            patterns[index].Invoke();

            return;
        }

        if (currentPhase == BossPhase.Phase2)
        {
            List<System.Action> patterns = new List<System.Action>();

            if (spinTimer <= 0f)
                patterns.Add(() => StartCoroutine(SpinAttack()));

            if (darkShotTimer <= 0f)
                patterns.Add(() => StartCoroutine(DarkShot()));

            patterns.Add(() => base.TryAttack());

            if (stompTimer <= 0f)
                patterns.Add(() => StartCoroutine(StompAttack()));

            int index = Random.Range(0, patterns.Count);
            patterns[index].Invoke();
            return;
        }

        base.TryAttack();
    }

    //공격
    protected override void Attack()
    {
        if (isComboAttacking) return;

        StartCoroutine(ComboAttack());
    }
    //기본 콤보
    IEnumerator ComboAttack()
    {
        if (isPhaseChanging) yield break;

        isAttacking = true;
        isComboAttacking = true;

        agent.isStopped = true;
        agent.updateRotation = false;

        agent.velocity = Vector3.zero;
        agent.ResetPath();

        Vector3 dir = currentTarget.position - transform.position;
        dir.y = 0;

        float rotateTime = 0.3f;
        float t = 0f;

        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.LookRotation(dir);

        while (t < rotateTime)
        {
            t += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t / rotateTime);
            yield return null;
        }

        attackDirection = dir.normalized;

        if (currentPhase == BossPhase.Phase1)
            animator.SetTrigger("AttackReady_P1");
        else
            animator.SetTrigger("AttackReady_P2");

        yield return new WaitForSeconds(1.5f);

        int rand = Random.Range(1, 4);

        if (currentPhase == BossPhase.Phase1)
            animator.SetTrigger($"Attack{rand}_P1");
        else
            animator.SetTrigger($"Attack{rand}_P2");

        yield return StartCoroutine(IdleDelayRoutine(normalDelay));

        agent.updateRotation = true;
        agent.isStopped = false;

        isComboAttacking = false;
        EndAttack();

        attackTimer = attackCooldown;
    }
    //오른쪽 할퀴기
    IEnumerator Slash()
    {
        if (isPhaseChanging) yield break;

        isAttacking = true;
        isComboAttacking = true;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.updateRotation = false;

        float speedMul = isRightHandBroken ? brokenHandAttackSpeedMultiplier : 1f;
        float animSpeed = isRightHandBroken ? brokenHandAnimSpeed : 1f;

        animator.speed = animSpeed;

        float rotateTime = 0.5f / speedMul;
        float timer = 0f;

        while (timer < rotateTime)
        {
            if (isPhaseChanging)
            {
                ResetAnimSpeed();
                EndAttack();
                yield break;
            }

            timer += Time.deltaTime;

            if (currentTarget != null)
            {
                Vector3 dir = currentTarget.position - transform.position;
                dir.y = 0;

                if (dir.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRot,
                        Time.deltaTime * 5f
                    );

                    attackDirection = dir.normalized;
                }
            }

            yield return null;
        }

        if (currentTarget != null)
        {
            Vector3 dir = currentTarget.position - transform.position;
            dir.y = 0;
            attackDirection = dir.normalized;
        }

        ShowSlashIndicator();

        yield return new WaitForSeconds(1.5f / speedMul);

        slashIndicator.SetActive(false);

        animator.SetTrigger("Slash");

        yield return new WaitForSeconds(0.3f / speedMul);

        DealSlashDamage();

        yield return new WaitForSeconds(slashDelay / speedMul);

        slashTimer = slashCooldown;

        isComboAttacking = false;

        ResetAnimSpeed(); 

        EndAttack();

        agent.isStopped = false;
        agent.updateRotation = true;
    }
    //할퀴기 범위장판
    void ShowSlashIndicator()
    {
        slashIndicator.SetActive(true);

        var ps = slashIndicator.GetComponent<ParticleSystem>();

        var main = ps.main;
        main.startSize = 8f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = slashAngle * 0.5f;
        shape.radius = 0f;
        shape.length = slashRange;

        float startOffset = 4.5f; 

        slashIndicator.transform.position =
            transform.position + attackDirection * startOffset;

        slashIndicator.transform.rotation =
            Quaternion.LookRotation(attackDirection);

        ps.Play();
    }
    //할퀴기 데미지
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
    //점프 어택
    IEnumerator JumpAttack()
    {
        if (isPhaseChanging) yield break;

        isLocked = false;
        isAttacking = true;
        isComboAttacking = true;
        isInvincible = true;

        agent.isStopped = true;
        agent.updateRotation = false;

        animator.SetTrigger("Jump");

        yield return new WaitForSeconds(0.3f);

        ShowJumpIndicator();

        float followSpeed = 2.0f;
        float keepDistance = 2.5f;

        float upTime = 0.6f;          
        float airTime = jumpDelay;  
        float downTime = 1f;        
        float jumpHeight = 7f;

        Vector3 startPos = transform.position;
        float timer = 0f;

        while (timer < upTime)
        {
            if (isPhaseChanging || _isDead)
            {
                EndAttack();
                yield break;
            }

            timer += Time.deltaTime;

            float t = timer / upTime;
            t = Mathf.SmoothStep(0, 1, t);

            Vector3 pos = transform.position;

            if (currentTarget != null)
            {
                Vector3 targetPos = currentTarget.position;

                Vector3 dir = targetPos - pos;
                dir.y = 0;

                if (dir.magnitude > keepDistance)
                    pos += dir.normalized * followSpeed * Time.deltaTime;

                if (dir.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.LookRotation(dir);

                jumpTargetPos = new Vector3(targetPos.x, 0.05f, targetPos.z);
            }

            pos.y = Mathf.Lerp(startPos.y, startPos.y + jumpHeight, t);
            transform.position = pos;

            jumpIndicator.transform.position = jumpTargetPos;

            yield return null;
        }

        HideModel();
        animator.speed = 0f;

        timer = 0f;

        while (timer < airTime)
        {
            if (isPhaseChanging || _isDead)
            {
                EndAttack();
                yield break;
            }

            timer += Time.deltaTime;

            float t = timer / airTime;

            Vector3 pos = transform.position;

            if (currentTarget != null && !isLocked)
            {
                Vector3 targetPos = currentTarget.position;

                Vector3 dir = targetPos - pos;
                dir.y = 0;

                if (dir.magnitude > keepDistance)
                    pos += dir.normalized * followSpeed * Time.deltaTime;

                jumpTargetPos = new Vector3(targetPos.x, 0.05f, targetPos.z);
            }

            if (!isLocked && t >= 0.8f)
            {
                isLocked = true;
            }

            transform.position = pos;
            jumpIndicator.transform.position = jumpTargetPos;

            yield return null;
        }

        transform.position = new Vector3(
            jumpTargetPos.x,
            transform.position.y,
            jumpTargetPos.z
        );

        ShowModel();
        animator.speed = 1f;
        timer = 0f;

        float startY = startPos.y + jumpHeight;

        while (timer < downTime)
        {
            if (isPhaseChanging || _isDead)
            {
                EndAttack();
                yield break;
            }

            timer += Time.deltaTime;

            float t = timer / downTime;

            float curve = t * t;

            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(startY, startPos.y, curve);

            transform.position = pos;

            jumpIndicator.transform.position = jumpTargetPos;

            yield return null;
        }

        jumpIndicator.SetActive(false);

        yield return new WaitForSeconds(0.05f);

        OnJumpImpact();

        isInvincible = false;

        yield return new WaitForSeconds(jump2Delay);

        jumpTimer = jumpCooldown;

        isComboAttacking = false;
        EndAttack();

        attackTimer = attackCooldown;

        agent.isStopped = false;
        agent.updateRotation = true;
    }
    //점프어택 장판
    void ShowJumpIndicator()
    {
        jumpIndicator.SetActive(true);

        float diameter = jumpAttackRange * 2f * jumpIndicatorScaleMultiplier;

        jumpIndicator.transform.localScale = Vector3.one * diameter;

        if (currentTarget != null)
        {
            jumpTargetPos = new Vector3(
                currentTarget.position.x,
                0.05f,
                currentTarget.position.z
            );

            jumpIndicator.transform.position = jumpTargetPos;
        }

        jumpIndicator.transform.rotation = Quaternion.identity;
    }
    public void HideModel()
    { modelRoot.SetActive(false); }
    public void ShowModel()
    { modelRoot.SetActive(true); }
    //돌진 
    IEnumerator Charge()
    {
        if (isPhaseChanging) yield break;
        isAttacking = true;
        isComboAttacking = true;
        isCharging = true;

        agent.isStopped = true;
        agent.updateRotation = false;
        agent.velocity = Vector3.zero;
        agent.ResetPath();

        animator.SetTrigger("ChargeReady");

        chargeIndicator.SetActive(true);

        float timer = 0f;

        while (timer < chargeLockTime)
        {
            if (isPhaseChanging || _isDead)
            {
                chargeIndicator.SetActive(false);
                EndAttack();
                yield break;
            }

            timer += Time.deltaTime;

            if (currentTarget != null)
            {
                Vector3 dir = currentTarget.position - transform.position;
                dir.y = 0;

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

                    chargeIndicator.transform.rotation =
                        Quaternion.LookRotation(transform.forward) *
                        Quaternion.Euler(0f, 0f, 0f);

                    float scaleZ = chargeDistance / chargeIndicatorBaseLength;

                    chargeIndicator.transform.localScale =
                        new Vector3(2f, 1f, scaleZ);
                }
            }

            yield return null;
        }
        Vector3 finalDir = transform.forward;
        attackDirection = finalDir;

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
                    if (actor == null || actor == this) continue;

                    actor.TakeDamage(40, 1f);
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
                isComboAttacking = false;

                EndAttack(); 

                StartCoroutine(StunRoutine());
                yield break;
            }

            yield return null;
        }

        yield return new WaitForSeconds(chargeDelay);

        chargeTimer = chargeCooldown;

        isCharging = false;
        isComboAttacking = false;
        EndAttack();

        agent.isStopped = false;
        agent.updateRotation = true;
    }
    //스턴
    IEnumerator StunRoutine()
    {
        isStuned = true;
        isAttacking = true;
        isComboAttacking = false;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.updateRotation = false;

        animator.SetBool("Stun", true);

        RightHandPointCollider.SetActive(true);

        WeakPoint wp = RightHandPointCollider.GetComponent<WeakPoint>();
        wp.Init(RightHandPointHP, this);

        yield return new WaitForSeconds(stunDuration);

        RightHandPointCollider.SetActive(false);

        animator.SetBool("Stun", false);

        isStuned = false;
        isAttacking = false;
        isComboAttacking = false;

        EndAttack();

        agent.isStopped = false;
        agent.updateRotation = true;

        attackTimer = 2f;
    }

    //스턴 후 부위파괴
    public void OnWeakPointBreak()
    {
        if (isRightHandBroken || _isDead) return;

        isRightHandBroken = true; 

        animator.SetBool("Stun", false);

        StartCoroutine(BreakRoutine());
    }

    //스턴 후 부위파괴
    IEnumerator BreakRoutine()
    {
        isStuned = false;
        isAttacking = true;
        isComboAttacking = false;

        RightHandPointCollider.SetActive(false);

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.updateRotation = false;

        animator.speed = 0.23f;
        animator.SetTrigger("Break");

        yield return new WaitForSecondsRealtime(breakAnimTime);

        animator.speed = 0f;

        yield return new WaitForSecondsRealtime(breakDownTime);

        animator.speed = 0.5f;
        animator.SetTrigger("GetUp");

        yield return new WaitForSecondsRealtime(getUpTime);

        animator.speed = 1f;
        isAttacking = false;

        agent.isStopped = false;
        agent.updateRotation = true;

        attackTimer = 2f;
    }

    //점프 데미지 주기
    void DealJumpDamage()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            jumpAttackRange,
            targetLayer
        );

        foreach (var hit in hits)
        {
            Actor actor = hit.GetComponent<Actor>();

            if (actor == null || actor == this) continue;

            actor.TakeDamage(jumpAttackDamage, 1f);
        }
    }

    public void OnJumpImpact()
    {
        if (jumpIndicator != null)
            jumpIndicator.SetActive(false);

        DealJumpDamage();
    }

    //휘두르기
    IEnumerator SpinAttack()
    {

        if (isPhaseChanging) yield break;
        isAttacking = true;
        isComboAttacking = true;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.updateRotation = false;

        ShowSpinIndicator();

        yield return new WaitForSeconds(1.5f); 

        spinIndicator.SetActive(false);

        animator.SetTrigger("Spin"); 

        yield return new WaitForSeconds(1f);
        yield return new WaitForSeconds(spinDelay);

        spinTimer = spinCooldown;

        isComboAttacking = false;
        EndAttack();

        agent.isStopped = false;
        agent.updateRotation = true;
    }
    //휘두르기 보여주기
    void ShowSpinIndicator()
    {
        spinIndicator.SetActive(true);

        float diameter = spinAttackRange * 2f;

        spinIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);

        Vector3 pos = transform.position;
        pos.y += 0.05f;

        spinIndicator.transform.position = pos;

        spinIndicator.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
    //휘두르기 데미지 주기
   public  void DealSpinDamage()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            spinAttackRange,
            targetLayer
        );

        foreach (var hit in hits)
        {
            Actor actor = hit.GetComponent<Actor>();
            if (actor == null || actor == this) continue;

            actor.TakeDamage(spinAttackDamage, 1f);
        }
    }

    //암흑 공 샷
    IEnumerator DarkShot()
    {
        if (isPhaseChanging) yield break;

        isAttacking = true;
        isComboAttacking = true;

        agent.isStopped = true;
        agent.updateRotation = false;

        Vector3 dir = currentTarget.position - transform.position;
        dir.y = 0;
        dir.Normalize();

        float rotateTime = 0.4f; 
        float t = 0f;

        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.LookRotation(dir);

        while (t < rotateTime)
        {
            t += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t / rotateTime);
            yield return null;
        }
        attackDirection = dir;
        animator.SetTrigger("DarkShot");

        yield return new WaitForSeconds(1f);
        yield return new WaitForSeconds(darkDelay);

        darkShotTimer = darkShotCooldown;

        isComboAttacking = false;
        EndAttack();

        agent.isStopped = false;
        agent.updateRotation = true;
    }
    //암흑 공 샷
    public void FireDarkShot()
    {
        FireSpreadProjectiles(attackDirection);
    }
    //암흑 공 샷
    void FireSpreadProjectiles(Vector3 forward)
    {
        float halfAngle = spreadAngle * 0.5f;

        SpawnProjectile(forward);

        Vector3 leftDir = Quaternion.Euler(0, -halfAngle, 0) * forward;
        SpawnProjectile(leftDir);

        Vector3 rightDir = Quaternion.Euler(0, halfAngle, 0) * forward;
        SpawnProjectile(rightDir);
    }
    //암흑 공 샷
    void SpawnProjectile(Vector3 dir)
    {
        GameObject proj = Instantiate(darkProjectilePrefab, firePoint.position, Quaternion.LookRotation(dir));

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = dir * projectileSpeed;
        }

        Destroy(proj, projectileLifeTime);
    }

    //내려찍기 공격
    IEnumerator StompAttack()
    {
        if (isPhaseChanging) yield break;

        isAttacking = true;
        isComboAttacking = true;

        agent.isStopped = true;
        agent.updateRotation = false;

        Vector3 dir = currentTarget.position - transform.position;
        dir.y = 0;

        float rotateTime = 0.3f;
        float t = 0f;

        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.LookRotation(dir);

        while (t < rotateTime)
        {
            t += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t / rotateTime);
            yield return null;
        }

        attackDirection = dir.normalized;

        yield return new WaitForSeconds(stompWarningTime);

        animator.speed = 0.4f; 

        animator.SetTrigger("Stomp");

        yield return new WaitForSeconds(stompDelay);

        stompTimer = stompCooldown;

        isComboAttacking = false;

        attackTimer = attackCooldown; 

        EndAttack();

        agent.isStopped = false;
        agent.updateRotation = true;

        animator.speed = 1f; 
    }

    //내려찍기 장판
    void ShowStompIndicator()
    {
        stompIndicator.SetActive(true);

        stompIndicator.transform.position = transform.position + Vector3.up * 0.05f;
        stompIndicator.transform.rotation = Quaternion.identity;

        StartCoroutine(FillStompVFX(stompIndicator));
    }

    //내려찍기 입팩트
    public void OnStompImpact()
    {
        if (stompIndicator != null)
            stompIndicator.SetActive(false);

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            stompRange,
            targetLayer
        );

        foreach (var hit in hits)
        {
            Actor actor = hit.GetComponent<Actor>();
            if (actor == null || actor == this) continue;

            actor.TakeDamage(stompDamage, 1f);
        }

        animator.speed = 1f; 
    }

    public void OnStompReady()
    {
        ShowStompIndicator();

        animator.speed = 0f;

        StartCoroutine(StompResumeRoutine());
    }

    IEnumerator StompResumeRoutine()
    {
        yield return new WaitForSeconds(2.5f);

        animator.speed = 0.25f;
    }

    IEnumerator FillStompVFX(GameObject vfx)
    {
        float timer = 0f;

        float maxScale = stompRange * 2f;

        ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
        Renderer rend = vfx.GetComponentInChildren<Renderer>();

        Color start = new Color(1, 1, 1, 0.2f);
        Color mid = new Color(1, 0.5f, 0, 0.6f);
        Color end = new Color(1, 0, 0, 1f);

        while (timer < stompGrowTime)
        {
            timer += Time.deltaTime;
            float t = timer / stompGrowTime;

            float curved = stompFillCurve.Evaluate(t);

            float size = Mathf.Lerp(0.1f, maxScale, curved);
            vfx.transform.localScale = new Vector3(size, size, size);

            if (rend != null)
            {
                Color c;
                if (t < 0.5f)
                    c = Color.Lerp(start, mid, t * 2f);
                else
                    c = Color.Lerp(mid, end, (t - 0.5f) * 2f);

                rend.material.color = c;
            }

            yield return null;
        }

        StartCoroutine(FlashVFX(vfx));
    }
    IEnumerator FlashVFX(GameObject vfx)
    {
        float timer = 0f;

        Renderer rend = vfx.GetComponentInChildren<Renderer>();

        while (timer < 0.5f)
        {
            timer += Time.deltaTime;

            float alpha = Mathf.PingPong(timer * 10f, 1f);

            if (rend != null)
            {
                Color c = rend.material.color;
                c.a = Mathf.Lerp(0.3f, 1f, alpha);
                rend.material.color = c;
            }

            yield return null;
        }
    }

    public override void TakeDamage(int damage, float severityOverride = -1f, bool isHeavyAttack = false)
    {
        if (isInvincible || isPhaseChanging) return; 

        if (isHit || _isDead) return;
        base.TakeDamage(damage, severityOverride);

        EndHit();
    }



    //----------------멍떄리는 코드
    IEnumerator IdleDelayRoutine(float delay)
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        yield return new WaitForSeconds(delay);
    }

    //vfx , 애니메이션들

    //할퀴기
    public void SpawnClawVFX()
    {
        if (clawVFXPrefab == null) return;

        Vector3 spawnPos = clawSpawnPoint != null
            ? clawSpawnPoint.position
            : transform.position;

        Quaternion rot = Quaternion.LookRotation(attackDirection);
        rot *= Quaternion.AngleAxis(180f, Vector3.forward);

        GameObject vfx = Instantiate(clawVFXPrefab, spawnPos, rot);

        vfx.transform.localScale = Vector3.one * 3.5f;

        float vfxSpeed = isRightHandBroken ? brokenHandAnimSpeed : 1f;

        ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.simulationSpeed = vfxSpeed; 

            ps.Play();
        }

        Destroy(vfx, 1.5f / vfxSpeed);
    }

    //포효
    public void SpawnRoarVFX()
    {
        GameObject vfx = Instantiate(roarVFXPrefab, transform.position, Quaternion.identity);

        vfx.transform.localScale = Vector3.one * 4f; 

        Destroy(vfx, 2f);
    }

    //물기
    public void SpawnBiteVFX()
    {
        if (biteVFXPrefab == null) return;

        Vector3 spawnPos = headTransform != null
            ? headTransform.position
            : transform.position + transform.forward * 1.0f + Vector3.up * 1.5f;

        Quaternion rot = Quaternion.LookRotation(attackDirection);

        GameObject vfx = Instantiate(biteVFXPrefab, spawnPos, rot);

        Destroy(vfx, 2f);
    }

    //돌기
    public void SpawnSpinVFX()
    {
        if (spinVFXPrefab == null) return;

        Vector3 pos = transform.position;
        pos.y += 0.1f;

        GameObject vfx = Instantiate(spinVFXPrefab, pos, Quaternion.identity);

        vfx.transform.localScale = Vector3.one * 1.8f; 

        Destroy(vfx, 2f);
    }

    public void SpawnJumpVFX()
    {
        if (jumpVFXPrefab == null) return;

        Vector3 pos = transform.position;
        pos.y += 0.1f;

        GameObject vfx = Instantiate(jumpVFXPrefab, pos, Quaternion.identity);

        vfx.transform.localScale = Vector3.one * 7f;

        Destroy(vfx, 2f);
    }

    void ResetAnimSpeed()
    {
        animator.speed = 1f;
    }
}

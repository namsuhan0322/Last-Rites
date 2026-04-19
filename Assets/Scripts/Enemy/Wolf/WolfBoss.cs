using Project.Scripts.Fractures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BossPhase;

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
    [Tooltip("토네이도 멍때리기")]
    [SerializeField] float tornadoDelay = 2f;
    [Tooltip("똥장판 멍때리기")]
    [SerializeField] float explosionDelay = 2f;
    [Tooltip("던지기 멍때리기")]
    [SerializeField] float throwDelay = 2f;


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
    [Tooltip("할퀴기 위험표시 앵글")]
    public float slashBaseAngle = 90f;

    [Header("1페이지 불 독 던지기")]
    public GameObject fireballPrefab;
    public GameObject poisonBallPrefab;
    public Transform throwPoint;
    public int fireDamage = 20;
    public int poisonDamage = 15;
    [Tooltip("불독 장판 크기")]
    public float throwRadius = 6f;
    public float throwCooldown = 8f;
    [Tooltip("불독 터지기 대기시간")]
    public float throwWarningTime = 1.5f;
    public GameObject throwIndicatorPrefab;
    public float poisonOuterRadius = 12f;



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

    [Header("1페이지 회오리 스킬")]
    [SerializeField] float tornadoSpeed = 3f;
    [Tooltip("토네이도 시간")]
    [SerializeField] float tornadoDuration = 5f;
    [SerializeField] int tornadoDamage = 20;
    [SerializeField] float tornadoCooldown = 10f;
    [Tooltip("토네이도 당기는 범위")]
    [SerializeField] float tornadoPullRadius = 8f;
    [Tooltip("토네이도 당기는 힘")]
    [SerializeField] float tornadoPullForce = 15f;
    [Tooltip("토네이도 당기는 최소거리")]
    [SerializeField] float tornadoMinDistance = 1.5f;


    [Header("2페이지 휘두르기")]
    [Tooltip("휘두르기범위")]
    public float spinAttackRange = 5f;
    [Tooltip("휘두르기데미지")]
    public int spinAttackDamage = 35;
    [Tooltip("휘두르기 대기시간")]
    public float spinCooldown = 6f;
    public GameObject spinIndicatorPrefab;

    [Header("2페이지 암흑탄")]
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
    [Tooltip("암흑탄 사용 거리")]
    public float darkShotMinDistance = 5f;
    public Transform headTransform;

    [Header("2페이지 내려찍기")]
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
    public AnimationCurve stompFillCurve;
    public float stompGrowTime = 2.5f;

    [Header("2페이지 원형똥 할퀴기")]
    public int explosionDamage = 15;
    public float explosionRadius = 8f;
    public GameObject circleIndicatorPrefab;
    public float slamExplosionCooldown = 8f;
    public float slamExplosionTimer = 0f;
    public float minExplodeDelay = 1.2f;
    public float maxExplodeDelay = 2.0f;
    public float indicatorBaseSize = 1f;
    public Transform leftHand;
    public Transform rightHand;

    [Header("2페이지 세르카 버전 삼각형")]
    public GameObject trianglePrefab;
    public Transform trileftHand;
    public Transform trirightHand;
    public int triangleCountPerHand = 3;
    public float triangleLength = 8f;
    public  float triangleWidth = 2f;
    public  float triCooldown = 20f;
    public  LayerMask groundLayer;

    [Header("능지패턴")]
    [SerializeField] float lineCooldown = 10f;
    [SerializeField] float lineWarningTime = 1f;
    [SerializeField] float lineSpacing = 3f;
    [SerializeField] float lineDelay = 0.5f;
    [SerializeField] GameObject lineIndicatorPrefab;

    [Header("Vfx")]
    public GameObject roarVFXPrefab;
    public GameObject clawVFXPrefab;
    public GameObject biteVFXPrefab;
    public GameObject spinVFXPrefab;
    public GameObject jumpVFXPrefab;
    public GameObject tornadoVFX;
    public GameObject sandstormVFXPrefab;
    public GameObject explosionVFXPrefab;
    public GameObject clawDdongVFXPrefab;
    public GameObject fireVFX;
    public GameObject poisonVFX;
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
    Collider myCollider;
    GameObject sandstormInstance;
    float tornadoTimer = 0f;
    bool isSlashFinished = false;
    bool isStompStopped = false;
    int activeExplosions = 0;
    bool isThrowFinished = false;
    int activeProjectiles = 0;
    float throwTimer = 0f;
    float triTimer = 0f;
    List<Vector3> poisonZones = new List<Vector3>();
    HashSet<Actor> hitActors = new HashSet<Actor>();
    Vector3 finalAttackDir;
    bool hasUsedCharge80 = false;
    bool hasUsedCharge60 = false;
    bool hasUsedJump70 = false;
    bool hasUsedJump50 = false;
    float lineTimer = 0f;
    bool isInsideBigCircle;
    bool isInsideSafeCircle;


    int[] pattern = new int[] { 3, 4, 3, 4 };
    int patternIndex = 0;
    enum ThrowType
    {
        Fire,
        Poison
    }

    ThrowType currentThrowType;
    protected override void Awake()
    {
        base.Awake();
        myCollider = GetComponent<Collider>();

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
        tornadoTimer -= Time.deltaTime;
        slamExplosionTimer -= Time.deltaTime;
        throwTimer -= Time.deltaTime;
        triTimer -= Time.deltaTime;
        lineTimer -= Time.deltaTime;

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

            float hpPercent = (float)_currentHP / _maxHP;

            if (jumpTimer <= 0f)
            {
                if (!hasUsedJump70 && hpPercent <= 0.7f)
                {
                    hasUsedJump70 = true;
                    StartCoroutine(JumpAttack());
                    return;
                }
                else if (!hasUsedJump50 && hpPercent <= 0.5f)
                {
                    hasUsedJump50 = true;
                    StartCoroutine(JumpAttack());
                    return;
                }
            }

            if (chargeTimer <= 0f)
            {
                if (!hasUsedCharge80 && hpPercent <= 0.8f)
                {
                    hasUsedCharge80 = true;
                    StartCoroutine(Charge());
                    return;
                }
                else if (!hasUsedCharge60 && hpPercent <= 0.6f)
                {
                    hasUsedCharge60 = true;
                    StartCoroutine(Charge());
                    return;
                }
            }

            if (slashTimer <= 0f && dist <= slashRange)
                patterns.Add(() => StartCoroutine(Slash()));

            if (tornadoTimer <= 0f)
                patterns.Add(() => StartCoroutine(TornadoSkill()));

            if (throwTimer <= 0f)
                patterns.Add(() => StartCoroutine(ThrowPattern()));

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

            if (darkShotTimer <= 0f && dist >= darkShotMinDistance)
                patterns.Add(() => StartCoroutine(DarkShot()));

            patterns.Add(() => base.TryAttack());

            if (stompTimer <= 0f)
                patterns.Add(() => StartCoroutine(StompAttack()));

            if (slamExplosionTimer <= 0f)
                patterns.Add(() => StartCoroutine(SlamExplosionPattern()));

            if (triTimer <= 0f)
                patterns.Add(() => StartCoroutine(SmashCombo()));

            if (lineTimer <= 0f)
                patterns.Add(() => StartCoroutine(LinePatternAttack()));

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

    //불독 스킬 
    IEnumerator ThrowPattern()
    {
        if (isPhaseChanging) yield break;

        hitActors.Clear(); 

        isAttacking = true;
        isComboAttacking = true;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.updateRotation = false;

        Vector3 dir = currentTarget.position - transform.position;
        dir.y = 0;
        transform.rotation = Quaternion.LookRotation(dir);

        currentThrowType = (Random.value < 0.5f) ? ThrowType.Fire : ThrowType.Poison;

        animator.SetTrigger("Throw");

        yield return new WaitUntil(() => isThrowFinished);
        isThrowFinished = false;

        yield return new WaitUntil(() => activeProjectiles <= 0);

        if (currentThrowType == ThrowType.Poison)
        {
            ApplyPoisonDamage();
            poisonZones.Clear();
        }

        yield return new WaitForSeconds(throwDelay);

        throwTimer = throwCooldown;

        isComboAttacking = false;

        EndAttack();

        agent.isStopped = false;
        agent.updateRotation = true;
    }

    //불독 장판 스폰
    void SpawnThrowProjectiles()
    {
        int count = 7;
        activeProjectiles = count;

        for (int i = 0; i < count; i++)
        {
            Vector3 targetPos = GetRandomPointAroundPlayer(throwRadius);

            GameObject prefab = (currentThrowType == ThrowType.Fire)
                ? fireballPrefab
                : poisonBallPrefab;

            GameObject proj = Instantiate(prefab, throwPoint.position, Quaternion.identity);

            StartCoroutine(MoveProjectile(proj, targetPos));
        }
    }

    //플레이어 주변 던지기
    Vector3 GetRandomPointAroundPlayer(float radius)
    {
        float angle = Random.Range(0f, 360f);
        float dist = Random.Range(1f, radius);

        Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;

        Vector3 pos = currentTarget.position + dir * dist;
        pos.y = 0.05f;

        return pos;
    }

    IEnumerator MoveProjectile(GameObject proj, Vector3 targetPos)
    {
        Vector3 start = proj.transform.position;

        float time = 0f;
        float duration = 0.7f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            Vector3 pos = Vector3.Lerp(start, targetPos, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * 3f;

            proj.transform.position = pos;

            yield return null;
        }

        Destroy(proj);

        SpawnExplosionPattern(targetPos);
    }
    void SpawnExplosionPattern(Vector3 pos)
    {
        if (currentThrowType == ThrowType.Fire)
        {
            StartCoroutine(FireExplosion(pos));
        }
        else
        {
            StartCoroutine(PoisonExplosion(pos));
        }
    }

    //불 장판 
    IEnumerator FireExplosion(Vector3 pos)
    {
        float radius = 4f;

        GameObject indicator = Instantiate(throwIndicatorPrefab, pos, Quaternion.Euler(-90f, 0f, 0f));
        indicator.transform.localScale = Vector3.zero;

        SetIndicatorColor(indicator, Color.red);

        StartCoroutine(GrowIndicator(indicator, radius, throwWarningTime));

        yield return new WaitForSeconds(throwWarningTime);

        if (indicator != null)
            Destroy(indicator);

        GameObject vfx = Instantiate(fireVFX, pos, Quaternion.identity);
        Destroy(vfx, 2f);

        Collider[] hits = Physics.OverlapSphere(pos, radius, targetLayer);

        foreach (var hit in hits)
        {
            Actor actor = hit.GetComponent<Actor>();
            if (actor == null || actor.IsDead) continue;

            if (hitActors.Contains(actor)) continue;

            hitActors.Add(actor);

            actor.TakeDamage(fireDamage, 1f);
        }
        activeProjectiles--;
    }
    //독 장판
    IEnumerator PoisonExplosion(Vector3 pos)
    {
        poisonZones.Add(pos);

        GameObject bigIndicator = Instantiate(throwIndicatorPrefab, pos, Quaternion.Euler(-90, 0, 0));
        bigIndicator.transform.localScale = Vector3.one * poisonOuterRadius * 2f;

        bigIndicator.transform.position += Vector3.up * 0.01f;

        SetIndicatorColor(bigIndicator, Color.green);
        Destroy(bigIndicator, throwWarningTime);

        GameObject safeIndicator = Instantiate(throwIndicatorPrefab, pos, Quaternion.Euler(-90, 0, 0));
        safeIndicator.transform.localScale = Vector3.one * 4f * 2f; 

        var mat = safeIndicator.GetComponent<Renderer>().material;
        mat.renderQueue = 3100;

        safeIndicator.transform.position += Vector3.up * 0.08f;

        SetIndicatorColor(safeIndicator, Color.white);
        Destroy(safeIndicator, throwWarningTime);

        yield return new WaitForSeconds(throwWarningTime);


        int vfxCount = 15;

        for (int i = 0; i < vfxCount; i++)
        {
            Vector3 randomPos = GetRandomPointInDonut(pos, 4f, poisonOuterRadius);

            GameObject vfx = Instantiate(poisonVFX, randomPos, Quaternion.identity);
            Destroy(vfx, 2f);
        }


        activeProjectiles--;
    }

    Vector3 GetRandomPointInDonut(Vector3 center, float innerRadius, float outerRadius)
    {
        float angle = Random.Range(0f, 360f);
        float dist = Random.Range(innerRadius, outerRadius);

        Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
        Vector3 pos = center + dir * dist;

        pos.y = 0.05f;

        return pos;
    }

    //독 장판 데미지 주기
    void ApplyPoisonDamage()
    {
        float safeRadius = 4f;

        foreach (var zone in poisonZones)
        {
            Collider[] hits = Physics.OverlapSphere(zone, poisonOuterRadius, targetLayer);

            foreach (var hit in hits)
            {
                Actor actor = hit.GetComponent<Actor>();
                if (actor == null || actor.IsDead) continue;

                bool isInsideSafeCircle = false;

                foreach (var safeZone in poisonZones)
                {
                    float safeDist = Vector3.Distance(actor.transform.position, safeZone);

                    if (safeDist <= safeRadius)
                    {
                        isInsideSafeCircle = true;
                        break;
                    }
                }

                if (!isInsideSafeCircle)
                {
                    if (hitActors.Contains(actor)) continue;

                    hitActors.Add(actor);
                    actor.TakeDamage(poisonDamage, 1f);
                }
            }
        }
    }

    //장판 색 바꾸기
    void SetIndicatorColor(GameObject indicator, Color color)
    {
        Renderer rend = indicator.GetComponent<Renderer>();

        if (rend != null)
        {
            // 머티리얼 복사해서 색 변경 (공유 방지)
            rend.material = new Material(rend.material);
            rend.material.color = color;
        }
    }

    //장판 자라나는 코드
    IEnumerator GrowIndicator(GameObject indicator, float radius, float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            float scale = Mathf.Lerp(0f, radius * 2f, t);
            indicator.transform.localScale = new Vector3(scale, scale, scale);

            yield return null;
        }
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
        main.startSize = 7f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = slashAngle * 0.5f;
        shape.radius = 0f;
        shape.length = slashRange;

        float startOffset = 4.5f;

        Vector3 pos = transform.position + attackDirection * startOffset;
        pos.y += 0.1f;

        slashIndicator.transform.position = pos;

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

        myCollider.enabled = false;

        agent.isStopped = true;
        agent.updateRotation = false;

        animator.SetTrigger("Jump");

        yield return new WaitForSeconds(0.3f);

        ShowJumpIndicator();

        float followSpeed = 2.0f;
        float keepDistance = 2.5f;

        float upTime = 0.6f;
        float airTime = jumpDelay;
        float downTime = 0.5f;
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

        jumpIndicator.SetActive(false);

        yield return new WaitForSeconds(1f);

        ShowModel();
        animator.speed = 1.4f;
        timer = 0f;

        float startY = startPos.y + jumpHeight;

        while (timer < downTime)
        {
            if (isPhaseChanging || _isDead)
            {
                myCollider.enabled = true;
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

        yield return new WaitForSeconds(0.05f);
        myCollider.enabled = true;
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

        float diameter = jumpAttackRange * 1.9f * jumpIndicatorScaleMultiplier;

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

                    actor.TakeDamage(25, 1f);
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

                foreach (var hit in envHits)
                {
                    FractureThis f = hit.GetComponentInParent<FractureThis>();

                    if (f != null && f.gameObject.activeSelf)
                    {
                        f.FractureAndDestroy();

                        if (f.gameObject != hit.gameObject)
                        {
                            hit.gameObject.SetActive(false);
                        }

                        break;
                    }
                }

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

        yield return new WaitForSeconds(2f);

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

    //회오리 스킬
    IEnumerator TornadoSkill()
    {
        if (isPhaseChanging) yield break;

        isAttacking = true;
        isComboAttacking = true;

        agent.isStopped = true;
        agent.updateRotation = false;

        animator.SetTrigger("Tornado");

        float timer = 0f;

        yield return new WaitUntil(() => tornadoVFX.activeSelf);

        while (timer < tornadoDuration)
        {
            timer += Time.deltaTime;

            if (currentTarget != null)
            {
                Vector3 dir = currentTarget.position - transform.position;
                dir.y = 0;

                if (dir.sqrMagnitude > 0.01f)
                {
                    transform.position += dir.normalized * tornadoSpeed * Time.deltaTime;
                    transform.rotation = Quaternion.LookRotation(dir);
                }

                ApplyTornadoPull(currentTarget);
            }

            yield return null;
        }

        EndTornado();

        yield return new WaitForSeconds(0.5f);
        yield return new WaitForSeconds(tornadoDelay);

        isComboAttacking = false;
        EndAttack();

        tornadoTimer = tornadoCooldown;

        agent.isStopped = false;
        agent.updateRotation = true;
    }

    public void OnTornadoTransform()
    {
        animator.speed = 0f;

        HideModel();
        myCollider.enabled = false;

        tornadoVFX.SetActive(true);

        tornadoVFX.transform.position = transform.position;
        tornadoVFX.transform.localPosition = Vector3.zero;

        var ps = tornadoVFX.GetComponent<ParticleSystem>();
        if (ps != null) ps.Play();

        sandstormInstance = Instantiate(
            sandstormVFXPrefab,
            transform.position,
            Quaternion.identity
        );
    }

    void EndTornado()
    {
        tornadoVFX.SetActive(false);

        if (sandstormInstance != null)
            Destroy(sandstormInstance);

        ShowModel();
        myCollider.enabled = true;

        animator.speed = 1f;
    }

    //토네이도 플레이어 당기는 힘
    void ApplyTornadoPull(Transform target)
    {
        Actor actor = target.GetComponent<Actor>();

        if (actor == null || actor.IsDead) return;

        float dist = Vector3.Distance(transform.position, target.position);

        if (dist > tornadoPullRadius) return;

        Vector3 dir = (transform.position - target.position).normalized;

        float force = tornadoPullForce * (1f - (dist / tornadoPullRadius));

        if (dist < tornadoMinDistance)
            force *= 0.3f;

        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(dir * force, ForceMode.Acceleration);
        }
        else
        {
            target.position += dir * force * Time.deltaTime;
        }
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

        float diameter = spinAttackRange * 0.9f;

        spinIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);

        Vector3 pos = transform.position;
        pos.y += 0.1f;

        spinIndicator.transform.position = pos;

        spinIndicator.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
    //휘두르기 데미지 주기
    public void DealSpinDamage()
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

        Vector3 origin = transform.position + Vector3.up * 1f;
        RaycastHit hit;

        if (Physics.Raycast(origin, Vector3.down, out hit, 10f, groundLayer))
        {
            stompIndicator.transform.position = hit.point + Vector3.up * -0.01f;
        }

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

    IEnumerator StompResumeRoutine()
    {
        yield return new WaitForSeconds(2.5f);

        animator.speed = 0.25f;
    }


    //양팔 - 내려찍기 - 똥 패턴
    IEnumerator SlamExplosionPattern()
    {
        if (isPhaseChanging) yield break;

        isAttacking = true;
        isComboAttacking = true;

        agent.isStopped = true;
        agent.updateRotation = false;

        Vector3 dir = currentTarget.position - transform.position;
        dir.y = 0;
        transform.rotation = Quaternion.LookRotation(dir);
        attackDirection = dir.normalized;

        animator.SetTrigger("DoubleSlash");

        yield return new WaitUntil(() => isSlashFinished);
        isSlashFinished = false;

        animator.SetTrigger("DdongStomp");
        animator.speed = 1f;

        yield return new WaitUntil(() => isStompStopped);
        isStompStopped = false;

        yield return StartCoroutine(SpawnAndExplodeSequential());

        animator.speed = 1f;
        animator.SetTrigger("ToIdle");

        yield return new WaitForSeconds(explosionDelay);

        slamExplosionTimer = slamExplosionCooldown;

        attackTimer = attackCooldown;

        isComboAttacking = false;
        EndAttack();

        agent.isStopped = false;
        agent.updateRotation = true;
    }


    IEnumerator SpawnAndExplodeSequential()
    {
        int randomCount = Random.Range(6, 10);

        int totalCount = randomCount + 2;

        float spawnDelay = 0.1f;

        activeExplosions = totalCount;

        SpawnSingleCircle(leftHand.position);
        SpawnSingleCircle(rightHand.position);

        yield return new WaitForSeconds(0.6f); 

        for (int i = 0; i < randomCount; i++)
        {
            if (_isDead) yield break;

            float angle = Random.Range(0f, 360f);
            float radius = Random.Range(3f, 8f);

            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Vector3 center = Random.value < 0.5f ? transform.position : currentTarget.position;

            Vector3 pos = center + dir * radius;
            pos.y = 0.05f;

            SpawnSingleCircle(pos);

            yield return new WaitForSeconds(spawnDelay);
        }

        yield return new WaitUntil(() => activeExplosions <= 0);
    }

    void SpawnSingleCircle(Vector3 pos)
    {
        pos.y = 0.05f;

        GameObject indicator = Instantiate(
            circleIndicatorPrefab,
            pos,
            Quaternion.Euler(-90f, 0f, 0f)
        );

        Renderer renderer = indicator.GetComponent<Renderer>();

        float baseSize = renderer.bounds.size.x;
        float targetDiameter = explosionRadius * 2f;
        float scale = targetDiameter / baseSize;

        indicator.transform.localScale = Vector3.one * scale;

        float explodeDelay = Random.Range(minExplodeDelay, maxExplodeDelay);

        StartCoroutine(GrowAndExplode(indicator, pos, explodeDelay));
    }


    //스폰하고 터지는 코드
    IEnumerator GrowAndExplode(GameObject indicator, Vector3 pos, float delay)
    {
        if (indicator == null)
        {
            activeExplosions--;
            yield break;
        }

        Renderer renderer = indicator.GetComponent<Renderer>();

        float baseSize = renderer.bounds.size.x;
        float targetDiameter = explosionRadius * 2f;
        float targetScale = targetDiameter / indicatorBaseSize;

        float timer = 0f;

        indicator.transform.localScale = Vector3.zero;

        while (timer < delay)
        {
            if (_isDead)
            {
                activeExplosions--;
                yield break;
            }

            timer += Time.deltaTime;
            float t = timer / delay;

            float scale = Mathf.Lerp(0f, targetScale, t);
            indicator.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        if (indicator != null)
            Destroy(indicator);

        GameObject vfx = Instantiate(explosionVFXPrefab, pos, Quaternion.identity);
        Destroy(vfx, 2f); 

        Collider[] hits = Physics.OverlapSphere(pos, explosionRadius, targetLayer);

        foreach (var hit in hits)
        {
            Actor actor = hit.GetComponent<Actor>();
            if (actor == null || actor.IsDead) continue;

            actor.TakeDamage(explosionDamage, 1f);
        }

        activeExplosions--;
    }

    //손에서 생성
    void SpawnTrianglesFromHand(Transform hand)
    {
        float angleStep = 360f / triangleCountPerHand;

        for (int i = 0; i < triangleCountPerHand; i++)
        {
            float baseAngle = i * angleStep;
            float randomOffset = Random.Range(-20f, 20f);
            float finalAngle = baseAngle + randomOffset;

            Quaternion rot = Quaternion.Euler(0, finalAngle, 0);

            Vector3 rayOrigin = hand.position + Vector3.up * 0.5f;
            Vector3 spawnPos = hand.position;

            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 50f, groundLayer))
            {
                spawnPos = hit.point;
            }
            else
            {
                spawnPos.y = 0f;
            }

            spawnPos.y += 0.05f;

            GameObject tri = Instantiate(trianglePrefab, spawnPos, rot);

            TriangleMesh aoe = tri.GetComponent<TriangleMesh>();
            if (aoe != null)
            {
                aoe.Init(spawnPos, tri.transform.forward, triangleLength, triangleWidth);
            }
        }
    }

    //그 삼각형 콤보
    IEnumerator SmashCombo()
    {
        isAttacking = true;
        isComboAttacking = true;

        agent.isStopped = true;
        agent.updateRotation = false;

        yield return StartCoroutine(GroundSmashPattern());
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(RotateLikeSlash(1f));
        animator.SetTrigger("Claw1");
        yield return new WaitForSeconds(2.0f);

        yield return StartCoroutine(RotateLikeSlash(1f));
        animator.SetTrigger("Claw2");
        yield return new WaitForSeconds(2.0f);

        yield return StartCoroutine(RotateLikeSlash(1f));

        animator.SetTrigger("Down_Final");

        yield return new WaitForSeconds(3.2f);

        triTimer = triCooldown;
        attackTimer = attackCooldown;
        isComboAttacking = false;
        EndAttack();

        agent.isStopped = false;
        agent.updateRotation = true;
    }
    IEnumerator GroundSmashPattern()
    {
        animator.SetTrigger("Down");

        yield return new WaitForSeconds(0.8f); 
    }

    //슬래쉬처럼 바라보기 사용
    IEnumerator RotateLikeSlash(float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
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

                    finalAttackDir = dir.normalized; 
                }
            }

            yield return null;
        }
    }

    //마지막 삼각형 공격
   public void SpawnFinalTriangle()
    {
        Debug.DrawRay(transform.position, transform.forward * 5f, Color.red, 2f);
        Vector3 dir = transform.forward;
        dir.y = 0f;
        dir.Normalize();

        Quaternion rot = Quaternion.LookRotation(dir);

        Vector3 spawnPos = transform.position;

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 50f, groundLayer))
        {
            spawnPos = hit.point;
        }

        spawnPos.y += 0.05f;

        GameObject tri = Instantiate(trianglePrefab, spawnPos, rot);

        TriangleMesh aoe = tri.GetComponent<TriangleMesh>();
        if (aoe != null)
        {
            aoe.Init(spawnPos, dir, triangleLength, triangleWidth, 1f);
        }
    }

    IEnumerator TripleSpawnRoutine()
    {
        for (int i = 0; i < 3; i++)
        {
            SpawnFinalTriangle(); 
            yield return new WaitForSeconds(0.7f); 
        }
    }

    //능지 패턴
    IEnumerator LinePatternAttack()
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

        yield return new WaitForSeconds(lineWarningTime);

        animator.SetTrigger("Line");

        yield return StartCoroutine(LinePatternRoutine());

        animator.speed = 1f;

        lineTimer = lineCooldown;

        attackTimer = attackCooldown;

        isComboAttacking = false;

        EndAttack();

        agent.isStopped = false;
        agent.updateRotation = true;
    }

    //능지패턴루틴
    IEnumerator LinePatternRoutine()
    {
        float spacing = 7f;
        float delay = 0.8f;
        float radius = 10f; 

        PatternStep[] steps = new PatternStep[]
        {
        new PatternStep(true, 3),
        new PatternStep(false, 4),
        new PatternStep(true, 4),
        new PatternStep(false, 3),
        };

        Vector3 basePos = transform.position;

        foreach (var step in steps)
        {
            Vector3 center = GetRandomAroundBoss(basePos, radius);

            if (step.isHorizontal)
                SpawnHorizontalLines(center, spacing);
            else
                SpawnVerticalLines(center, spacing);

            yield return new WaitForSeconds(delay);
        }

        yield return new WaitForSeconds(3f);
    }
    List<Vector2Int> usedCells = new List<Vector2Int>();
    struct PatternStep
    {
        public bool isHorizontal;
        public int count;

        public PatternStep(bool isHorizontal, int count)
        {
            this.isHorizontal = isHorizontal;
            this.count = count;
        }
    }

    //가로라인
    void SpawnVerticalLines(Vector3 center, float spacing)
    {
        Vector3 dir = transform.forward;

        for (int i = -1; i <= 1; i++)
        {
            Vector3 pos = center + transform.right * (i * spacing);
            SpawnLineIndicator(pos, dir);
        }
    }

    //세로라인
    void SpawnHorizontalLines(Vector3 center, float spacing)
    {
        Vector3 dir = transform.right;

        for (int i = -1; i <= 1; i++)
        {
            Vector3 pos = center + transform.forward * (i * spacing);
            SpawnLineIndicator(pos, dir);
        }
    }
    //능지패턴생성
    void SpawnLineIndicator(Vector3 pos, Vector3 dir)
    {
        Quaternion rot = Quaternion.LookRotation(dir) * Quaternion.Euler(90f, 0f, 0f);

        GameObject obj = Instantiate(lineIndicatorPrefab, pos, rot);

        LineAOE aoe = obj.GetComponent<LineAOE>();
        if (aoe != null)
        {
            aoe.Init(3f); // 3초 후 터짐
        }
    }

    //보스근처에서 생성
    Vector3 GetRandomAroundBoss(Vector3 basePos, float radius)
    {
        Vector2 randomCircle = Random.insideUnitCircle * radius;

        return basePos +
               transform.right * randomCircle.x +
               transform.forward * randomCircle.y;
    }
    //데미지 받는 함수
    public override void TakeDamage(int damage, float severityOverride = -1f, bool isHeavyAttack = false, bool showDamageText = true)
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

        vfx.transform.localScale = Vector3.one * 2.8f;

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

    public void TriangleClawVFX()
    {
        if (clawVFXPrefab == null) return;

        Vector3 spawnPos = clawSpawnPoint != null
            ? clawSpawnPoint.position
            : transform.position;

        Vector3 dir = transform.forward;
        dir.y = 0f;
        dir.Normalize();

        Quaternion rot = Quaternion.LookRotation(dir);
        rot *= Quaternion.AngleAxis(180f, Vector3.forward);

        GameObject vfx = Instantiate(clawVFXPrefab, spawnPos, rot);

        vfx.transform.localScale = Vector3.one * 1.3f;

        ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.simulationSpeed = 1f; 
            ps.Play();
        }

        Destroy(vfx, 1.5f);
    }
    public void SpawnClawDdongVFX()
    {
        if (clawDdongVFXPrefab == null) return;

        Vector3 spawnPos = clawSpawnPoint != null
            ? clawSpawnPoint.position
            : transform.position;

        Quaternion rot = Quaternion.LookRotation(attackDirection);
        rot *= Quaternion.AngleAxis(180f, Vector3.forward);

        GameObject vfx = Instantiate(clawDdongVFXPrefab, spawnPos, rot);

        vfx.transform.localScale = Vector3.one * 1.3f;

        Destroy(vfx, 2f);
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

        vfx.transform.localScale = Vector3.one * 2.3f;

        Destroy(vfx, 2f);
    }

    //불독 돌기
    public void FirePosSpinVFX()
    {
        if (spinVFXPrefab == null) return;

        Vector3 pos = transform.position;
        pos.y += 0.1f;

        GameObject vfx = Instantiate(spinVFXPrefab, pos, Quaternion.identity);

        vfx.transform.localScale = Vector3.one * 1.4f;

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

    IEnumerator FillStompVFX(GameObject vfx)
    {
        float timer = 0f;
        float maxScale = stompRange * 0.41f;
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
                Color c = (t < 0.5f) ? Color.Lerp(start, mid, t * 2f) : Color.Lerp(mid, end, (t - 0.5f) * 2f);
                rend.material.color = c;
            }
            yield return null;
        }

        int explosionVfxCount = 80;
        for (int i = 0; i < explosionVfxCount; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * stompRange;

            Vector3 spawnPos = vfx.transform.position + new Vector3(randomCircle.x, 0.5f, randomCircle.y);

            GameObject explosion = Instantiate(fireVFX, spawnPos, Quaternion.identity);
            Destroy(explosion, 2f);
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

    void ResetAnimSpeed()
    {
        animator.speed = 1f;
    }


    //애니메이션 이벤트 함수들 
    public void OnSlashStart()
    {
        animator.speed = 1.2f;
    }

    public void OnSlashSlow()
    {
        animator.speed = 0.3f;
    }

    public void OnSlashEnd()
    {
        animator.speed = 1f;
        isSlashFinished = true;
    }

    public void OnStompFreeze()
    {
        animator.speed = 0f;
        isStompStopped = true;
    }

    public void OnStompReady()
    {
        ShowStompIndicator();

        animator.speed = 0f;

        StartCoroutine(StompResumeRoutine());
    }

    public void FireDarkShot()
    {
        FireSpreadProjectiles(attackDirection);
    }

    public void OnJumpImpact()
    {
        if (jumpIndicator != null)
            jumpIndicator.SetActive(false);

        DealJumpDamage();
    }
    public void OnThrowProjectile()
    {
        SpawnThrowProjectiles();
    }

    public void OnThrowEnd()
    {
        isThrowFinished = true;
    }

    public void SpawnHandTriangles()
    {
        SpawnTrianglesFromHand(leftHand);
        SpawnTrianglesFromHand(rightHand);
    }

    public void StartFinalTripleAttack()
    {
        StartCoroutine(TripleSpawnRoutine());
    }

    public void OnLineStop()
    {
        animator.speed = 0f;
    }
    public override void ResetEnemy()
    {
        // 부모(Enemy)의 리셋을 먼저 실행해서 타겟(currentTarget)과 어그로를 싹 지웁니다.
        base.ResetEnemy();

        // 진행 중인 보스 패턴 코루틴 강제 종료
        StopAllCoroutines();

        if (agent != null)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        // 보스 전용 플래그 초기화
        isComboAttacking = false;
        isPhaseChanging = false;
        isInvincible = false;
        isCharging = false;
        isLocked = false;
        isSlashFinished = false;
        isStompStopped = false;
        hasStartedCombat = false;
        activeExplosions = 0;

        // 보스 전용 스킬 타이머 초기화 
        attackTimer = 2f;
        slashTimer = 3f;
        jumpTimer = 6f;
        chargeTimer = 9f;
        spinTimer = 12f;
        darkShotTimer = 5f;
        stompTimer = 8f;
        tornadoTimer = 15f;
        slamExplosionTimer = 10f;

        animator.speed = 1f;
        animator.Rebind();
        animator.Update(0f);

        myCollider.enabled = true;
        ShowModel();
        isRightHandBroken = false;
        if (RightHandPointCollider != null)
        {
            RightHandPointCollider.SetActive(true);
            WeakPoint wp = RightHandPointCollider.GetComponent<WeakPoint>();
            if (wp != null) wp.Init(RightHandPointHP, this);
        }

        if (slashIndicator != null) slashIndicator.SetActive(false);
        if (jumpIndicator != null) jumpIndicator.SetActive(false);
        if (chargeIndicator != null) chargeIndicator.SetActive(false);
        if (spinIndicator != null) spinIndicator.SetActive(false);
        if (stompIndicator != null) stompIndicator.SetActive(false);

        if (tornadoVFX != null) tornadoVFX.SetActive(false);
        if (sandstormInstance != null) Destroy(sandstormInstance);

        currentPhase = BossPhase.Phase1;
    }

    protected override void Die()
    {
        if (_isDead) return;

        StopAllCoroutines();

        if (animator != null)
            animator.speed = 1f;

        if (slashIndicator != null) slashIndicator.SetActive(false);
        if (jumpIndicator != null) jumpIndicator.SetActive(false);
        if (chargeIndicator != null) chargeIndicator.SetActive(false);
        if (spinIndicator != null) spinIndicator.SetActive(false);
        if (stompIndicator != null) stompIndicator.SetActive(false);
        if (tornadoVFX != null) tornadoVFX.SetActive(false);
        if (RightHandPointCollider != null) RightHandPointCollider.SetActive(false);

        if (myCollider != null)
            myCollider.enabled = false;

        base.Die();
    }

    protected override IEnumerator DieRoutine()
    {
        yield return new WaitForSeconds(3f);

        if (agent != null)
            agent.enabled = false;

        Destroy(gameObject);
    }
}

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
    public Transform clawSpawnPoint; // 손 위치

    [Header("1페이지 점프 공격")]
    public float jumpAttackRange = 5f;
    public int jumpAttackDamage = 30;
    public float jumpDelay = 2.5f; 
    public float jumpCooldown = 8f;
    public GameObject jumpIndicatorPrefab;

    [Header("1페이지 돌진 스킬")]
    public float chargeCooldown = 10f;
    public float chargeDistance = 10f;
    public float chargeSpeed = 20f;
    public float chargeLockTime = 2f;
    public GameObject chargeIndicatorPrefab;

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


    [Header("Phase2 콤보")]
    public float comboDelay_P2 = 0.3f;
    public float comboRecovery_P2 = 1.0f;

    [Header("Phase2 휘두르기")]
    public float spinAttackRange = 5f;
    public int spinAttackDamage = 35;
    public float spinCooldown = 6f;
    public GameObject spinIndicatorPrefab;

    float spinTimer = 0f;
    GameObject spinIndicator;


    public Transform headTransform; 
    int comboIndex = 0;
    bool isComboAttacking = false;
    bool isPhaseChanging = false;
    [Header("Vfx")]
    public GameObject roarVFXPrefab;
    public GameObject clawVFXPrefab;
    public GameObject biteVFXPrefab;
    public GameObject spinVFXPrefab;
    protected override void Awake()
    {
        base.Awake();

        slashIndicator = Instantiate(slashIndicatorPrefab, transform);
        slashIndicator.SetActive(false);

        jumpIndicator = Instantiate(jumpIndicatorPrefab, transform);
        jumpIndicator.SetActive(false);

        chargeIndicator = Instantiate(chargeIndicatorPrefab, transform);
        chargeIndicator.SetActive(false);

        spinIndicator = Instantiate(spinIndicatorPrefab, transform);
        spinIndicator.SetActive(false);
    }

    protected override void Update()
    {
        attackTimer -= Time.deltaTime;
        slashTimer -= Time.deltaTime;
        jumpTimer -= Time.deltaTime;
        chargeTimer -= Time.deltaTime;
        spinTimer -= Time.deltaTime;

        if (_isDead) return;

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

    //페이즈 업데이트
    void UpdatePhase()
    {
        if (isPhaseChanging) return;

        float hpPercent = (float)_currentHP / _maxHP;

        if (currentPhase == BossPhase.Phase1 && hpPercent <= phase2HpPercent)
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

        yield return new WaitForSeconds(2.0f);

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
            Attack();
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

            patterns.Add(() => base.TryAttack());

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
    //오른쪽 할퀴기
    IEnumerator Slash()
    {
        if (isPhaseChanging) yield break;
        isAttacking = true;
        isComboAttacking = true;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.updateRotation = false;

        float rotateTime = 0.5f; 
        float timer = 0f;

        while (timer < rotateTime)
        {
            if (isPhaseChanging)
            {
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

        yield return new WaitForSeconds(1.5f);

        slashIndicator.SetActive(false);

        animator.SetTrigger("Slash");

        yield return new WaitForSeconds(0.3f);

        DealSlashDamage();

        yield return new WaitForSeconds(1.0f);
        yield return new WaitForSeconds(1.5f);
        slashTimer = slashCooldown;

        isComboAttacking = false;
        EndAttack();

        agent.isStopped = false;
        agent.updateRotation = true;
    }
    //할퀴기 범위장판
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
        isAttacking = true;
        isComboAttacking = true;
        isInvincible = true; 

        agent.isStopped = true;
        agent.updateRotation = false;

        animator.SetTrigger("Jump");

        yield return new WaitForSeconds(0.3f);

        ShowJumpIndicator();

        float airTime = 1.7f;
        float followSpeed = 1.8f;

        float timer = 0f;

        float keepDistance = 2.0f; 

        while (timer < airTime)
        {
            if (isPhaseChanging || _isDead)
            {
                EndAttack(); 
                yield break;
            }

            timer += Time.deltaTime;

            if (currentTarget != null)
            {
                Vector3 targetPos = currentTarget.position;
                targetPos.y = transform.position.y;

                Vector3 dir = targetPos - transform.position;
                float dist = dir.magnitude;

                if (dist > keepDistance) 
                {
                    Vector3 moveDir = dir.normalized;

                    transform.position += moveDir * followSpeed * Time.deltaTime;
                }

                if (dir.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.LookRotation(dir);
            }

            jumpIndicator.transform.position = transform.position;

            yield return null;
        }

        jumpIndicator.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        isInvincible = false;

        yield return new WaitForSeconds(3.0f);

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

        float diameter = jumpAttackRange * 2f;

        jumpIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);

        Vector3 pos = transform.position;
        pos.y += 0.05f;

        jumpIndicator.transform.position = pos;

        jumpIndicator.transform.rotation = Quaternion.Euler(90f, 0, 0);
    }
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
                        Quaternion.Euler(90f, 0, 0);

                    chargeIndicator.transform.localScale =
                        new Vector3(2f, chargeDistance, 1f);
                }
            }

            yield return null;
        }
        Vector3 finalDir = transform.forward;
        attackDirection = finalDir;

        chargeIndicator.SetActive(false);

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
                StartCoroutine(StunRoutine());
                yield break;
            }

            yield return null;
        }

        yield return new WaitForSeconds(3f);

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

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.updateRotation = false;

        animator.SetBool("Stun", true); 

        yield return new WaitForSeconds(stunDuration);

        animator.SetBool("Stun", false);

        isStuned = false;
        isAttacking = false;

        agent.isStopped = false;
        agent.updateRotation = true;

        attackTimer = 3f; 
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

        yield return new WaitForSeconds(1.5f);
        yield return new WaitForSeconds(2f);

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

        spinIndicator.transform.rotation = Quaternion.Euler(90f, 0, 0);
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

    public override void TakeDamage(int damage, float severityOverride = -1f)
    {
        if (isInvincible || isPhaseChanging) return; 

        if (isHit || _isDead) return;
        base.TakeDamage(damage, severityOverride);

        EndHit();
    }

    //vfx 애니메이션들

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

        ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
        }

        Destroy(vfx, 1.5f);
    }

    //포효
    public void SpawnRoarVFX()
    {
        GameObject vfx = Instantiate(roarVFXPrefab, transform.position, Quaternion.identity);

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

        vfx.transform.localScale = Vector3.one * 2f; 

        Destroy(vfx, 2f);
    }

}

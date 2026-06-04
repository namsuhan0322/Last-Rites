using System.Collections;
using UnityEngine;

public class BlackSpiderBoss : Enemy
{
    [Header("랜덤 기본 공격 설정")]
    public int attackPatternCount = 3;
    public float attackAnimTime = 1.0f;
    public float attackCooldownTime = 2.0f;
    public float postAttackDelay = 1.5f;

    [Header("양발 내려찍기")]
    public GameObject stompIndicatorPrefab;
    public float doubleStompCooldown = 8f;
    public float doubleStompReadyTime = 0.8f;
    public float doubleStompAnimTime = 2f;
    public float doubleStompRecoveryTime = 3f;
    public int doubleStompDamage = 30;

    public float doubleStompRange = 5f;

    [Header("2페이즈 설정")]
    public float phase2HpPercent = 0.4f;
    public float phaseRoarTime = 2.0f;
    public bool isPhase2 = false;
    private bool phase2Started = false;

    [Header("폭발 스킬")]
    public GameObject explosionIndicatorPrefab;
    public float explosionReadyTime = 1.2f;
    public float explosionAnimTime = 1.5f;
    public float explosionRecoveryTime = 2.0f;
    public float explosionRange = 6f;
    public int explosionDamage = 40;

    [Header("회전 설정")]
    public float turnSpeed = 8f;
    public float attackStartAngle = 8f;
    public float maxTurnTime = 1.0f;

    private bool isRandomAttacking = false;
    private bool isSkillAttacking = false;
    private bool isRecovering = false;
    private GameObject explosionIndicator;
    private bool isPhaseSkillPlaying = false;
    private float doubleStompTimer = 0f;
    private GameObject stompIndicator;

    protected override void Awake()
    {
        base.Awake();

        if (stompIndicatorPrefab != null)
        {
            stompIndicator = Instantiate(stompIndicatorPrefab, transform);
            stompIndicator.SetActive(false);
        }

        if (explosionIndicatorPrefab != null)
        {
            explosionIndicator = Instantiate(explosionIndicatorPrefab, transform);
            explosionIndicator.SetActive(false);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (_isDead) return;

        attackTimer -= Time.deltaTime;
        doubleStompTimer -= Time.deltaTime;

        CheckPhase2();
    }

    protected override void TryAttack()
    {
        if (currentTarget == null) return;

        if (isAttacking || isRandomAttacking || isSkillAttacking || isRecovering || isPhaseSkillPlaying)
        {
            StopAgent();
            return;
        }

        float dist = Vector3.Distance(transform.position, currentTarget.position);
        if (dist > attackRange) return;

        if (doubleStompTimer <= 0f)
        {
            StartCoroutine(DoubleStomp());
            return;
        }

        if (attackTimer <= 0f)
        {
            StartCoroutine(RandomAttackRoutine());
        }
    }

    //2페이지 인가?
    void CheckPhase2()
    {
        if (phase2Started || isPhase2)
            return;

        float hpRate = (float)CurrentHP / MaxHP;

        if (hpRate <= phase2HpPercent)
        {
            phase2Started = true;
            StartCoroutine(Phase2RoarAndExplosion());
        }
    }

    IEnumerator RandomAttackRoutine()
    {
        isAttacking = true;
        isRandomAttacking = true;

        StopAgent();
        yield return StartCoroutine(FaceTargetSmooth());

        attackTimer = attackCooldownTime;

        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);

        int randomAttackIndex = Random.Range(1, attackPatternCount + 1);
        animator.SetTrigger("Attack" + randomAttackIndex);

        yield return new WaitForSeconds(attackAnimTime);

        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);

        yield return new WaitForSeconds(postAttackDelay);

        isRandomAttacking = false;
        isAttacking = false;

        ResumeAgent();
    }

    IEnumerator DoubleStomp()
    {
        isAttacking = true;
        isSkillAttacking = true;

        StopAgent();
        yield return StartCoroutine(FaceTargetSmooth());

        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);

        if (stompIndicator != null)
        {
            stompIndicator.SetActive(true);
            stompIndicator.transform.localPosition = Vector3.zero;
            stompIndicator.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            stompIndicator.transform.localScale = Vector3.one * doubleStompRange * 2f;
        }

        yield return new WaitForSeconds(doubleStompReadyTime);

        if (stompIndicator != null)
            stompIndicator.SetActive(false);

        animator.SetTrigger("DoubleStomp");
        doubleStompTimer = doubleStompCooldown;

        yield return new WaitForSeconds(doubleStompAnimTime);

        yield return StartCoroutine(Recover(doubleStompRecoveryTime));

        attackTimer = attackCooldownTime;

        isSkillAttacking = false;
        isAttacking = false;

        ResumeAgent();
    }

    void DealDoubleStompDamage()
    {
        Collider[] hits = Physics.OverlapSphere(
          transform.position,
          doubleStompRange,
          targetLayer
         );


        foreach (var hit in hits)
        {
            Actor actor = hit.GetComponent<Actor>();

            if (actor == null || actor == this)
                continue;

            actor.TakeDamage(doubleStompDamage, 1f);
        }
    }

    IEnumerator Phase2RoarAndExplosion()
    {
        isPhaseSkillPlaying = true;
        isAttacking = true;
        isSkillAttacking = true;

        StopAgent();

        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);

        animator.SetTrigger("PhaseRoar");

        yield return new WaitForSeconds(phaseRoarTime);

        isPhase2 = true;

        if (explosionIndicator != null)
        {
            explosionIndicator.SetActive(true);
            explosionIndicator.transform.localPosition = Vector3.zero;
            explosionIndicator.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            explosionIndicator.transform.localScale = Vector3.one * explosionRange * 2f;
        }

        yield return new WaitForSeconds(explosionReadyTime);

        if (explosionIndicator != null)
            explosionIndicator.SetActive(false);

        animator.SetTrigger("Explosion");

        yield return new WaitForSeconds(explosionAnimTime);

        yield return StartCoroutine(Recover(explosionRecoveryTime));

        attackTimer = attackCooldownTime;
        doubleStompTimer = doubleStompCooldown;

        isPhaseSkillPlaying = false;
        isSkillAttacking = false;
        isAttacking = false;

        ResumeAgent();
    }

    void DealExplosionDamage()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            explosionRange,
            targetLayer
        );

        foreach (var hit in hits)
        {
            Actor actor = hit.GetComponent<Actor>();

            if (actor == null || actor == this)
                continue;

            actor.TakeDamage(explosionDamage, 1f);
        }
    }

    IEnumerator Recover(float time)
    {
        isRecovering = true;

        StopAgent();

        yield return new WaitForSeconds(time);

        isRecovering = false;
    }

    void StopAgent()
    {
        if (agent == null) return;

        agent.updateRotation = false;

        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
    }

    void ResumeAgent()
    {
        if (agent == null) return;

        agent.updateRotation = true;

        if (!_isDead && agent.enabled && agent.isOnNavMesh)
            agent.isStopped = false;
    }

    protected override bool IsRecovering()
    {
        return isRecovering || isSkillAttacking || isPhaseSkillPlaying;
    }

    public override void EndAttack()
    {
        if (isRandomAttacking || isSkillAttacking)
        {
            StopAgent();
            return;
        }

        base.EndAttack();
    }

    IEnumerator FaceTargetSmooth()
    {
        float timer = 0f;

        while (timer < maxTurnTime)
        {
            if (currentTarget == null)
                yield break;

            Vector3 dir = currentTarget.position - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.001f)
                yield break;

            attackDirection = dir.normalized;

            Quaternion targetRot = Quaternion.LookRotation(attackDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * turnSpeed
            );

            float angle = Vector3.Angle(transform.forward, attackDirection);

            if (angle <= attackStartAngle)
                yield break;

            timer += Time.deltaTime;
            yield return null;
        }
    }

    public void OnExplosionHit()
    {
        DealExplosionDamage();
    }

    public void OnDoubleStompHit()
    {
        DealDoubleStompDamage();
    }
}

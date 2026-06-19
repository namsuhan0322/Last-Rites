using System.Collections;
using UnityEngine;

public class DemonSpiderBoss : Enemy
{
    [Header("랜덤 기본 공격 설정")]
    public int attackPatternCount = 3;
    public float attackAnimTime = 1.0f;
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

    [Header("2페이즈 점프 스킬")]
    public float jumpSkillCooldown = 10f;
    public float jumpSkillUseRange = 15f;
    public float jumpUpTime = 0.6f;
    public float jumpHeight = 8f;
    public GameObject jumpIndicator;
    public float indicatorFollowTime = 2.0f;
    public float indicatorStopTime = 0.6f;

    public float fallTime = 0.5f;
    public float jumpDamageRange = 4f;
    public int jumpDamage = 40;

    [Header("회전 설정")]
    public float turnSpeed = 8f;
    public float attackStartAngle = 8f;
    public float maxTurnTime = 1.0f;

    private bool isRandomAttacking = false;
    private bool isSkillAttacking = false;
    private bool isRecovering = false;
    private bool isPhaseSkillPlaying = false;
    private float doubleStompTimer = 0f;
    private GameObject stompIndicator;
    private float jumpSkillTimer = 0f;

    protected override void Awake()
    {
        base.Awake();

        if (stompIndicatorPrefab != null)
        {
            stompIndicator = Instantiate(stompIndicatorPrefab, transform);
            stompIndicator.SetActive(false);
        }

        if (jumpIndicator != null)
            jumpIndicator.SetActive(false);
    }

    protected override void Update()
    {
        base.Update();

        if (_isDead) return;

        attackTimer -= Time.deltaTime;
        doubleStompTimer -= Time.deltaTime;
        jumpSkillTimer -= Time.deltaTime;

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

        if (isPhase2 && jumpSkillTimer <= 0f && dist <= jumpSkillUseRange)
        {
            StartCoroutine(JumpAttack());
            return;
        }

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

        attackTimer = attackCooldown;

        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);

        int randomAttackIndex = Random.Range(1, attackPatternCount + 1);
        animator.SetTrigger("Attack" + randomAttackIndex);
        PlayBossBoneSpiderAttackSound();

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

        attackTimer = attackCooldown;

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
        PlayBossBoneRoarSound();

        yield return new WaitForSeconds(phaseRoarTime);

        isPhase2 = true;

        attackTimer = attackCooldown;
        doubleStompTimer = doubleStompCooldown;

        isPhaseSkillPlaying = false;
        isSkillAttacking = false;
        isAttacking = false;

        ResumeAgent();
    }

    IEnumerator JumpAttack()
    {
        isAttacking = true;
        isSkillAttacking = true;

        StopAgent();

        agent.updatePosition = false;
        agent.updateRotation = false;

        yield return StartCoroutine(FaceTargetSmooth());

        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);

        Vector3 startPos = transform.position;
        Vector3 airPos = startPos + Vector3.up * jumpHeight;

        animator.SetTrigger("Jump");
        PlayBossBoneSpiderJumpUpSound();

        float t = 0f;
        while (t < jumpUpTime)
        {
            t += Time.deltaTime;
            float p = t / jumpUpTime;

            transform.position = Vector3.Lerp(startPos, airPos, p);

            yield return null;
        }

        transform.position = airPos;

        Vector3 lockedLandPos = currentTarget.position;

        if (jumpIndicator != null)
        {
            jumpIndicator.SetActive(true);
            jumpIndicator.transform.position = lockedLandPos + Vector3.up * 0.02f;
            jumpIndicator.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            jumpIndicator.transform.localScale = Vector3.one * jumpDamageRange * 2f;
        }

        float followTimer = 0f;

        while (followTimer < indicatorFollowTime)
        {
            followTimer += Time.deltaTime;

            if (currentTarget != null)
            {
                lockedLandPos = currentTarget.position;

                transform.position = new Vector3(
                    lockedLandPos.x,
                    airPos.y,
                    lockedLandPos.z
                );
            }

            if (jumpIndicator != null)
                jumpIndicator.transform.position = lockedLandPos + Vector3.up * 0.02f;

            yield return null;
        }

        yield return new WaitForSeconds(indicatorStopTime);

        if (jumpIndicator != null)
            jumpIndicator.transform.position = lockedLandPos + Vector3.up * 0.02f;

        animator.SetTrigger("Fall");
        PlayBossBoneSpiderJumpDownSound();

        Vector3 fallStartPos = transform.position;
        Vector3 fallEndPos = new Vector3(
            lockedLandPos.x,
            startPos.y,
            lockedLandPos.z
        );

        t = 0f;
        while (t < fallTime)
        {
            t += Time.deltaTime;
            float p = t / fallTime;

            transform.position = Vector3.Lerp(fallStartPos, fallEndPos, p);

            yield return null;
        }

        transform.position = fallEndPos;

        if (jumpIndicator != null)
            jumpIndicator.SetActive(false);

        DealJumpDamage(fallEndPos);

        jumpSkillTimer = jumpSkillCooldown;
        attackTimer = attackCooldown;
        doubleStompTimer = doubleStompCooldown;

        yield return StartCoroutine(Recover(2f));

        isSkillAttacking = false;
        isAttacking = false;

        agent.Warp(transform.position);
        agent.updatePosition = true;
        agent.updateRotation = true;

        ResumeAgent();
    }

    void DealJumpDamage(Vector3 center)
    {
        Collider[] hits = Physics.OverlapSphere(
            center,
            jumpDamageRange,
            targetLayer
        );

        foreach (var hit in hits)
        {
            Actor actor = hit.GetComponent<Actor>();

            if (actor == null || actor == this)
                continue;

            actor.TakeDamage(jumpDamage, 1f);
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

    public void OnDoubleStompHit()
    {
        DealDoubleStompDamage();
    }

    public void PlayBossBoneSpiderAttackSound()
    {
        SoundManager.Instance.PlaySound("BossBoneSpiderAttack");
    }

    public void PlayBossBoneSpiderJumpDownSound()
    {
        SoundManager.Instance.PlaySound("BossBoneSpiderDown");
    }

    public void PlayBossBoneSpiderDieSound()
    {
        SoundManager.Instance.PlaySound("BossBoneSpiderDie");
    }

    public void PlayBossBoneSpiderJumpUpSound()
    {
        SoundManager.Instance.PlaySound("BossBoneSpiderUp"); 
    }

    public void PlayBossBoneRoarSound()
    {
        SoundManager.Instance.PlaySound("BossBothSpiderRoar");
    }

    public void PlayBossSpiderStompSound()
    {
        SoundManager.Instance.PlaySound("EliteSpiderStomp");
    }
}


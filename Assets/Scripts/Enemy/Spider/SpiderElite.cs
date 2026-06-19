using System.Collections;
using UnityEngine;

public class SpiderElite : Enemy
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

    [Header("회전 설정")]
    public float turnSpeed = 8f;
    public float attackStartAngle = 8f;
    public float maxTurnTime = 1.0f;

    private bool isRandomAttacking = false;
    private bool isSkillAttacking = false;
    private bool isRecovering = false;

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
    }

    protected override void Update()
    {
        base.Update();

        if (_isDead) return;

        attackTimer -= Time.deltaTime;
        doubleStompTimer -= Time.deltaTime;
    }

    protected override void TryAttack()
    {
        if (currentTarget == null) return;

        if (isAttacking || isRandomAttacking || isSkillAttacking || isRecovering)
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

        PlaySpiderAttackSound();

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
        return isRecovering || isSkillAttacking;
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

    public void PlaySpiderAttackSound()
    {
        SoundManager.Instance.PlaySound("MinionSpiderAttack");
    }


    public void PlayEliteSpiderStompSound()
    {
        SoundManager.Instance.PlaySound("EliteSpiderStomp");
    }

}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Actor
{
    WaveManager manager;
    EnemyData data;

    [Header("이동")]
    public NavMeshAgent agent;

    [Header("타겟 설정")]
    public LayerMask aiLayer;
    public Transform currentTarget;

    [Header("분리(겹침 방지)")]
    public float separationRadius = 2f;
    public float separationForce = 2f;

    [Header("포위 설정")]
    public float surroundRadius = 2f;

    [SerializeField] float stunMarkDuration = 2f; 
    public TextMeshPro stunText;
    public TextMeshPro tauntText;

    //기절변수들
    bool isStunned = false;
    float stunTimer = 0f;

    //변수들 선언
    public LayerMask enemyLayer;   
    Transform player;
    Transform forcedTarget;
    float forcedTimer = 0f;
    float detectRadius;
    float patrolRadius;
    float patrolWaitTime;
    float patrolSpeed;
    float chaseSpeed;
    float waitTimer = 0f;
    float attackRange;
    float attackCooldown;
    int attackDamage;

    float attackTimer = 0f;


    //EnemyData에서 가져온 수치
    public void Init(WaveManager manager, EnemyData data)
    {
        this.manager = manager;
        this.data = data;

        InitActor(data.enemyHp);

        patrolSpeed = data.patrolSpeed;
        chaseSpeed = data.chaseSpeed;
        detectRadius = data.detectRadius;
        patrolRadius = data.patrolRadius;
        attackDamage = data.attackDamage;
        attackRange = data.attackRange;
        attackCooldown = data.attackCooldown;
        patrolWaitTime = data.patrolWaitTime;
        if (agent != null)
            agent.speed = patrolSpeed;

        Debug.Log($"[Enemy] patrolRadius={patrolRadius}, detectRadius={detectRadius}");
    }
    //어웨이크

    protected override void Awake()
    {
        base.Awake(); 

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }


    //업데이트 부분
    void Update()
    {
        if (_isDead) return;

        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
                EndStun();
            return;
        }

        HandleForcedTarget();
        HandleMovement();
        TryAttack();
    }

    //도발 걸린 상태
    void HandleForcedTarget()
    {
        if (forcedTarget == null) return;

        if (float.IsInfinity(forcedTimer))
        {
            currentTarget = forcedTarget;
            return;
        }

        forcedTimer -= Time.deltaTime;
        if (forcedTimer > 0)
            currentTarget = forcedTarget;
        else
            forcedTarget = null;
    }

    //-----------누굴 따라갈것인가?-----------
    void HandleMovement()
    {
        if (player == null) return;

        if (forcedTarget != null)
        {
            currentTarget = forcedTarget;
            ChasePlayer(Vector3.Distance(transform.position, currentTarget.position));
            return;
        }

        Transform bestTarget = GetBestTarget();

        float dist = Vector3.Distance(transform.position, bestTarget.position);

        if (dist <= detectRadius)
        {
            currentTarget = bestTarget;
            ChasePlayer(dist);
        }
        else
        {
            currentTarget = null;
            RandomPatrol();
        }
    }
    //---------어떤것이 더 적합한 타겟인가?------------
    Transform GetBestTarget()
    {
        Transform best = player;
        float bestDist = Vector3.Distance(transform.position, player.position);

        // 주변 AI 검색
        Collider[] allies = Physics.OverlapSphere(transform.position, detectRadius, aiLayer);

        foreach (var a in allies)
        {
            float d = Vector3.Distance(transform.position, a.transform.position);

            if (d < bestDist)
            {
                best = a.transform;
                bestDist = d;
            }
        }

        return best;
    }

    // ---------- 추적 ----------
    void ChasePlayer(float dist)
    {
        agent.isStopped = false;
        agent.speed = chaseSpeed;

        Vector3 surroundPos = GetSurroundPosition(currentTarget);

        // 겹침 방지 적용
        surroundPos = ApplySeparation(surroundPos);

        agent.SetDestination(surroundPos);

        SetWalking(true);
    }

    // ---------- 랜덤 순찰 ----------
    void RandomPatrol()
    {
        agent.isStopped = false;
        agent.speed = patrolSpeed;  

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= patrolWaitTime)
            {
                Vector3 newPos;
                if (GetRandomPoint(transform.position, patrolRadius, out newPos))
                {
                    agent.SetDestination(newPos);
                }

                waitTimer = 0f;
            }
        }

        SetWalking(true);
    }
    
    //----------랜덤좌표값---------
    bool GetRandomPoint(Vector3 center, float radius, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPos = center + Random.insideUnitSphere * radius;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPos, out hit, 2f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = center;
        return false;
    }

    //--------원형으로 둥글게 공격---------
    Vector3 GetSurroundPosition(Transform target)
    {
        Collider[] others = Physics.OverlapSphere(
            target.position,
            5f,
            enemyLayer
        );

        int myIndex = 0;
        for (int i = 0; i < others.Length; i++)
        {
            if (others[i].transform == transform)
                myIndex = i;
        }

        float angle = (360f / Mathf.Max(1, others.Length)) * myIndex;

        Vector3 dir = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            0,
            Mathf.Sin(angle * Mathf.Deg2Rad)
        );

        return target.position + dir * surroundRadius;
    }

    //----------안 겹치게-----------
    Vector3 ApplySeparation(Vector3 desiredPos)
    {
        Collider[] allies = Physics.OverlapSphere(
            transform.position,
            separationRadius,
            enemyLayer
        );

        Vector3 push = Vector3.zero;

        foreach (var a in allies)
        {
            if (a.transform == transform) continue;

            Vector3 dir = transform.position - a.transform.position;
            float dist = dir.magnitude;

            if (dist > 0.01f)
                push += dir.normalized / dist;   // 가까우면 더 많이 밀어냄
        }

        desiredPos += push * separationForce;
        return desiredPos;
    }

    //----------도발 타겟 고정-------------
    public void ForceTarget(Transform t, float duration)
    {
        forcedTarget = t;
        forcedTimer = duration;
    }

    //-----------도발당함----------------
    public void ShowTauntMark(float duration)
    {
        if (tauntText == null) return;
        if (isStunned) return;   // ⭐ 핵심

        tauntText.gameObject.SetActive(true);
        tauntText.text = "!";

        StartCoroutine(HideTauntText(duration));
    }

    IEnumerator HideTauntText(float time)
    {
        yield return new WaitForSeconds(time);
        tauntText.gameObject.SetActive(false);
    }

    //스턴을 당했나?
    public void ApplyStun(float duration)
    {
        if (isStunned) return;

        isStunned = true;
        stunTimer = duration;

        ShowStunMark();
    }

    //스턴 마크 보여주기
    void ShowStunMark()
    {
        if (stunText == null) return;

        stunText.gameObject.SetActive(true);
        stunText.text = "@";

        StopCoroutine(nameof(HideStunMark));
        StartCoroutine(HideStunMark());
    }

    //스턴 마크 숨기기
    IEnumerator HideStunMark()
    {
        yield return new WaitForSeconds(stunMarkDuration);
        stunText.gameObject.SetActive(false);
    }

    //스턴이 끝난 시점
    void EndStun()
    {
        isStunned = false;

        agent.isStopped = false;
        animator?.SetBool("Stun", false);

        stunText.gameObject.SetActive(false);

        Debug.Log($"[Enemy] {name} STUN END");
    }

    // ---------- 애니메이션 ----------
    void SetWalking(bool walking)
    {
    }


    //적 죽음
    protected override void Die()
    {
        if (_isDead) return;

        base.Die();

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (animator != null)
            animator.SetTrigger("Die");

        manager?.OnEnemyDead();

        Destroy(gameObject);
    }
    //공격

    //데미지 받기
    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

        if (!_isDead)
            animator?.SetTrigger("Hit");
    }
    protected virtual void TryAttack()
    {
        if (currentTarget == null) return;

        attackTimer -= Time.deltaTime;
        if (attackTimer > 0f) return;

        float dist = Vector3.Distance(transform.position, currentTarget.position);
        if (dist > attackRange) return;

        Attack();
    }

    void Attack()
    {
        attackTimer = attackCooldown;

        Actor target = currentTarget.GetComponent<Actor>();
        if (target != null)
        {
            Debug.Log($"[Enemy Attack] {name} dmg={attackDamage}");
            target.TakeDamage(attackDamage);
        }
    }
}

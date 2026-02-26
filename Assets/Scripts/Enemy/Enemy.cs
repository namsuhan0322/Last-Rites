
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
    public bool isHit = false;
    public float attackRange;
    public float attackCooldown;
    public int attackDamage;
    public float attackTimer = 0f;
    protected float actionLockTimer = 0f;
    protected bool isAttacking = false;


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
        agent.stoppingDistance = attackRange;
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
    protected override void Update()
    {
        base.Update();

        if (_isDead || isHit) return;

        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
                EndStun();
            return;
        }

        if (isAttacking) return;   

        HandleForcedTarget();
        HandleMovement();
        TryAttack();

        if (agent.velocity.magnitude < 0.1f)
        {
            animator?.SetBool("Walk", false);
            animator?.SetBool("Run", false);
        }

        if (actionLockTimer > 0f)
        {
            actionLockTimer -= Time.deltaTime;
            return;
        }

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
        agent.speed = chaseSpeed;
        agent.isStopped = false;
        agent.SetDestination(currentTarget.position);

        RotateToTarget();

        animator.SetBool("Walk", false);
        animator.SetBool("Run", true);
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
        animator.SetBool("Walk", true);
        animator.SetBool("Run", false);
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
        animator?.SetBool("Stun", true);
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

    //적 죽음
    protected override void Die()
    {
        if (_isDead) return;

        _isDead = true;

        isHit = false;
        animator.ResetTrigger("Hit");

        base.Die(); 

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        manager?.OnEnemyDead();

        StartCoroutine(DieRoutine());
    }

    IEnumerator DieRoutine()
    {
        yield return new WaitForSeconds(2f);

        if (agent != null)
            agent.enabled = false;

        Destroy(gameObject);
    }

    //데미지 받기
    public override void TakeDamage(int damage)
    {
        if (_isDead) return;

        base.TakeDamage(damage);

        if (_isDead) return; 

        isHit = true;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
            agent.isStopped = true;

        animator?.SetTrigger("Hit");
    }

    //공격 시도
    protected virtual void TryAttack()
    {
        if (currentTarget == null) return;

        attackTimer -= Time.deltaTime;
        if (attackTimer > 0f) return;

        float dist = Vector3.Distance(transform.position, currentTarget.position);
        if (dist > attackRange) return;

        Attack();
    }

    //공격 변수
    void Attack()
    {
        RotateToTarget();
        attackTimer = attackCooldown;

    }

    //데미지 이벤트 함수
    public void DealDamage()
    {
        if (currentTarget == null) return;

        Actor target = currentTarget.GetComponent<Actor>();
        if (target != null)
            target.TakeDamage(attackDamage);
    }

    //타겟 쳐다보기
    public void RotateToTarget()
    {
        if (currentTarget == null) return;

        Vector3 dir = currentTarget.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 10f);
    }

    //피격 끝 시점
    public void EndHit()
    {
        if (_isDead) return;

        isHit = false;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
            agent.isStopped = false;
    }
}


using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Actor
{
    WaveManager manager;
   public EnemyData data;

    [Header("이동")]
    public NavMeshAgent agent;

    [Header("타겟 설정")]
    public LayerMask aiLayer;
    public Transform currentTarget;

    [Header("포위 설정")]
    public float surroundRadius = 2f;

    [Header("어그로락")]
    public float aggroLockDuration = 3f;

    float targetLockTimer = 0f;

    float aggroTimer = 0f;

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
    Transform lastTarget;

    //랭크표시 편하게
    public EnemyRank Rank => data.rank;

    //엘리트나 보스인가?
    bool IsEliteOrBoss()
    {
        if (data == null) return false;
        return data.rank == EnemyRank.Elite || data.rank == EnemyRank.Boss;
    }

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

    protected override void Start()
    {
        base.Start();

        OnStun += HandleStun;
    }

    protected override void Awake()
    {
        base.Awake();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = true; 

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

        if (forcedTimer > 0)
        {
            if (forcedTimer <= 0)
                forcedTarget = null;
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
        {
            currentTarget = forcedTarget;
        }
        else
        {
            forcedTarget = null;

            if (IsEliteOrBoss() && player != null)
            {
                currentTarget = player;
                agent.SetDestination(player.position); 
            }
            else
            {
                currentTarget = null;
            }
        }
    }

    //-----------누굴 따라갈것인가?-----------
    void HandleMovement()
    {
        if (player == null) return;

        if (forcedTarget != null)
        {
            currentTarget = forcedTarget;

            if (currentTarget == null) return; 

            ChasePlayer(Vector3.Distance(transform.position, currentTarget.position));
            return;
        }

        if (currentTarget == null)
        {
            currentTarget = GetBestTargetExcept(null);
            aggroTimer = aggroLockDuration;

            if (currentTarget == null)  
            {
                RandomPatrol();
                return;
            }
        }
        else
        {
            aggroTimer -= Time.deltaTime;

            if (aggroTimer <= 0f)
            {
                lastTarget = currentTarget;

                Transform newTarget = GetBestTargetExcept(currentTarget);

                if (newTarget != null)
                    currentTarget = newTarget;

                aggroTimer = aggroLockDuration;
            }
        }

        if (currentTarget == null)
        {
            RandomPatrol();
            return;
        }

        float dist = Vector3.Distance(transform.position, currentTarget.position);

        if (dist <= detectRadius)
            ChasePlayer(dist);
        else
        {
            currentTarget = null;
            RandomPatrol();
        }
    }
    //---------어떤것이 더 적합한 타겟인가?------------
    Transform GetBestTargetExcept(Transform except)
    {
        Transform best = null;
        float bestDist = float.MaxValue;

        Collider[] allies = Physics.OverlapSphere(transform.position, detectRadius, aiLayer);

        if (IsEliteOrBoss())
        {
            if (player != null)
            {
                float d = Vector3.Distance(transform.position, player.position);

                if (d < detectRadius)
                    return player;
            }

            foreach (var a in allies)
            {
                if (a.transform == except) continue;

                float d = Vector3.Distance(transform.position, a.transform.position);

                if (d < bestDist)
                {
                    best = a.transform;
                    bestDist = d;
                }
            }

            return best;
        }

        if (player != null && player != except)
        {
            float d = Vector3.Distance(transform.position, player.position);

            if (d < detectRadius)
            {
                best = player;
                bestDist = d;
            }
        }

        foreach (var a in allies)
        {
            if (a.transform == except) continue;

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
        agent.updateRotation = false;

        if (dist <= attackRange)
        {
            agent.isStopped = true;
            RotateToTarget();
            return;
        }

        agent.speed = chaseSpeed;
        agent.isStopped = false;

        if (agent.destination != currentTarget.position)
            agent.SetDestination(currentTarget.position);

        RotateToTarget();

        animator.SetBool("Walk", false);
        animator.SetBool("Run", true);
    }

    // ---------- 랜덤 순찰 ----------
    void RandomPatrol()
    {
        agent.updateRotation = true; 

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
        if (isStunned) return;

        StopCoroutine(nameof(HideStunMark));
        if (stunText != null)
            stunText.gameObject.SetActive(false);

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
        if (data != null && (data.rank == EnemyRank.Elite || data.rank == EnemyRank.Boss))
            return;

        if (isStunned) return;

        isStunned = true;
        stunTimer = duration;

        agent.isStopped = true; 

        animator?.SetBool("Stun", true);
        ShowStunMark();
    }

    //스턴 마크 보여주기
    void ShowStunMark()
    {
        if (stunText == null) return;

        StopCoroutine(nameof(HideTauntText));
        if (tauntText != null)
            tauntText.gameObject.SetActive(false);

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

        isStunned = false;
        stunTimer = 0f;

        animator?.SetBool("Stun", false);

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
    public override void TakeDamage(int damage, float severityOverride = -1f)
    {
        if (_isDead) return;

        base.TakeDamage(damage, severityOverride);

        if (_isDead) return;

        if (isStunned) return;

        isHit = true;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
            agent.isStopped = true;

        animator?.SetTrigger("Hit");
    }

    //공격 시도
    protected virtual void TryAttack()
    {
        if (currentTarget == null) return;
        if (isAttacking) return;

        attackTimer -= Time.deltaTime;
        if (attackTimer > 0f) return;

        float dist = Vector3.Distance(transform.position, currentTarget.position);
        if (dist > attackRange) return;

        Attack();
    }

    //공격 변수
    protected virtual void Attack()
    {
        isAttacking = true;

        agent.isStopped = true;   
        agent.velocity = Vector3.zero;

        RotateToTarget();

        attackTimer = attackCooldown;
    }
    //공격 끝남
    public void EndAttack()
    {
        isAttacking = false;

        if (!_isDead && !isStunned)
            agent.isStopped = false;
    }

    //데미지 이벤트 함수
    public void DealDamage()
    {
        if (currentTarget == null) return;

        Actor target = currentTarget.GetComponent<Actor>();
        if (target != null)
        {
            float hitSeverity = (data.rank == EnemyRank.Minion) ? 0f : 1.0f;
            target.TakeDamage(attackDamage, hitSeverity);
        }       
    }

    //타겟 쳐다보기
    public void RotateToTarget()
    {
        if (currentTarget == null) return;

        Vector3 dir = currentTarget.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 15f);
    }

    //피격 끝 시점
    public void EndHit()
    {
        if (_isDead) return;

        isHit = false;

        if (!isStunned && agent != null && agent.enabled && agent.isOnNavMesh)
            agent.isStopped = false;
    }

    void HandleStun()
    {
        ApplyStun(2f); 
    }
}

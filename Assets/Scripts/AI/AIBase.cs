using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIBase : Actor
{
    [Header("이동")]
    public NavMeshAgent agent;

    [Header("플레이어 추적")]
    public float followRange = 6f;
    public float followTolerance = 1.5f;

    [Header("전투 탐지")]
    public float attackDetectRadius = 8f;
    public LayerMask enemyLayer;

    [Header("동료 탐지")]
    public LayerMask aiLayer;

    [Header("분리(겹침 방지)")]
    public float separationRadius = 2f;
    public float separationForce = 2f;

    [Header("기본 공격")]
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float attackCooldown = 1.2f;

    protected Transform player;
    protected int atk;  // AISO에서 가져올 공격력
    bool canAttack = true;  //공격할 수 있나?
    protected AISO data;

    Transform targetEnemy;
    bool isChasingEnemy = false;

    protected override void Awake()
    {
        base.Awake();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    //Actor에서 가져오기
    public virtual void Setup(AISO aiData)
    {
        data = aiData;

        atk = data.Atk;         
        InitActor(data.Hp);
    }

    protected virtual void Update()
    {
        if (_isDead) return;
        if (player == null) return;

        DetectEnemy();

        if (isChasingEnemy)
        {
            ChaseEnemy();
            TryBasicAttack();   
        }
        else
            HandleFollow();
    }

    //기본 공격
    void TryBasicAttack()
    {
        if (!canAttack) return;
        if (targetEnemy == null) return;

        float dist = Vector3.Distance(transform.position, targetEnemy.position);
        if (dist > attackRange) return;

        StartCoroutine(BasicAttack());
    }

    IEnumerator BasicAttack()
    {
        canAttack = false;

        agent.isStopped = true;
        SetWalking(false);

        LookAtEnemy();

        // 공격 모션 타이밍
        yield return new WaitForSeconds(0.3f);

        if (targetEnemy != null)
        {
            Enemy enemy = targetEnemy.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(atk);
                OnAttackHit(enemy);   
            }
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    protected virtual void OnAttackHit(Enemy enemy) //직업별 확장성
    {

    }

    // --------- 적 탐지 ----------
    protected virtual void DetectEnemy()
    {
        // 이미 전투 중이면 새로 찾지 않음
        if (isChasingEnemy) return;

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            attackDetectRadius,
            enemyLayer
        );

        if (hits.Length == 0) return;

        float minDist = float.MaxValue;
        Transform closest = null;

        foreach (var h in hits)
        {
            float d = Vector3.Distance(transform.position, h.transform.position);
            if (d < minDist)
            {
                minDist = d;
                closest = h.transform;
            }
        }

        targetEnemy = closest;
        isChasingEnemy = true;     
    }

    //적 쫒아가기
    void ChaseEnemy()
    {
        if (targetEnemy == null)
        {
            isChasingEnemy = false;
            agent.isStopped = true;
            return;
        }

        float dist = Vector3.Distance(transform.position, targetEnemy.position);

        if (dist > attackDetectRadius * 1.5f)
        {
            targetEnemy = null;
            isChasingEnemy = false;
            return;
        }

        if (dist <= attackRange)
        {
            agent.isStopped = true;
            SetWalking(false);
            LookAtEnemy();
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(targetEnemy.position);
        SetWalking(true);
    }

    // --------- 적 바라보기 ----------
    protected virtual void LookAtEnemy()
    {
        if (targetEnemy == null) return;

        Vector3 dir = targetEnemy.position - transform.position;
        dir.y = 0f; 

        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                Time.deltaTime * 8f
            );
    }

    // -------- 플레이어 따라가기 ----------
    protected virtual void HandleFollow()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > followRange + followTolerance)
        {
            agent.isStopped = false;
            agent.SetDestination(
                ApplySeparation(player.position)
            );
            SetWalking(true);
        }
        else if (dist < followRange - followTolerance)
        {
            agent.isStopped = true;
            SetWalking(false);
        }
    }

    //----------서로 밀어내기------------
    Vector3 ApplySeparation(Vector3 desiredPos)
    {
        Collider[] allies = Physics.OverlapSphere(
            transform.position,
            separationRadius,
            aiLayer
        );

        Vector3 push = Vector3.zero;

        foreach (var a in allies)
        {
            if (a.transform == transform) continue;

            Vector3 dir = transform.position - a.transform.position;
            float dist = dir.magnitude;

            if (dist > 0.01f)
                push += dir.normalized / dist;   // 가까울수록 더 밀림
        }

        desiredPos += push * separationForce;

        return desiredPos;
    }

    //죽음 
    protected override void Die()
    {
        if (_isDead) return;

        base.Die();

        agent.isStopped = true;
        agent.enabled = false;

        animator?.SetTrigger("Die");

        foreach (var c in GetComponents<Collider>())
            c.enabled = false;

        Destroy(gameObject, 3f);  //애니메이터 초수 따라서
    }

    // -------- 애니메이션 훅 ----------
    protected virtual void SetWalking(bool walking) { }


    //기즈모 확인
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDetectRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

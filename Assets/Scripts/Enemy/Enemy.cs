using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    WaveManager manager;
    EnemyData data;

    [Header("스탯")]
    public int maxHp = 10;
    int hp;

    [Header("탐지")]
    public float detectRadius = 10f;

    [Header("순찰")]
    public float patrolRadius = 8f;     // 랜덤 탐색 반경
    public float patrolWaitTime = 1f;   // 도착 후 잠깐 대기
    float waitTimer = 0f;

    [Header("이동")]
    public NavMeshAgent agent;

    [Header("속도")]
    public float patrolSpeed = 2f;   // 순찰 속도
    public float chaseSpeed = 5f;    // 추적 속도

    [Header("타겟 설정")]
    public LayerMask aiLayer;
    Transform currentTarget;

    [Header("분리(겹침 방지)")]
    public float separationRadius = 2f;
    public float separationForce = 2f;

    [Header("포위 설정")]
    public float surroundRadius = 2f;


    //변수들 선언
    Transform lockedTarget;
    public LayerMask enemyLayer;   
    Transform player;
    Transform forcedTarget;
    float forcedTimer = 0f;

    public void Init(WaveManager manager, EnemyData data)
    {
        this.manager = manager;
        maxHp = data.Enemyhp;
        hp = maxHp;
    }

    void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (forcedTarget != null)
        {
            forcedTimer -= Time.deltaTime;

            if (forcedTimer > 0)
                currentTarget = forcedTarget;
            else
                forcedTarget = null;
        }

        HandleMovement();
    }


    //-----------누굴 따라갈것인가?-----------
    void HandleMovement()
    {
        if (player == null) return;

        if (forcedTarget != null)
        {
            currentTarget = forcedTarget;

            float dist = Vector3.Distance(transform.position, currentTarget.position);

            if (dist <= detectRadius)
                ChasePlayer(dist);
            else
                RandomPatrol();

            return; 
        }

        if (lockedTarget == null)
            lockedTarget = GetBestTarget();

        if (lockedTarget == null || Vector3.Distance(transform.position, lockedTarget.position) > detectRadius)
            lockedTarget = GetBestTarget();

        currentTarget = lockedTarget;

        float d = Vector3.Distance(transform.position, currentTarget.position);

        if (d <= detectRadius)
            ChasePlayer(d);
        else
            RandomPatrol();
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

    //----------타겟 고정-------------
    public void ForceTarget(Transform t, float duration)
    {
        forcedTarget = t;
        forcedTimer = duration;
    }

    // ---------- 애니메이션 ----------
    void SetWalking(bool walking)
    {
    }

    // ---------- 전투 ----------
    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        if (hp <= 0) Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (manager != null)
            manager.OnEnemyDead();
    }
}

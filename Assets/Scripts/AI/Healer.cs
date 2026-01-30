using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : AIBase
{
    [Header("힐 스킬")]
    [SerializeField] float healRadius = 6f;
    [SerializeField] float castDelay = 1f; // 힐 전 멈춤 시간
    AISpeechController speech;


    float healAmount;
    float healCooldown;
    float healTimer;

    Actor healTarget;

    bool isCasting;
    float castTimer;


    protected override void Awake()
    {
        base.Awake(); // AIBase 초기화 먼저

        speech = GetComponent<AISpeechController>();
    }


    // AISO에서 수정
    public override void Setup(AISO aiData)
    {
        base.Setup(aiData);

        healAmount = data.S1_Val;     
        healCooldown = data.S1_Cool;  
        healTimer = healCooldown;
    }

    protected override void Update()
    {
        if (_isDead) return;

        if (isCasting)
        {
            HandleCasting();
            return;
        }

        base.Update(); // 기본 이동 / 추적

        healTimer -= Time.deltaTime;

        if (healTimer <= 0f)
        {
            if (healTarget == null || healTarget.IsDead || healTarget.IsFullHP)
            {
                FindHealTarget();
            }

            if (healTarget != null)
            {
                MoveOrCast();
            }
        }
    }

    // ---------------- 힐 대상 찾기 ----------------
    void FindHealTarget()
    {
        Actor best = null;
        float lowestRatio = 1f;

        if (player != null)
            EvaluateTarget(player.GetComponent<Actor>(), ref best, ref lowestRatio);

        Collider[] allies = Physics.OverlapSphere(
            transform.position,
            healRadius * 2f,
            aiLayer
        );

        foreach (var c in allies)
            EvaluateTarget(c.GetComponent<Actor>(), ref best, ref lowestRatio);

        healTarget = best;
    }

    //타겟이 지금 현재 체력이 어느정도인가?
    void EvaluateTarget(Actor target, ref Actor best, ref float lowestRatio)
    {
        if (target == null) return;
        if (target.IsDead) return;
        if (target.IsFullHP) return;

        float ratio = (float)target.CurrentHP / target.MaxHP;
        if (ratio < lowestRatio)
        {
            lowestRatio = ratio;
            best = target;
        }

        Debug.Log($"{target.name} HP: {target.CurrentHP}/{target.MaxHP}");
    }

    // ---------------- 이동 or 캐스팅 ----------------
    void MoveOrCast()
    {
        float dist = Vector3.Distance(transform.position, healTarget.transform.position);

        if (dist > healRadius)
        {
            agent.isStopped = false;
            agent.SetDestination(healTarget.transform.position);
            SetWalking(true);
        }
        else
        {
            StartCasting();
        }
    }

    // ---------------- 캐스팅 ----------------
    void StartCasting()
    {
        agent.isStopped = true;
        SetWalking(false);

        isCasting = true;
        castTimer = castDelay;

        speech?.Speak("힐 드릴게요!", castDelay);
    }

    void HandleCasting()
    {
        castTimer -= Time.deltaTime;

        LookAtHealTarget(); 

        if (castTimer <= 0f)
        {
            CompleteHeal();
        }
    }

    // ---------------- 힐 완료 ----------------
    void CompleteHeal()
    {
        if (player != null)
        {
            Actor p = player.GetComponent<Actor>();
            if (p != null && !p.IsDead)
                p.Heal((int)healAmount);
        }

        Collider[] allies = Physics.OverlapSphere(
            transform.position,
            healRadius,
            aiLayer
        );

        foreach (var c in allies)
        {
            Actor ally = c.GetComponent<Actor>();
            if (ally == null || ally.IsDead) continue;

            ally.Heal((int)healAmount);
        }

        healTimer = healCooldown;
        healTarget = null;
        isCasting = false;

        agent.isStopped = false;
    }


    //캐스팅 취소
    void CancelCasting()
    {
        isCasting = false;
        speech?.Clear();
    }


    //힐하는 사람 바라보기
    void LookAtHealTarget()
    {
        Vector3 dir = healTarget.transform.position - transform.position;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                Time.deltaTime * 8f
            );
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, healRadius);
    }


}

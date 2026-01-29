using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wolf : Enemy
{
    [Header("근접 공격")]
    public float attackRange = 2f;
    public int attackDamage = 10;
    public float attackCooldown = 1.5f;

    float attackTimer = 0f;

    // 부모 공격 로직 덮어쓰기
    protected override void TryAttack()
    {
        if (currentTarget == null) return;

        float dist = Vector3.Distance(transform.position, currentTarget.position);

        attackTimer -= Time.deltaTime;

        if (dist <= attackRange && attackTimer <= 0f)
        {
            Attack();
        }
    }

    //공격 변수 나중에 애니메이터 추가
    void Attack()
    {
        attackTimer = attackCooldown;

        // 방향 체크 (뒤에 있으면 공격 안 함)
        Vector3 dirToTarget = (currentTarget.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, dirToTarget);
        if (dot < 0.5f) return;

        // 사거리 체크
        float dist = Vector3.Distance(transform.position, currentTarget.position);
        if (dist > attackRange) return;

        // AI 공격
        AIBase ai = currentTarget.GetComponent<AIBase>();
        if (ai != null)
        {
            ai.TakeDamage(attackDamage);
            return;
        }

        // 플레이어 공격
        Actor player = currentTarget.GetComponent<Actor>();
        if (player != null)
        {
            player.TakeDamage(attackDamage);
        }
    }
}

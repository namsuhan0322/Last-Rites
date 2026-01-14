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
    bool isAttacking = false;

    //부모 스크립트 덮어씌우기
    protected override void TryAttack()
    {
        if (currentTarget == null) return;

        float dist = Vector3.Distance(transform.position, currentTarget.position);

        attackTimer -= Time.deltaTime;

        if (dist <= attackRange && attackTimer <= 0f && !isAttacking)
        {
            Attack();
        }
    }

    //공격 변수
    void Attack()
    {
        isAttacking = true;
        attackTimer = attackCooldown;

        // 애니메이션 트리거
        GetComponent<Animator>().SetTrigger("Attack");
    }

    //늑대공격 애니메이션 시작 부분 ㄴ이벤트쪽
    public void OnAttackHit()
    {
        if (currentTarget == null) return;

        Vector3 dirToTarget = (currentTarget.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, dirToTarget);

        if (dot < 0.5f) return;

        float dist = Vector3.Distance(transform.position, currentTarget.position);
        if (dist > attackRange) return;

        //AI쪽 데미지 주기
        AIBase ai = currentTarget.GetComponent<AIBase>();
        if (ai != null)
        {
            ai.TakeDamage(attackDamage);
            return;
        }

        // 플레이어면 플레이어 체력
        Actor player = currentTarget.GetComponent<Actor>();
        if (player != null)
        {
            player.TakeDamage(attackDamage);
        }
    }

    // 애니메이션 끝에서 호출
    public void OnAttackEnd()
    {
        isAttacking = false;
    }
}

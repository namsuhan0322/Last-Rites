using System.Collections;
using UnityEngine;

public class SpiderMinion : Enemy
{
    [Header("랜덤 공격 설정")]
    public int attackPatternCount = 3;
    public float attackAnimTime = 1.0f;
    public float postAttackDelay = 1.5f;

    bool isRandomAttacking = false;

    protected override void TryAttack()
    {
        if (currentTarget == null) return;
        if (isRandomAttacking) return;
        if (isAttacking) return;

        attackTimer -= Time.deltaTime;
        if (attackTimer > 0f) return;

        float dist = Vector3.Distance(transform.position, currentTarget.position);
        if (dist > attackRange) return;

        StartCoroutine(RandomAttackRoutine());
    }

    IEnumerator RandomAttackRoutine()
    {
        isAttacking = true;
        isRandomAttacking = true;

        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
        attackTimer = attackCooldown;
        Vector3 dir = currentTarget.position - transform.position;
        dir.y = 0f;

        attackDirection = dir.normalized;

        if (attackDirection.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(attackDirection);

        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);

        int randomAttackIndex = Random.Range(1, attackPatternCount + 1);

        animator.SetTrigger("Attack" + randomAttackIndex);

        yield return new WaitForSeconds(attackAnimTime);

        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);

        yield return new WaitForSeconds(postAttackDelay);

        isRandomAttacking = false;
        isAttacking = false;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
            agent.isStopped = false;
    }

    public override void EndAttack()
    {
        if (isRandomAttacking)
        {
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }

            return;
        }

        base.EndAttack();
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfMinion : Enemy
{
    [Header("ÄÞº¸ ¼³Á¤")]
    public int comboCount = 3;
    public float comboInterval = 0.4f;
    public float comboCooldown = 2f;

    bool isComboAttacking = false;

    protected override void TryAttack()
    {
        if (currentTarget == null) return;
        if (isComboAttacking) return;

        attackTimer -= Time.deltaTime;
        if (attackTimer > 0f) return;

        float dist = Vector3.Distance(transform.position, currentTarget.position);
        if (dist > attackRange) return;

        StartCoroutine(ComboAttack());
    }

    IEnumerator ComboAttack()
    {
        isComboAttacking = true;
        agent.isStopped = true;

        for (int i = 0; i < comboCount; i++)
        {
            RotateToTarget();

            animator.SetTrigger("Attack" + (i + 1));
            yield return new WaitForSeconds(comboInterval);
        }

        agent.isStopped = false;
        isComboAttacking = false;
        attackTimer = comboCooldown;
    }
}

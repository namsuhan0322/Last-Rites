using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfMinion : Enemy
{
    [Header("콤보 설정")]
    public int comboCount = 3;
    public float comboInterval = 0.4f;
    public float comboCooldown = 2f;

    bool isComboAttacking = false;


    //공격 시도
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


    //콤보 어택
    IEnumerator ComboAttack()
    {
        isAttacking = true;
        isComboAttacking = true;
        agent.isStopped = true;

        attackTimer = comboCooldown;   

        RotateToTarget();

        for (int i = 0; i < comboCount; i++)
        {
            animator.SetTrigger("Attack" + (i + 1));
            yield return new WaitForSeconds(comboInterval);
        }

        yield return new WaitForSeconds(0.5f);

        isAttacking = false;
        isComboAttacking = false;
    }
}

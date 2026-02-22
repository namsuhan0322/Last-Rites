using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum WolfType
{
    Minion,
    Elite,
    Boss
}
public class Wolf : Enemy
{
    public WolfType type;

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
        isComboAttacking = true;
        agent.isStopped = true;

        for (int i = 0; i < comboCount; i++)
        {
            if (currentTarget == null) break;

            RotateToTarget();

            string animName = "Attack" + (i + 1);
            animator.SetTrigger(animName);

            yield return null;

            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(state.length);
        }

        agent.isStopped = false;
        isComboAttacking = false;

        attackTimer = comboCooldown; 
    }


}


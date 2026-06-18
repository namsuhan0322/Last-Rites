using System.Collections;
using UnityEngine;

public class WolfMinion : Enemy
{
    [Header("콤보 설정")]
    public int comboCount = 3;
    public float comboInterval = 0.4f;
    public float comboCooldown = 2f;
    public float postAttackDelay = 1.5f;

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
        isAttacking = true;
        isComboAttacking = true;

        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;

        attackTimer = comboCooldown;

        Vector3 dir = currentTarget.position - transform.position;
        dir.y = 0f;

        attackDirection = dir.normalized;

        if (attackDirection.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(attackDirection);

        for (int i = 0; i < comboCount; i++)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;

            animator.SetBool("Walk", false);
            animator.SetBool("Run", false);
            animator.SetTrigger("Attack" + (i + 1));

            yield return new WaitForSeconds(comboInterval);
        }

        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);

        yield return new WaitForSeconds(postAttackDelay);

        isComboAttacking = false;
        isAttacking = false;

        agent.isStopped = false;
    }

    public override void EndAttack()
    {
        if (isComboAttacking)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            return;
        }

        base.EndAttack();
    }

    public void PlayWolfAttackSound()
    {
        SoundManager.Instance.PlaySound("MinionWolfAttack");
    }
}
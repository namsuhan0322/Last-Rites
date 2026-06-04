using System.Collections;
using UnityEngine;

public class SuicideBombEnemy : Enemy
{
    [Header("자폭 설정")]
    public float touchDistance = 1.5f;
    public float runSpeedMultiplier = 1.5f;

    [Header("폭발 설정")]
    public GameObject explosionIndicatorPrefab;
    public float explosionReadyTime = 1.5f;
    public float explosionRange = 4f;
    public int explosionDamage = 30;

    private GameObject explosionIndicator;
    private bool isExploding = false;
    private Transform target;

    protected override void Start()
    {
        base.Start();

        target = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = chaseSpeed * runSpeedMultiplier;
        }

        animator.SetBool("Run", true);
    }

    protected override void EnemyAIUpdate()
    {
        if (_isDead || isExploding)
            return;

        if (target == null)
            return;

        float dist = Vector3.Distance(transform.position, target.position);

        if (dist <= touchDistance)
        {
            StartCoroutine(ExplosionRoutine());
            return;
        }

        ChaseToTarget();
    }

    void ChaseToTarget()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        agent.isStopped = false;
        agent.speed = chaseSpeed * runSpeedMultiplier;
        agent.SetDestination(target.position);

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);

        animator.SetBool("Run", true);
    }

    IEnumerator ExplosionRoutine()
    {
        isExploding = true;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        animator.SetBool("Run", false);
        animator.SetTrigger("Die");

        if (explosionIndicatorPrefab != null)
        {
            explosionIndicator = Instantiate(
                explosionIndicatorPrefab,
                transform.position + Vector3.up * 0.02f,
                Quaternion.Euler(90f, 0f, 0f)
            );

            explosionIndicator.transform.localScale = Vector3.zero;
        }

        float timer = 0f;

        while (timer < explosionReadyTime)
        {
            timer += Time.deltaTime;
            float p = timer / explosionReadyTime;

            if (explosionIndicator != null)
            {
                explosionIndicator.transform.position = transform.position + Vector3.up * 0.02f;
                explosionIndicator.transform.localScale =
                    Vector3.Lerp(Vector3.zero, Vector3.one * explosionRange * 2f, p);
            }

            yield return null;
        }

        DealExplosionDamage();

        if (explosionIndicator != null)
            Destroy(explosionIndicator);

        yield return new WaitForSeconds(0.3f);

        Die();
    }

    void DealExplosionDamage()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            explosionRange,
            targetLayer
        );

        foreach (var hit in hits)
        {
            Actor actor = hit.GetComponent<Actor>();

            if (actor == null || actor == this)
                continue;

            actor.TakeDamage(explosionDamage, 1f);
        }
    }

    protected override void TryAttack()
    {
        // 자폭병은 일반 공격 안 함
    }
}

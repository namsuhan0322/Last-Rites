using UnityEngine;
using System.Collections;

public class BlackholeSkillEffect : MonoBehaviour
{
    [Header("블랙홀 설정")]
    [Tooltip("터지기 전까지 걸리는 시간 (모으는 시간)")]
    public float explosionDelay = 2.0f;
    [Tooltip("적을 중앙으로 끌어당기는 범위")]
    public float pullRadius = 6f;
    [Tooltip("적을 끌어당기는 힘(속도)")]
    public float pullForce = 3f;

    [Header("폭발(데미지) 설정")]
    [Tooltip("폭발 데미지가 들어가는 최종 범위")]
    public float damageRadius = 4f;
    [Tooltip("이펙트 전체 유지 시간")]
    public float lifeTime = 4f;

    private int _damage;
    private LayerMask _enemyLayer;
    private bool _isExploded = false;

    public void Initialize(int damage, LayerMask enemyLayer)
    {
        _damage = damage;
        _enemyLayer = enemyLayer;

        StartCoroutine(ExplosionCoroutine());
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // 터지기 전까지 매 프레임 주변 적들을 중앙으로 끌어당김!
        if (!_isExploded)
        {
            Collider[] targets = Physics.OverlapSphere(transform.position, pullRadius, _enemyLayer);
            foreach (Collider col in targets)
            {
                Actor enemy = col.GetComponentInParent<Actor>();
                if (enemy != null && !enemy.IsDead)
                {
                    // 적을 블랙홀 중심 방향으로 이동시킴 (y축은 0으로 고정해 땅에 붙어있게 함)
                    Vector3 direction = (transform.position - enemy.transform.position).normalized;
                    direction.y = 0;

                    // 주의: 몬스터에 NavMeshAgent가 켜져 있으면 충돌할 수 있으니, 
                    // 덜덜거리면 몬스터의 NavMeshAgent.velocity를 건드리는 방식으로 응용할 수 있습니다.
                    enemy.transform.position += direction * pullForce * Time.deltaTime;
                }
            }
        }
    }

    private IEnumerator ExplosionCoroutine()
    {
        // 지정된 시간만큼 모으기 연출 대기
        yield return new WaitForSeconds(explosionDelay);

        _isExploded = true; // 끌어당기기 종료

        // 쾅! 폭발 데미지 판정
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, damageRadius, _enemyLayer);
        foreach (Collider enemyCollider in hitEnemies)
        {
            Actor enemy = enemyCollider.GetComponentInParent<Actor>();
            if (enemy != null && !enemy.IsDead)
            {
                enemy.TakeDamage(_damage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, pullRadius); // 파란 선: 끌어당기는 범위

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius); // 빨간 선: 터지는 범위
    }
}
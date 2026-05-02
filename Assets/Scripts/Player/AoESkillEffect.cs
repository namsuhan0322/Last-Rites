using UnityEngine;
using System.Collections;

public class AoESkillEffect : MonoBehaviour
{
    [Header("스킬 설정")]
    [Tooltip("이펙트가 생성되고 몇 초 뒤에 데미지를 줄 것인가? (검이 땅에 꽂히는 타이밍)")]
    public float damageDelay = 0.5f;
    [Tooltip("데미지가 들어가는 반경 (광역 범위)")]
    public float damageRadius = 3f;
    [Tooltip("이펙트 유지 시간")]
    public float lifeTime = 2f;

    private int _damage;
    private LayerMask _enemyLayer;

    public void Initialize(int damage, LayerMask enemyLayer)
    {
        _damage = damage;
        _enemyLayer = enemyLayer;

        StartCoroutine(DealDamageCoroutine());
        Destroy(gameObject, lifeTime);
    }

    private IEnumerator DealDamageCoroutine()
    {
        yield return new WaitForSeconds(damageDelay);

        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, damageRadius, _enemyLayer);

        foreach (Collider enemyCollider in hitEnemies)
        {
            Actor enemy = enemyCollider.GetComponent<Actor>();
            if (enemy != null)
            {
                enemy.TakeDamage(_damage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
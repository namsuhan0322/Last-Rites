using UnityEngine;
using System.Collections;

/// <summary>
/// [역할] 생성 후 damageDelay 뒤 반경 안 적에게 한 번 데미지 (엑스칼리버 V, 마법사 Wizard Q/W/V·BlackHole 프리팹).
///        한 번 터질 때를 공격 하나로 보고, 대상마다 한 번·약점이 있는 보스에게도 한 번만 준다(AttackHitResolver).
/// [이유] 생성하는 쪽이 ISkillDamageObject로 한 번에 찾는다. 예전에는 콜라이더 자신에게서만 적을 찾아
///        늑대 보스는 몸통+약점으로 두 번 맞고 드래곤 몸통(뼈 콜라이더)은 못 맞혔다 → 해결 (이상한 동작 Q9, 공격 판정 X4).
/// [근거] 명세 3.5, 결정 R16, 이상한 동작 Q9 / 공격 판정 결정 H10·H11
/// </summary>
public class AoESkillEffect : MonoBehaviour, ISkillDamageObject
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
    private readonly AttackHitResolver _hits = new AttackHitResolver();   // 한 번 터짐 = 공격 하나 (H10)

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

        _hits.BeginAttack();
        foreach (Collider enemyCollider in hitEnemies)
        {
            _hits.TryHit(enemyCollider, _damage);   // 부모 Actor 찾기·죽은 적 건너뜀·보스 한 번 (Q9 해결)
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
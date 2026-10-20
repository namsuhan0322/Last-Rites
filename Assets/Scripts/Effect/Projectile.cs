using UnityEngine;

/// <summary>
/// [역할] 창 R 버프의 추가 투사체(Slash wave 프리팹): 앞으로 날아가며 적 레이어에 닿을 때마다 데미지.
///        관통하고(맞아도 사라지지 않음) lifeTime 뒤 사라진다. 투사체 하나를 공격 하나로 보고 대상마다 한 번,
///        약점이 있는 보스에게도 한 번만 준다(AttackHitResolver).
/// [이유] 생성하는 쪽(PlayerAttack)이 ISkillDamageObject로 한 번에 찾는다.
///        예전에는 콜라이더 자신의 Actor만 찾아 늑대 보스는 몸통+약점으로 두 번 맞았다 (공격 판정 X4).
/// [근거] 명세 3.5, 결정 R16, R33 / 공격 판정 결정 H10·H11
/// </summary>
public class Projectile : MonoBehaviour, ISkillDamageObject
{
    [Header("투사체 설정")]
    public float speed = 20f;
    public float lifeTime = 3f;
    public GameObject hitEffect;

    private int _damage;
    private LayerMask _enemyLayer;
    private readonly AttackHitResolver _hits = new AttackHitResolver();   // 투사체 하나 = 공격 하나 (H10)

    public void Initialize(int damage, LayerMask targetLayer)
    {
        _damage = damage;
        _enemyLayer = targetLayer;
        _hits.BeginAttack();

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & _enemyLayer) != 0)
        {
            // 부모 Actor 찾기·같은 대상 한 번·보스 한 번. 맞힘 이펙트도 데미지를 준 때만 (H10)
            if (_hits.TryHit(other, _damage) != null && hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
        }
    }
}
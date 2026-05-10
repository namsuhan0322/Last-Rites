using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(Collider))]
public class VFXProjectile : MonoBehaviour
{
    [Header("VFX 설정")]
    public VisualEffect vfx;                    // 인스펙터에서 연결할 VFX 컴포넌트
    public string createEventName = "create";   // 생성 시 보낼 이벤트 이름
    public string hitEventName = "hit";         // 적중 시 보낼 이벤트 이름

    [Header("투사체 설정")]
    public float speed = 15f;                   // 날아가는 속도
    public float lifeTime = 5f;                 // 빗나갔을 때 자동 소멸 시간
    public float destroyDelayAfterHit = 2f;     // 이펙트가 재생될 시간을 벌어주는 딜레이

    private int _damage;
    private LayerMask _enemyLayer;
    private bool _isHit = false;

    public void Initialize(int damage, LayerMask enemyLayer)
    {
        _damage = damage;
        _enemyLayer = enemyLayer;
        _isHit = false;

        if (vfx != null)
        {
            vfx.SendEvent(createEventName);
        }

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (_isHit) return;
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isHit) return;

        if (((1 << other.gameObject.layer) & _enemyLayer) != 0)
        {
            Actor enemy = other.GetComponentInParent<Actor>();

            if (enemy != null && !enemy.IsDead)
            {
                enemy.TakeDamage(_damage);
                TriggerHitEffect();
            }
        }
    }

    private void TriggerHitEffect()
    {
        _isHit = true;

        if (vfx != null)
        {
            vfx.SendEvent(hitEventName);
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, destroyDelayAfterHit);
    }
}
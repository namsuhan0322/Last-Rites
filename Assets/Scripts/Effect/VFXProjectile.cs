using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(Collider))]
public class VFXProjectile : MonoBehaviour
{
    [Header("VFX 설정")]
    public VisualEffect vfx;
    public string createEventName = "create";
    public string hitEventName = "hit";

    [Header("투사체 설정")]
    public float speed = 15f;
    [Tooltip("투사체가 날아갈 수 있는 최대 사거리")]
    public float maxDistance = 10f;
    public float destroyDelayAfterHit = 2f;

    private int _damage;
    private LayerMask _enemyLayer;
    private bool _isHit = false;
    private Vector3 _startPosition;

    public void Initialize(int damage, LayerMask enemyLayer)
    {
        _damage = damage;
        _enemyLayer = enemyLayer;
        _isHit = false;

        _startPosition = transform.position;

        if (vfx != null)
        {
            vfx.SendEvent(createEventName);
        }

        Destroy(gameObject, 10f);
    }

    private void Update()
    {
        if (_isHit) return;

        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        if (Vector3.Distance(_startPosition, transform.position) >= maxDistance)
        {
            TriggerHitEffect();
        }
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawRay(transform.position, transform.forward * maxDistance);

        Vector3 endPosition = transform.position + transform.forward * maxDistance;
        Gizmos.DrawWireSphere(endPosition, 0.5f);
    }
}
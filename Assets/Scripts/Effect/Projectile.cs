using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("투사체 설정")]
    public float speed = 20f;
    public float lifeTime = 3f;
    public GameObject hitEffect;

    private int _damage;
    private LayerMask _enemyLayer;

    public void Initialize(int damage, LayerMask targetLayer)
    {
        _damage = damage;
        _enemyLayer = targetLayer;

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
            Actor enemy = other.GetComponent<Actor>();
            if (enemy != null)
            {
                enemy.TakeDamage(_damage);
            }

            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
        }
    }
}
using UnityEngine;

public class DarkProjectile : MonoBehaviour
{
    public int damage = 10;
    public LayerMask targetLayer;
    public GameObject hitVFXPrefab;

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & targetLayer) == 0)
            return;

        Actor actor = other.GetComponent<Actor>();
        if (actor != null)
        {
            SpawnHitVFX();

            actor.TakeDamage(damage, 1f);

        }

        Destroy(gameObject);

        if (other.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            SpawnHitVFX();
            Destroy(gameObject);
        }
    }

    void SpawnHitVFX()
    {
        if (hitVFXPrefab == null) return;

        GameObject vfx = Instantiate(
            hitVFXPrefab,
            transform.position,
            Quaternion.identity
        );

        Destroy(vfx, 1f);
    }
}

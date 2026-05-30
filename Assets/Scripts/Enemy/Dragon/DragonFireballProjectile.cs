using System.Collections.Generic;
using UnityEngine;

public class DragonFireballProjectile : MonoBehaviour
{
    [SerializeField] private int explosionDamage = 25;
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeTime = 5f;

    [Header("Ãæµ¹")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask targetLayer;

    [Header("Æø¹ß")]
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private float explosionRadius = 2.5f;

    private Vector3 direction;
    private Actor owner;
    private bool initialized = false;
    private bool exploded = false;

    public void Init(Vector3 dir, Actor owner)
    {
        direction = dir.normalized;
        this.owner = owner;
        initialized = true;

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (!initialized || exploded) return;

        float moveDistance = speed * Time.deltaTime;

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, moveDistance, groundLayer))
        {
            Explode(hit.point);
            return;
        }

        transform.position += direction * moveDistance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (exploded) return;

        Actor target = other.GetComponentInParent<Actor>();

        if (target == owner)
            return;

        if (target != null)
        {
            Vector3 hitPos = other.ClosestPoint(transform.position);
            Explode(hitPos);
            return;
        }

        if (((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            Vector3 hitPos = other.ClosestPoint(transform.position);
            Explode(hitPos);
        }
    }

    private void Explode(Vector3 position)
    {
        exploded = true;

        SoundManager.Instance.PlaySound("DragonFireballExplosion");


        if (explosionEffect != null)
        {
            GameObject fx = Instantiate(explosionEffect, position, Quaternion.identity);
            Destroy(fx, 3f);
        }

        HashSet<Actor> damagedTargets = new HashSet<Actor>();

        Collider[] hits = Physics.OverlapSphere(position, explosionRadius, targetLayer);

        foreach (Collider hit in hits)
        {
            Actor target = hit.GetComponentInParent<Actor>();

            if (target == null) continue;
            if (target == owner) continue;
            if (damagedTargets.Contains(target)) continue;

            damagedTargets.Add(target);
            target.TakeDamage(explosionDamage);
        }

        Destroy(gameObject);
    }
}
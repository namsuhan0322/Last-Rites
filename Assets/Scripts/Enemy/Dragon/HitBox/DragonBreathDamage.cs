using System.Collections.Generic;
using UnityEngine;

public class DragonBreathDamage : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private LayerMask targetLayer;

    private Actor owner;
    private BoxCollider boxCollider;

    // 한 번 스캔할 때 같은 Actor 중복 방지
    private HashSet<Actor> damagedThisFrame = new HashSet<Actor>();

    public void Init(Actor owner)
    {
        this.owner = owner;
        boxCollider = GetComponent<BoxCollider>();

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (boxCollider == null)
            return;

        damagedThisFrame.Clear();

        Vector3 center = boxCollider.transform.TransformPoint(boxCollider.center);
        Vector3 halfExtents = Vector3.Scale(boxCollider.size * 0.5f, boxCollider.transform.lossyScale);
        Quaternion rotation = boxCollider.transform.rotation;

        Collider[] hits = Physics.OverlapBox(
            center,
            halfExtents,
            rotation,
            targetLayer
        );

        foreach (Collider hit in hits)
        {
            Actor target = hit.GetComponentInParent<Actor>();

            if (target == null) continue;
            if (target == owner) continue;
            if (damagedThisFrame.Contains(target)) continue;

            damagedThisFrame.Add(target);
            target.TakeDamage(damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null) return;

        Gizmos.matrix = Matrix4x4.TRS(
            box.transform.TransformPoint(box.center),
            box.transform.rotation,
            Vector3.Scale(box.size, box.transform.lossyScale)
        );

        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}
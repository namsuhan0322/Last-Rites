using System.Collections.Generic;
using UnityEngine;

public class DragonAttackHitbox : MonoBehaviour
{
    [SerializeField] private int damage = 20;

    private bool isActive = false;
    private HashSet<Actor> hitTargets = new HashSet<Actor>();

    private Actor owner;

    private void Awake()
    {
        owner = GetComponentInParent<Actor>();
    }

    public void EnableHitbox()
    {
        isActive = true;
        hitTargets.Clear();
    }

    public void DisableHitbox()
    {
        isActive = false;
        hitTargets.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive)
            return;

        Actor target = other.GetComponentInParent<Actor>();

        if (target == null)
            return;

        if (target == owner)
            return;

        if (hitTargets.Contains(target))
            return;

        hitTargets.Add(target);
        target.TakeDamage(damage);

        Debug.Log($"Dragon Hit : {target.name}");
    }
}

using System.Collections.Generic;
using UnityEngine;

public class DragonWingHitbox : MonoBehaviour
{
    [SerializeField] private int damage = 25;

    private bool isActive = false;
    private HashSet<Actor> hitTargets = new HashSet<Actor>();

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
        if (!isActive) return;

        Actor target = other.GetComponentInParent<Actor>();
        if (target == null) return;

        if (target == GetComponentInParent<Actor>()) return;

        if (hitTargets.Contains(target)) return;

        hitTargets.Add(target);
        target.TakeDamage(damage);

        Debug.Log($"Wing Slam Hit : {target.name}");
    }
}

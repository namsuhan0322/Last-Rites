using System.Collections.Generic;
using UnityEngine;

public class DragonHeadHitbox : MonoBehaviour
{
    [SerializeField] private int damage = 20;

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
        if (!isActive)
            return;

        Actor target = other.GetComponent<Actor>();

        if (target == null)
            return;

        // 자기 자신 제외
        if (target.gameObject == transform.root.gameObject)
            return;

        if (hitTargets.Contains(target))
            return;

        hitTargets.Add(target);

        target.TakeDamage(damage);

        Debug.Log($"Dragon Bite Hit : {target.name}");
    }
}
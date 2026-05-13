using System.Collections.Generic;
using UnityEngine;

public class DragonBreathDamage : MonoBehaviour
{
    [SerializeField] private int damage = 5;
    [SerializeField] private float tickInterval = 0.3f;
    [SerializeField] private float lifeTime = 2f;

    private Actor owner;
    private Dictionary<Actor, float> nextDamageTime = new Dictionary<Actor, float>();

    public void Init(Actor owner)
    {
        this.owner = owner;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerStay(Collider other)
    {
        Actor target = other.GetComponentInParent<Actor>();

        if (target == null) return;
        if (target == owner) return;

        if (!nextDamageTime.ContainsKey(target))
            nextDamageTime[target] = 0f;

        if (Time.time < nextDamageTime[target])
            return;

        nextDamageTime[target] = Time.time + tickInterval;
        target.TakeDamage(damage);
    }
}
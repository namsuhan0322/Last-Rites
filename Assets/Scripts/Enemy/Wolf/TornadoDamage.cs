using System.Collections.Generic;
using UnityEngine;

public class TornadoDamage : MonoBehaviour
{
    public int damage = 10;
    public float tickInterval = 0.5f; 

    private Dictionary<Actor, float> hitTimers = new Dictionary<Actor, float>();

    private void OnTriggerStay(Collider other)
    {
        Actor actor = other.GetComponent<Actor>();
        if (actor == null) return;

        if (!hitTimers.ContainsKey(actor))
            hitTimers[actor] = 0f;

        hitTimers[actor] += Time.deltaTime;

        if (hitTimers[actor] >= tickInterval)
        {
            actor.TakeDamage(damage, 0f);
            hitTimers[actor] = 0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Actor actor = other.GetComponent<Actor>();
        if (actor == null || actor.IsDead) return;

        if (hitTimers.ContainsKey(actor))
            hitTimers.Remove(actor);
    }
}

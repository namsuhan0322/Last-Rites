using UnityEngine;

public class TornadoDamage : MonoBehaviour
{
    public int damage = 10;

    private void OnTriggerEnter(Collider other)
    {
        Actor actor = other.GetComponent<Actor>();
        if (actor == null) return;

        actor.TakeDamage(damage, 1f);
    }
}

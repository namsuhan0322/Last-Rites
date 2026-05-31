using System.Collections.Generic;
using UnityEngine;

public class DragonTornadoParticleDamage : MonoBehaviour
{
    [Header("데미지")]
    [SerializeField] private int damage = 30;
    [SerializeField] private float damageRadius = 3f;
    [SerializeField] private float damageCooldown = 0.5f;
    [SerializeField] private LayerMask targetLayer;

    [Header("디버그")]
    [SerializeField] private bool showDebugGizmos = true;

    private DragonBoss owner;
    private ParticleSystem ps;

    private readonly List<ParticleSystem.Particle> enterParticles =
        new List<ParticleSystem.Particle>();

    private readonly List<Vector3> debugPositions =
        new List<Vector3>();

    private readonly Dictionary<Actor, float> lastHitTime =
        new Dictionary<Actor, float>();

    public void Init(DragonBoss owner, int damage, LayerMask targetLayer)
    {
        this.owner = owner;
        this.damage = damage;
        this.targetLayer = targetLayer;

        ps = GetComponent<ParticleSystem>();
        RegisterPlayerCollider();
    }

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void RegisterPlayerCollider()
    {
        if (ps == null)
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player 태그 오브젝트를 못 찾음");
            return;
        }

        Collider col = player.GetComponentInChildren<Collider>();

        if (col == null)
        {
            CharacterController controller =
                player.GetComponentInChildren<CharacterController>();

            if (controller != null)
                col = controller;
        }

        if (col == null)
        {
            Debug.LogWarning("Player Collider 또는 CharacterController를 못 찾음");
            return;
        }

        ParticleSystem.TriggerModule trigger = ps.trigger;
        trigger.enabled = true;
        trigger.SetCollider(0, col);

        trigger.inside = ParticleSystemOverlapAction.Ignore;
        trigger.outside = ParticleSystemOverlapAction.Ignore;
        trigger.enter = ParticleSystemOverlapAction.Callback;
        trigger.exit = ParticleSystemOverlapAction.Ignore;

        Debug.Log("토네이도 Trigger Player 등록 완료: " + col.name);
    }

    private void OnParticleTrigger()
    {
        if (ps == null)
            return;

        int count = ps.GetTriggerParticles(
            ParticleSystemTriggerEventType.Enter,
            enterParticles
        );

        if (count <= 0)
            return;

        Debug.Log("토네이도 파티클 감지: " + count);

        debugPositions.Clear();

        for (int i = 0; i < count; i++)
        {
            Vector3 particlePos = enterParticles[i].position;
            debugPositions.Add(particlePos);

            Collider[] hits = Physics.OverlapSphere(
                particlePos,
                damageRadius,
                targetLayer
            );

            foreach (Collider hit in hits)
            {
                Actor actor = hit.GetComponentInParent<Actor>();

                if (actor == null || actor == owner)
                    continue;

                if (lastHitTime.TryGetValue(actor, out float lastTime))
                {
                    if (Time.time - lastTime < damageCooldown)
                        continue;
                }

                lastHitTime[actor] = Time.time;
                actor.TakeDamage(damage);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos)
            return;

        Gizmos.color = Color.red;

        foreach (Vector3 pos in debugPositions)
        {
            Gizmos.DrawWireSphere(pos, damageRadius);
        }
    }
}
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Patrol",
    story: "[Self] patrols",
    category: "Action/Dragon",
    id: "dragon_patrol_action")]
public partial class DragonPatrolAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private DragonBoss boss;
    private Vector3 targetPoint;
    private float timer;

    private const float maxPatrolTime = 6f;
    private const float arriveDistance = 1.2f;

    protected override Status OnStart()
    {
        boss = Self.Value.GetComponent<DragonBoss>();
        if (boss == null) return Status.Failure;

        timer = 0f;

        boss.agent.isStopped = false;
        boss.agent.ResetPath();
        boss.agent.speed = boss.PatrolSpeed;
        boss.agent.updateRotation = true;
        boss.SetMoveType(1);

        if (boss.GetRandomPatrolPoint(out targetPoint))
        {
            boss.agent.SetDestination(targetPoint);
            return Status.Running;
        }

        boss.Idle();
        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        if (boss == null || boss.agent == null)
            return Status.Failure;

        if (boss.HasPlayerInRange())
        {
            boss.Idle();
            return Status.Success;
        }

        if (!boss.agent.isOnNavMesh)
        {
            boss.Idle();
            return Status.Failure;
        }

        timer += Time.deltaTime;

        if (!boss.agent.pathPending &&
            boss.agent.pathStatus != UnityEngine.AI.NavMeshPathStatus.PathComplete)
        {
            boss.Idle();
            return Status.Success;
        }


        float directDistance = Vector3.Distance(
            boss.transform.position,
            targetPoint
        );

        if (directDistance <= arriveDistance)
        {
            boss.Idle();
            return Status.Success;
        }

        if (!boss.agent.pathPending &&
            boss.agent.hasPath &&
            boss.agent.remainingDistance <= boss.agent.stoppingDistance + 0.5f)
        {
            boss.Idle();
            return Status.Success;
        }

        if (timer >= maxPatrolTime)
        {
            boss.Idle();
            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (boss == null) return;

        if (boss.agent != null && boss.agent.isOnNavMesh)
        {
            boss.agent.isStopped = true;
            boss.agent.ResetPath();
            boss.agent.velocity = Vector3.zero;
            boss.agent.updateRotation = false;
        }

        boss.SetMoveType(0);
    }
}
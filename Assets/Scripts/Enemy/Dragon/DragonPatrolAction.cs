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
        boss.SetMoveType(1);

        if (boss.GetRandomPatrolPoint(out targetPoint))
        {
            boss.agent.SetDestination(targetPoint);
            return Status.Running;
        }

        boss.Idle();
        return Status.Failure;
    }

    protected override Status OnUpdate()
    {
        timer += Time.deltaTime;

        RotateToMoveDirection();

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

    private void RotateToMoveDirection()
    {
        Vector3 dir = boss.agent.desiredVelocity;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);

        boss.transform.rotation = Quaternion.Slerp(
            boss.transform.rotation,
            targetRot,
            Time.deltaTime * 3f
        );
    }

    protected override void OnEnd()
    {
        if (boss != null)
            boss.Idle();
    }
}
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Patrol",
    story: "[Self] changes to dragon patrol state",
    category: "Action/Dragon",
    id: "dragon_patrol_action")]
public partial class DragonPatrolAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private DragonBoss boss;

    protected override Status OnStart()
    {
        boss = Self.Value.GetComponent<DragonBoss>();
        if (boss == null) return Status.Failure;

        boss.agent.isStopped = false;
        boss.agent.speed = boss.PatrolSpeed;
        boss.SetMoveType(1);

        if (boss.GetRandomPatrolPoint(out Vector3 point))
        {
            boss.agent.SetDestination(point);
            return Status.Running;
        }

        boss.Idle();
        return Status.Failure;
    }

    protected override Status OnUpdate()
    {
        RotateToDestination();

        if (!boss.agent.pathPending &&
            boss.agent.remainingDistance <= boss.agent.stoppingDistance + 0.3f)
        {
            boss.Idle();
            return Status.Success;
        }

        return Status.Running;
    }

    private void RotateToDestination()
    {
        Vector3 dir = boss.agent.steeringTarget - boss.transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);

        boss.transform.rotation = Quaternion.Slerp(
            boss.transform.rotation,
            targetRot,
            Time.deltaTime * 3f
        );
    }
}

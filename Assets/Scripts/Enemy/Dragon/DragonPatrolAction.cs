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

        if (boss == null)
            return Status.Failure;

        boss.BT_Patrol();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (boss == null)
            return Status.Failure;

        // PatrolState가 끝나서 IdleState로 돌아가면 성공
        if (boss.StateMachine.CurrentState == boss.IdleState)
            return Status.Success;

        return Status.Running;
    }
}


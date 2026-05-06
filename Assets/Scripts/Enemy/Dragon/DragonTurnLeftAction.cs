using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Turn Left",
    story: "[Self] changes to dragon turn left state",
    category: "Action/Dragon",
    id: "dragon_turn_left_action")]
public partial class DragonTurnLeftAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private DragonBoss boss;

    protected override Status OnStart()
    {
        boss = Self.Value.GetComponent<DragonBoss>();

        if (boss == null)
            return Status.Failure;

        boss.BT_TurnLeft();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (boss == null)
            return Status.Failure;

        if (boss.StateMachine.CurrentState == boss.IdleState)
            return Status.Success;

        return Status.Running;
    }
}


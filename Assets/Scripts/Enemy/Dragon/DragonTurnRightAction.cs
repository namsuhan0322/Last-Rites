using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Turn Right",
    story: "[Self] changes to dragon turn right state",
    category: "Action/Dragon",
    id: "dragon_turn_right_action")]
public partial class DragonTurnRightAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private DragonBoss boss;

    protected override Status OnStart()
    {
        boss = Self.Value.GetComponent<DragonBoss>();

        if (boss == null)
            return Status.Failure;

        boss.BT_TurnRight();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (boss == null)
            return Status.Failure;

        // TurnRightState가 끝나서 IdleState로 돌아가면 성공
        if (boss.StateMachine.CurrentState == boss.IdleState)
            return Status.Success;

        return Status.Running;
    }
}
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Face Player",
    story: "[Self] faces player",
    category: "Action/Dragon",
    id: "dragon_face_player_action")]
public partial class DragonFacePlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private DragonBoss boss;

    protected override Status OnStart()
    {
        boss = Self.Value.GetComponent<DragonBoss>();

        if (boss == null)
            return Status.Failure;

        if (!boss.HasPlayerInRange())
            return Status.Failure;

        if (!boss.CanFacePlayer())
            return Status.Failure;

        boss.BT_FacePlayer();

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


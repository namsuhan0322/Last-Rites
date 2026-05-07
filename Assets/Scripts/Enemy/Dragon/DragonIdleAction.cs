using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Idle",
    story: "[Self] changes to dragon idle state",
    category: "Action/Dragon",
    id: "dragon_idle_action")]
public partial class DragonIdleAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    protected override Status OnStart()
    {
        DragonBoss boss = Self.Value.GetComponent<DragonBoss>();
        if (boss == null) return Status.Failure;

        boss.Idle();
        return Status.Success;
    }
}


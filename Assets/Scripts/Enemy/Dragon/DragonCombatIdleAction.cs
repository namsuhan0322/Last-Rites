using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Combat Idle",
    story: "[Self] waits during combat",
    category: "Action/Dragon",
    id: "dragon_combat_idle_action")]
public partial class DragonCombatIdleAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private DragonBoss boss;
    private float timer;

    protected override Status OnStart()
    {
        boss = Self.Value.GetComponent<DragonBoss>();

        if (boss == null)
            return Status.Failure;

        if (!boss.HasLockedTarget())
            return Status.Failure;

        timer = 0f;
        boss.Idle();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (boss == null)
            return Status.Failure;

        if (!boss.HasLockedTarget())
            return Status.Failure;

        boss.Idle();

        if (boss.IsInGlobalRecovery())
            return Status.Running;

        return Status.Success;
    }
}


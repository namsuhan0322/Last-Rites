using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Fire Breath",
    story: "[Self] uses fire breath",
    category: "Action/Dragon",
    id: "dragon_fire_breath_action")]
public partial class DragonFireBreathAction : Action
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

        if (!boss.CanUseAnyAttack())
            return Status.Failure;

        timer = 0f;

        boss.StopMove();
        boss.PlayFireBreath();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        timer += Time.deltaTime;

        if (timer >= boss.fireBreathDuration)
        {
            boss.StartGlobalAttackRecovery();
            boss.Idle();

            return Status.Success;
        }

        return Status.Running;
    }
}

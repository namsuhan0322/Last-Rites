using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Side Breath",
    story: "[Self] uses side breath",
    category: "Action/Dragon",
    id: "dragon_side_breath_action")]
public partial class DragonSideBreathAction : Action
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
        boss.PlaySideBreath();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        timer += Time.deltaTime;

        if (timer >= boss.sideBreathDuration)
        {
            boss.StopAttachedBreath();

            boss.StartGlobalAttackRecovery();
            boss.Idle();

            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (boss != null)
            boss.StopAttachedBreath();
    }
}
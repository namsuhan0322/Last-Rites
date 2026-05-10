using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Fireball",
    story: "[Self] uses fireball",
    category: "Action/Dragon",
    id: "dragon_fireball_action")]
public partial class DragonFireballAction : Action
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

        if (!boss.CanFireball())
            return Status.Failure;

        if (!boss.IsTargetInFireballRange())
            return Status.Failure;

        timer = 0f;

        boss.StopMove();
        boss.PlayFireball();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        timer += Time.deltaTime;

        if (timer >= boss.fireballDuration)
        {
            boss.StartFireballCooldown();
            boss.StartGlobalAttackRecovery();

            boss.Idle();

            return Status.Success;
        }

        return Status.Running;
    }
}

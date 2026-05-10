using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Wing Slam",
    story: "[Self] uses wing slam",
    category: "Action/Dragon",
    id: "dragon_wing_slam_action")]
public partial class DragonWingSlamAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private DragonBoss boss;
    private float timer;

    protected override Status OnStart()
    {
        boss = Self.Value.GetComponent<DragonBoss>();

        if (boss == null)
            return Status.Failure;

        if (!boss.CanUseAnyAttack())
            return Status.Failure;

        if (!boss.HasLockedTarget())
            return Status.Failure;

        if (!boss.CanWingSlam())
            return Status.Failure;

        int moveType = boss.GetWingSlamMoveType();

        if (moveType == -1)
            return Status.Failure;

        timer = 0f;

        boss.StopMove();
        boss.DisableAllWingHitboxes();
        boss.PlayWingSlam(moveType);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        timer += Time.deltaTime;

        if (timer >= boss.wingSlamDuration)
        {
            boss.DisableAllWingHitboxes();

            boss.StartWingSlamCooldown();
            boss.StartGlobalAttackRecovery();

            boss.Idle();

            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (boss != null)
            boss.DisableAllWingHitboxes();
    }
}


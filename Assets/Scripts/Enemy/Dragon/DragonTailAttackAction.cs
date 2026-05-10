using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Tail Attack",
    story: "[Self] uses tail attack",
    category: "Action/Dragon",
    id: "dragon_tail_attack_action")]
public partial class DragonTailAttackAction : Action
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

        if (!boss.CanTailAttack())
            return Status.Failure;

        int moveType = boss.GetTailAttackMoveType();

        if (moveType == -1)
            return Status.Failure;

        timer = 0f;

        boss.StopMove();
        boss.DisableAllTailHitboxes();
        boss.PlayTailAttack(moveType);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        timer += Time.deltaTime;

        if (timer >= boss.tailAttackDuration)
        {
            boss.DisableAllTailHitboxes();

            boss.StartTailAttackCooldown();
            boss.StartGlobalAttackRecovery();

            boss.Idle();

            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (boss != null)
            boss.DisableAllTailHitboxes();
    }
}

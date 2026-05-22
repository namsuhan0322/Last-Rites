using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Meteor Attack",
    story: "[Self] flies up and casts meteors",
    category: "Action/Dragon",
    id: "dragon_meteor_attack_action")]
public partial class DragonMeteorAttackAction : Action
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

        if (!boss.CanMeteor())
            return Status.Failure;

        timer = 0f;

        boss.SetBTActionPlaying(true);
        boss.StopMove();
        boss.SetManualMoveMode(true);
        boss.AnimatorApplyRootMotion(true);

        boss.PlayFlyUp();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (boss == null)
            return Status.Failure;

        timer += Time.deltaTime;

        if (timer >= boss.meteorDuration)
        {
            boss.StartMeteorCooldown();
            boss.StartGlobalAttackRecovery();

            boss.AnimatorApplyRootMotion(false);
            boss.SetManualMoveMode(false);
            boss.Idle();
            boss.SetBTActionPlaying(false);

            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (boss != null)
        {
            boss.AnimatorApplyRootMotion(false);
            boss.SetManualMoveMode(false);
            boss.Idle();
            boss.SetBTActionPlaying(false);
        }
    }
}

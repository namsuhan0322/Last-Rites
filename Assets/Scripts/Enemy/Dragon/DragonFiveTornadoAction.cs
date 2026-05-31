using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Five Tornado",
    story: "[Self] uses five tornado attack",
    category: "Action/Dragon",
    id: "dragon_five_tornado_action")]
public partial class DragonFiveTornadoAction : Action
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

        if (!boss.CanFiveTornado())
            return Status.Failure;

        timer = 0f;

        boss.SetManualMoveMode(true);
        boss.SetBTActionPlaying(true);
        boss.StopMove();
        boss.PlayFiveTornado();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        timer += Time.deltaTime;

        if (timer >= boss.fiveTornadoDuration)
        {
            boss.StartFiveTornadoCooldown();
            boss.StartGlobalAttackRecovery();

            boss.SyncAgentToTransform();
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
            boss.SyncAgentToTransform();
            boss.SetManualMoveMode(false);
            boss.SetBTActionPlaying(false);
        }
    }
}

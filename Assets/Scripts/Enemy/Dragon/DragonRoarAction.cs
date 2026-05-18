using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Roar",
    story: "[Self] roars",
    category: "Action/Dragon",
    id: "dragon_roar_action")]
public partial class DragonRoarAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private DragonBoss boss;
    private float timer;

    protected override Status OnStart()
    {
        boss = Self.Value.GetComponent<DragonBoss>();

        if (boss == null)
            return Status.Failure;

        if (!boss.ShouldFirstEncounterRoar())
            return Status.Failure;

        timer = 0f;

        boss.StopMove();
        boss.SetMoveType(4);
        boss.SetFirstEncounterRoared();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        timer += Time.deltaTime;

        if (timer >= boss.roarDuration)
        {
            boss.Idle();

            boss.StartGlobalAttackRecovery();
            boss.StartFaceCooldown();

            return Status.Success;
        }

        return Status.Running;
    }
}


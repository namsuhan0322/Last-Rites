using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Breath Charge",
    story: "[Self] casts breath then charges",
    category: "Action/Dragon",
    id: "dragon_breath_charge_action")]
public partial class DragonBreathChargeAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private DragonBoss boss;
    private float timer;
    private int phase;
    private bool spawnedCharge;

    protected override Status OnStart()
    {
        boss = Self.Value.GetComponent<DragonBoss>();

        if (boss == null)
            return Status.Failure;

        if (!boss.HasPlayerInRange())
            return Status.Failure;

        if (!boss.CanUseAnyAttack())
            return Status.Failure;

        if (!boss.ShouldUseBreathChargeEvent())
            return Status.Failure;

        boss.StopMove();
        boss.ResetBreathChargeCancel();
        boss.EnableWingWeakPoints();

        timer = 0f;
        phase = 0;
        spawnedCharge = false;

        boss.PlayBreathCast();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {

        if (boss.ShouldCancelBreathCharge())
        {
            boss.StopBreathCharge();
            boss.DisableWingWeakPoints();
            return Status.Failure;
        }

        timer += Time.deltaTime;

        if (phase == 0)
        {
            if (timer >= boss.breathCastTime)
            {
                phase = 1;
                timer = 0f;

                boss.PlayBreathChargeLoop();
                boss.SpawnBreathCharge();
                spawnedCharge = true;
            }

            return Status.Running;
        }

        if (phase == 1)
        {
            boss.KeepBreathChargeAlive();

            if (timer >= boss.breathChargeTime)
            {
                boss.DoBreathChargeExplosionDamage();

                boss.StopBreathCharge();
                boss.EndBreathChargeLoop();
                boss.StartGlobalAttackRecovery();
                boss.DisableWingWeakPoints();
                boss.ClearBreathChargeEvent();

                return Status.Success;
            }

            return Status.Running;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (boss != null)
        {
            boss.StopBreathCharge();
            boss.DisableWingWeakPoints();
        }
    }
}


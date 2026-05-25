using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Phase2 Crush Slam Combo",
    story: "[Self] uses right crush, left crush, right wing slam, left wing slam",
    category: "Action/Dragon",
    id: "dragon_phase2_crush_slam_combo_action")]
public partial class DragonPhase2CrushSlamComboAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private DragonBoss boss;
    private float timer;

    private bool playedFirst;
    private bool playedSecond;
    private bool playedThird;
    private bool playedFourth;

    protected override Status OnStart()
    {
        boss = Self.Value.GetComponent<DragonBoss>();

        if (boss == null)
            return Status.Failure;

        if (!boss.HasLockedTarget())
            return Status.Failure;

        if (!boss.CanUseAnyAttack())
            return Status.Failure;

        if (!boss.CanPhase2CrushSlamCombo())
            return Status.Failure;

        timer = 0f;
        playedFirst = false;
        playedSecond = false;
        playedThird = false;
        playedFourth = false;

        boss.SetBTActionPlaying(true);
        boss.StopMove();

        boss.DisableAllWingHitboxes();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (boss == null)
            return Status.Failure;

        timer += Time.deltaTime;

        if (!playedFirst)
        {
            playedFirst = true;
            boss.PlayRightWingCrush();
        }

        if (!playedSecond && timer >= boss.phase2CrushSlamComboSecondDelay)
        {
            playedSecond = true;
            boss.PlayLeftWingCrush();
        }

        if (!playedThird && timer >= boss.phase2CrushSlamComboThirdDelay)
        {
            playedThird = true;
            boss.PlayRightWingSlam();
        }

        if (!playedFourth && timer >= boss.phase2CrushSlamComboFourthDelay)
        {
            playedFourth = true;
            boss.PlayLeftWingSlam();
        }

        if (timer >= boss.phase2CrushSlamComboDuration)
        {
            boss.DisableAllWingHitboxes();

            boss.StartPhase2CrushSlamComboCooldown();
            boss.StartGlobalAttackRecovery();

            boss.Idle();
            boss.SetBTActionPlaying(false);

            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (boss == null)
            return;

        boss.DisableAllWingHitboxes();
        boss.SetBTActionPlaying(false);
    }
}


using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Phase2 Mixed Combo",
    story: "[Self] uses right crush, left wing slam, left crush combo",
    category: "Action/Dragon",
    id: "dragon_phase2_mixed_combo_action")]
public partial class DragonPhase2MixedComboAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private DragonBoss boss;
    private float timer;

    private bool playedFirst;
    private bool playedSecond;
    private bool playedThird;

    protected override Status OnStart()
    {
        boss = Self.Value.GetComponent<DragonBoss>();

        if (boss == null)
            return Status.Failure;

        if (!boss.HasLockedTarget())
            return Status.Failure;

        if (!boss.CanUseAnyAttack())
            return Status.Failure;

        if (!boss.CanPhase2MixedCombo())
            return Status.Failure;

        timer = 0f;
        playedFirst = false;
        playedSecond = false;
        playedThird = false;

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

        // 1타: 오른쪽 내려찍기
        if (!playedFirst)
        {
            playedFirst = true;
            boss.PlayRightWingCrush();
        }

        // 2타: 왼쪽 날개 공격
        if (!playedSecond && timer >= boss.phase2MixedComboSecondDelay)
        {
            playedSecond = true;
            boss.PlayLeftWingSlam();
        }

        // 3타: 왼쪽 내려찍기
        if (!playedThird && timer >= boss.phase2MixedComboThirdDelay)
        {
            playedThird = true;
            boss.PlayLeftWingCrush();
        }

        if (timer >= boss.phase2MixedComboDuration)
        {
            boss.DisableAllWingHitboxes();

            boss.StartPhase2MixedComboCooldown();
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


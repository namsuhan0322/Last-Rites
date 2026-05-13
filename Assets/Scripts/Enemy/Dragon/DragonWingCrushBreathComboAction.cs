using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Wing Crush Breath Combo",
    story: "[Self] faces target, uses random wing crush combo, then breath",
    category: "Action/Dragon",
    id: "dragon_wing_crush_breath_combo_action")]
public partial class DragonWingCrushBreathComboAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private DragonBoss boss;
    private Vector3 targetPosition;

    private float timer;
    private bool faced;
    private bool playedFirst;
    private bool playedSecond;
    private bool playedBreath;

    private bool firstLeft;

    protected override Status OnStart()
    {
        boss = Self.Value.GetComponent<DragonBoss>();

        if (boss == null)
            return Status.Failure;

        if (!boss.HasPlayerInRange())
            return Status.Failure;

        if (!boss.CanUseAnyAttack())
            return Status.Failure;

        if (!boss.CanWingCrushBreathCombo())
            return Status.Failure;

        Transform target = boss.GetLockedTarget();
        if (target == null)
            return Status.Failure;

        boss.StopMove();

        targetPosition = target.position;

        timer = 0f;
        faced = false;
        playedFirst = false;
        playedSecond = false;
        playedBreath = false;

        firstLeft = UnityEngine.Random.Range(0, 2) == 0;

        boss.DisableAllWingCrushHitboxes();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        // 처음에만 플레이어 위치 바라보기
        if (!faced)
        {
            Vector3 dir = targetPosition - boss.transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.01f)
            {
                faced = true;
                boss.Idle();
                timer = 0f;
                return Status.Running;
            }

            float signedAngle = Vector3.SignedAngle(
                boss.transform.forward,
                dir.normalized,
                Vector3.up
            );

            if (Mathf.Abs(signedAngle) <= boss.faceFinishAngle)
            {
                faced = true;
                boss.Idle();
                timer = 0f;
                return Status.Running;
            }

            if (signedAngle > 0f)
                boss.SetMoveType(3);
            else
                boss.SetMoveType(2);

            Quaternion targetRot = Quaternion.LookRotation(dir.normalized);

            boss.transform.rotation = Quaternion.RotateTowards(
                boss.transform.rotation,
                targetRot,
                boss.turnSpeed * Time.deltaTime
            );

            return Status.Running;
        }

        timer += Time.deltaTime;

        if (!playedFirst)
        {
            playedFirst = true;

            if (firstLeft)
                boss.PlayLeftWingCrush();
            else
                boss.PlayRightWingCrush();
        }

        if (!playedSecond && timer >= boss.secondCrushDelay)
        {
            playedSecond = true;

            if (firstLeft)
                boss.PlayRightWingCrush();
            else
                boss.PlayLeftWingCrush();
        }

        if (!playedBreath && timer >= boss.breathStartDelay)
        {
            playedBreath = true;
            boss.PlayFireBreath();
        }

        if (timer >= boss.wingCrushBreathDuration)
        {
            boss.DisableAllWingCrushHitboxes();

            boss.StartWingCrushBreathComboCooldown();
            boss.StartGlobalAttackRecovery();
            boss.Idle();

            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (boss == null) return;

        boss.DisableAllWingCrushHitboxes();
    }
}

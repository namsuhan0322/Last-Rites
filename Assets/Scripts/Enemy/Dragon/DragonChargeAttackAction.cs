using System;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Charge Attack",
    story: "[Self] uses charge attack",
    category: "Action/Dragon",
    id: "dragon_charge_attack_action")]
public partial class DragonChargeAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private DragonBoss boss;
    private float timer;
    private int phase;

    private GameObject indicator;
    private HashSet<Actor> hitActors = new HashSet<Actor>();

    protected override Status OnStart()
    {
        boss = Self.Value.GetComponent<DragonBoss>();

        if (boss == null)
            return Status.Failure;

        if (!boss.HasLockedTarget())
            return Status.Failure;

        if (!boss.CanUseAnyAttack())
            return Status.Failure;

        if (!boss.CanCharge())
            return Status.Failure;

        if (!boss.IsTargetInChargeRange())
            return Status.Failure;

        timer = 0f;
        phase = 0;
        hitActors.Clear();

        boss.SetBTActionPlaying(true);
        boss.StopMove();

        boss.ResetChargeTriggers();
        boss.PlayChargeReady();

        indicator = boss.CreateChargeIndicator();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (boss == null)
            return Status.Failure;

        timer += Time.deltaTime;

        // 0단계: 준비자세 + 플레이어 방향 추적 + 장판 추적
        if (phase == 0)
        {
            FollowTargetAndIndicator();

            if (timer >= boss.chargeReadyTime)
            {
                if (indicator != null)
                    UnityEngine.Object.Destroy(indicator);

                timer = 0f;
                phase = 1;

                boss.Idle();

                return Status.Running;
            }

            return Status.Running;
        }

        // 1단계: 장판 사라진 뒤 0.8초 대기
        if (phase == 1)
        {
            if (timer >= boss.chargeStartDelay)
            {
                timer = 0f;
                phase = 2;

                boss.ResetChargeTriggers();
                boss.SetManualMoveMode(true);
                boss.PlayCharge();

                return Status.Running;
            }

            return Status.Running;
        }

        // 2단계: 돌진 애니 진행 중
        if (phase == 2)
        {
            CheckChargeHit();

            if (timer >= boss.chargeDuration)
            {
                boss.ResetChargeTriggers();

                boss.SyncAgentToTransform();
                boss.SetManualMoveMode(false);
                boss.Idle();

                boss.StartChargeCooldown();
                boss.StartGlobalAttackRecovery();

                boss.SetBTActionPlaying(false);

                return Status.Success;
            }

            return Status.Running;
        }
        return Status.Running;
    }

    private void FollowTargetAndIndicator()
    {
        Transform target = boss.GetLockedTarget();

        if (target == null)
            return;

        Vector3 dir = target.position - boss.transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);

        boss.transform.rotation = Quaternion.RotateTowards(
            boss.transform.rotation,
            targetRot,
            boss.chargeTurnSpeed * Time.deltaTime
        );

        if (indicator != null)
        {
            Vector3 forwardOffset =
                boss.transform.forward *
                ((boss.chargeDistance * 0.5f) + boss.chargeIndicatorForwardOffset);

            indicator.transform.position =
                boss.transform.position + forwardOffset;

            indicator.transform.rotation =
                Quaternion.LookRotation(boss.transform.forward);

            float scaleZ =
                boss.chargeDistance / boss.chargeIndicatorBaseLength;

            indicator.transform.localScale =
                new Vector3(7f, 1f, scaleZ);
        }
    }

    private void CheckChargeHit()
    {
        Collider[] hits = Physics.OverlapSphere(
            boss.transform.position,
            boss.chargeHitRadius,
            boss.targetLayer
        );

        foreach (Collider hit in hits)
        {
            Actor target = hit.GetComponentInParent<Actor>();

            if (target == null)
                continue;

            if (target == boss)
                continue;

            if (hitActors.Contains(target))
                continue;

            hitActors.Add(target);
            target.TakeDamage(boss.chargeDamage);
        }
    }

    protected override void OnEnd()
    {
        if (indicator != null)
            UnityEngine.Object.Destroy(indicator);

        if (boss != null)
        {
            boss.ResetChargeTriggers();
            boss.SyncAgentToTransform();
            boss.SetManualMoveMode(false);
            boss.Idle();
            boss.SetBTActionPlaying(false);
        }
    }
}


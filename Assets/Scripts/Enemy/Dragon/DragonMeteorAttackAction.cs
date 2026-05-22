using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Meteor Attack",
    story: "[Self] flies up, casts meteors, then lands with jump attack",
    category: "Action/Dragon",
    id: "dragon_meteor_attack_action")]
public partial class DragonMeteorAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private DragonBoss boss;

    private float timer;
    private int phase;

    private Vector3 fallStartPos;
    private Vector3 landPos;

    private GameObject warningObj;
    private bool damageDone;
    private bool fallAnimPlayed;
    private float groundY;

    protected override Status OnStart()
    {
        boss = Self.Value.GetComponent<DragonBoss>();

        if (boss == null)
            return Status.Failure;

        if (!boss.CanUseAnyAttack())
            return Status.Failure;

        if (!boss.CanMeteor())
            return Status.Failure;

        if (!boss.HasLockedTarget())
            return Status.Failure;

        timer = 0f;
        phase = 0;
        damageDone = false;
        fallAnimPlayed = false;
        groundY = boss.transform.position.y;

        boss.SetBTActionPlaying(true);
        boss.StopMove();
        boss.SetManualMoveMode(true);

        boss.PlayFlyUp();
        boss.PlayJumpStartDustEffect();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (boss == null)
            return Status.Failure;

        timer += Time.deltaTime;

        if (phase == 0)
        {
            if (timer >= boss.meteorDuration)
            {
                phase = 1;
                timer = 0f;
                boss.PlaySkyLoop();
            }

            return Status.Running;
        }

        if (phase == 1)
        {
            if (timer >= boss.skyLoopTime)
            {
                phase = 2;
                timer = 0f;

                Transform target = boss.GetLockedTarget();
                if (target == null)
                    return Status.Failure;

                landPos = target.position;
                landPos.y = groundY;

                warningObj = boss.CreateJumpWarning(landPos);
            }

            return Status.Running;
        }

        if (phase == 2)
        {
            if (timer >= boss.warningShowTime)
            {
                if (warningObj != null)
                    GameObject.Destroy(warningObj);

                phase = 3;
                timer = 0f;
            }

            return Status.Running;
        }

        if (phase == 3)
        {
            if (timer >= boss.afterWarningDelay)
            {
                phase = 4;
                timer = 0f;

                fallAnimPlayed = false;
                damageDone = false;

                fallStartPos = boss.transform.position;
            }

            return Status.Running;
        }

        if (phase == 4)
        {
            if (!fallAnimPlayed && timer >= boss.fallAnimDelay)
            {
                fallAnimPlayed = true;
                boss.PlayJumpFall();
            }

            float t = timer / boss.fallTime;
            t = Mathf.Clamp01(t);

            boss.transform.position = Vector3.Lerp(fallStartPos, landPos, t);

            if (!damageDone && t >= 0.85f)
            {
                damageDone = true;
                boss.StartJumpImpactWave(landPos);
            }

            if (t >= 1f)
            {
                boss.transform.position = landPos;

                boss.SetManualMoveMode(false);

                boss.StartMeteorCooldown();
                boss.StartMeteorRecovery();

                boss.Idle();
                boss.SetBTActionPlaying(false);

                return Status.Success;
            }

            return Status.Running;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (warningObj != null)
            GameObject.Destroy(warningObj);

        if (boss != null)
        {
            boss.SetManualMoveMode(false);
            boss.Idle();
            boss.SetBTActionPlaying(false);
        }
    }
}
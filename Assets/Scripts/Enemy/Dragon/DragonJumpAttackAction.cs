using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Jump Attack",
    story: "[Self] uses jump attack",
    category: "Action/Dragon",
    id: "dragon_jump_attack_action")]
public partial class DragonJumpAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private DragonBoss boss;

    private float timer;
    private int phase;

    private Vector3 startPos;
    private Vector3 airPos;
    private Vector3 landPos;

    private GameObject warningObj;
    private bool damageDone;
    private bool fallAnimPlayed;

    protected override Status OnStart()
    {
        boss = Self.Value.GetComponent<DragonBoss>();

        if (boss == null)
            return Status.Failure;

        if (!boss.CanUseAnyAttack())
            return Status.Failure;

        if (!boss.HasLockedTarget())
            return Status.Failure;

        if (!boss.CanJumpAttack())
            return Status.Failure;

        Transform target = boss.GetLockedTarget();
        if (target == null)
            return Status.Failure;

        boss.StopMove();
        boss.SetManualMoveMode(true);

        startPos = boss.transform.position;
        landPos = target.position;
        landPos.y = startPos.y;

        airPos = startPos + Vector3.up * boss.jumpUpHeight;

        timer = 0f;
        phase = 0;
        damageDone = false;

        boss.PlayJumpStart();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        timer += Time.deltaTime;

        if (phase == 0)
        {
            float t = timer / boss.jumpUpTime;
            boss.transform.position = Vector3.Lerp(startPos, airPos, t);

            if (t >= 1f)
            {
                phase = 1;
                timer = 0f;
                boss.PlaySkyLoop();
            }

            return Status.Running;
        }

        if (phase == 1)
        {
            boss.transform.position = airPos;

            if (timer >= boss.skyLoopTime)
            {
                phase = 2;
                timer = 0f;

                warningObj = boss.CreateJumpWarning(landPos);
            }

            return Status.Running;
        }

        if (phase == 2)
        {
            boss.transform.position = airPos;

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
            boss.transform.position = airPos;

            if (timer >= boss.afterWarningDelay)
            {
                phase = 4;
                timer = 0f;
                fallAnimPlayed = false;
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
            boss.transform.position = Vector3.Lerp(airPos, landPos, t);

            if (!damageDone && t >= 0.85f)
            {
                damageDone = true;
                boss.DoJumpDamage(landPos);
            }

            if (t >= 1f)
            {
                boss.transform.position = landPos;

                boss.SetManualMoveMode(false);
                boss.StartJumpAttackCooldown();
                boss.StartGlobalAttackRecovery();
                boss.Idle();
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
            boss.SetManualMoveMode(false);
    }
}

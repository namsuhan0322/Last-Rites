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

        boss.SetBTActionPlaying(true);
        boss.StopMove();
        boss.SetManualMoveMode(true);
        boss.AnimatorApplyRootMotion(true);

        boss.PlayFlyUp();
        boss.PlayJumpStartDustEffect();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (boss == null)
            return Status.Failure;

        timer += Time.deltaTime;

        // 0단계: FlyUp + 메테오 진행
        if (phase == 0)
        {
            if (timer >= boss.meteorDuration)
            {
                phase = 1;
                timer = 0f;

                boss.AnimatorApplyRootMotion(false);
                boss.PlaySkyLoop();
            }

            return Status.Running;
        }

        // 1단계: SkyLoop 유지 후 착지 위치 정하기
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

                // 보스가 뒤로 밀려 보이면 착지 목표를 보스 앞쪽으로 보정
                landPos += boss.transform.forward * boss.meteorLandForwardOffset;

                // 바닥 높이 고정
                landPos.y = 0f;

                warningObj = boss.CreateJumpWarning(landPos);
            }

            return Status.Running;
        }

        // 2단계: 위험장판 표시
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

        // 3단계: 장판 사라진 후 짧은 딜레이
        if (phase == 3)
        {
            if (timer >= boss.afterWarningDelay)
            {
                phase = 4;
                timer = 0f;

                fallAnimPlayed = false;
                damageDone = false;

                // 내려찍기 시작 위치만 현재 공중 위치로 저장
                fallStartPos = boss.transform.position;
            }

            return Status.Running;
        }

        // 4단계: 내려찍을 때만 보스 몸 이동
        if (phase == 4)
        {
            if (!fallAnimPlayed && timer >= boss.fallAnimDelay)
            {
                fallAnimPlayed = true;

                boss.AnimatorApplyRootMotion(false); // 추가
                boss.PlayJumpFall();
            }

            float t = timer / boss.fallTime;
            t = Mathf.Clamp01(t);

            boss.transform.position = Vector3.Lerp(fallStartPos, landPos, t);

            if (!damageDone && t >= 0.85f)
            {
                damageDone = true;

                boss.DoJumpDamage(landPos);
                boss.PlayJumpLandImpactEffect(landPos);
            }

            if (t >= 1f)
            {
                boss.transform.position = landPos;

                boss.SetManualMoveMode(false);
                boss.AnimatorApplyRootMotion(false);

                boss.StartMeteorCooldown();
                boss.StartGlobalAttackRecovery();

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
            boss.AnimatorApplyRootMotion(false);
            boss.SetManualMoveMode(false);
            boss.Idle();
            boss.SetBTActionPlaying(false);
        }
    }
}
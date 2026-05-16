using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "DragonWingSlamBiteSideBreathComboAction", story: "uses wing slams, bite, then side breath", category: "Action", id: "81f41ee1b2a040cff70f35b82b457d28")]
public partial class DragonWingSlamBiteSideBreathComboAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private DragonBoss boss;
    private float timer;

    private bool playedFirstWing;
    private bool playedSecondWing;
    private bool startedFace;
    private bool faced;
    private bool playedBite;

    private bool firstLeft;
    private Vector3 biteTargetPosition;
    private bool playedSideBreath;

    protected override Status OnStart()
    {
        boss = Self.Value.GetComponent<DragonBoss>();

        if (boss == null)
            return Status.Failure;

        if (!boss.HasPlayerInRange())
            return Status.Failure;

        if (!boss.CanUseAnyAttack())
           return Status.Failure;

       if (!boss.CanWingSlamBiteSideBreathCombo())
          return Status.Failure;

        timer = 0f;
        playedFirstWing = false;
        playedSecondWing = false;
        startedFace = false;
        faced = false;
        playedBite = false;
        playedSideBreath = false;

        firstLeft = UnityEngine.Random.Range(0, 2) == 0;

        boss.StopMove();
        boss.DisableAllWingHitboxes();
        boss.DisableBiteHitbox();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        timer += Time.deltaTime;

        // 1. 첫 번째 WingSlam
        if (!playedFirstWing)
        {
            playedFirstWing = true;

            if (firstLeft)
                boss.PlayLeftWingSlam();
            else
                boss.PlayRightWingSlam();
        }

        // 2. 두 번째 WingSlam
        if (!playedSecondWing && timer >= boss.secondWingSlamDelay)
        {
            playedSecondWing = true;

            if (firstLeft)
                boss.PlayRightWingSlam();
            else
                boss.PlayLeftWingSlam();
        }

        // 3. Bite 준비 시작
        if (!startedFace && timer >= boss.biteAfterWingDelay)
        {
            Transform target = boss.GetLockedTarget();

            if (target == null)
                return Status.Failure;

            startedFace = true;
            biteTargetPosition = target.position;

            boss.StopMove();
        }

        // 4. Bite 직전에 플레이어 바라보기
        if (startedFace && !faced)
        {
            Vector3 dir = biteTargetPosition - boss.transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.01f)
            {
                faced = true;
                boss.Idle();
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

        // 5. Bite 실행
        if (faced && !playedBite)
        {
            playedBite = true;

            int moveType = boss.GetRandomBiteMoveType();

            boss.DisableBiteHitbox();
            boss.PlayBite(moveType);
        }

        if (!playedSideBreath && timer >= boss.sideBreathStartDelay)
        {
            playedSideBreath = true;
            boss.PlaySideBreath();
        }

        // 6. 전체 종료
        if (timer >= boss.wingSlamBiteSideBreathComboDuration)
        {
            boss.DisableAllWingHitboxes();
            boss.DisableBiteHitbox();
            boss.StopAttachedBreath();

            boss.StartWingSlamBiteSideBreathComboCooldown();
            boss.StartGlobalAttackRecovery();
            boss.Idle();

            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (boss == null) return;

        boss.DisableAllWingHitboxes();
        boss.DisableBiteHitbox();
        boss.StopAttachedBreath();
    }
}


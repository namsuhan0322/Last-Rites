using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Face Player For Attack",
    story: "[Self] faces locked attack position",
    category: "Action/Dragon",
    id: "dragon_face_player_for_attack_action")]
public partial class DragonFacePlayerForAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private DragonBoss boss;
    private Vector3 targetPosition;

    protected override Status OnStart()
    {
        boss = Self.Value.GetComponent<DragonBoss>();

        if (boss == null)
            return Status.Failure;

        if (!boss.HasLockedTarget())
            return Status.Failure;

        if (!boss.CanUseAnyAttack())
            return Status.Failure;

        boss.StopMove();

        boss.LockAttackPosition();
        targetPosition = boss.GetLockedAttackPosition();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Vector3 dir = targetPosition - boss.transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f)
        {
            boss.Idle();
            return Status.Success;
        }

        float signedAngle = Vector3.SignedAngle(
            boss.transform.forward,
            dir.normalized,
            Vector3.up
        );

        if (Mathf.Abs(signedAngle) <= boss.faceFinishAngle)
        {
            boss.Idle();
            return Status.Success;
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
}

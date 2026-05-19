using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Face Player Once",
    story: "[Self] faces player once",
    category: "Action/Dragon",
    id: "dragon_face_player_once_action")]
public partial class DragonFacePlayerOnceAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private DragonBoss boss;

    protected override Status OnStart()
    {
        boss = Self.Value.GetComponent<DragonBoss>();

        if (boss == null)
            return Status.Failure;

        if (!boss.ShouldFirstEncounterRoar())
            return Status.Failure;

        boss.StopMove();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Transform target = boss.GetLockedTarget();

        if (target == null)
            return Status.Failure;

        Vector3 dir = target.position - boss.transform.position;
        dir.y = 0f;

        float signedAngle = Vector3.SignedAngle(
            boss.transform.forward,
            dir.normalized,
            Vector3.up
        );

        if (Mathf.Abs(signedAngle) <= boss.faceFinishAngle)
        {
            boss.Idle();
            return Status.Success; // 포효로 넘어가기 위해 Success
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

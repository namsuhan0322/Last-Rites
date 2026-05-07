using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Bite",
    story: "[Self] uses bite attack",
    category: "Action/Dragon",
    id: "dragon_bite_action")]
public partial class DragonBiteAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private DragonBoss boss;
    private float timer;

    protected override Status OnStart()
    {
        boss = Self.Value.GetComponent<DragonBoss>();

        if (boss == null)
            return Status.Failure;

        if (!boss.HasLockedTarget())
            return Status.Failure;

        if (!boss.CanBite())
            return Status.Failure;

        if (!boss.IsLockedTargetInBiteRange())
            return Status.Failure;

        int moveType = boss.GetRandomBiteMoveType();

        if (moveType == -1)
            return Status.Failure;

        Debug.Log("진짜 BITE 실행");

        timer = 0f;

        boss.StopMove();
        boss.DisableBiteHitbox();
        boss.PlayBite(moveType);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        timer += Time.deltaTime;

        if (timer >= boss.biteDuration)
        {
            boss.DisableBiteHitbox();
            boss.StartBiteCooldownWithRecovery();
            boss.Idle();

            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (boss != null)
            boss.DisableBiteHitbox();
    }
}


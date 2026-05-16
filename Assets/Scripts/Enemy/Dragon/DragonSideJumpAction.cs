using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Side Jump",
    story: "[Self] jumps left or right",
    category: "Action/Dragon",
    id: "dragon_side_jump_action")]
public partial class DragonSideJumpAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private DragonBoss boss;
    private float timer;

    protected override Status OnStart()
    {
        boss = Self.Value.GetComponent<DragonBoss>();

        if (boss == null)
            return Status.Failure;

        if (!boss.HasPlayerInRange())
            return Status.Failure;

        if (!boss.CanUseAnyAttack())
            return Status.Failure;

        if (!boss.CanSideJump())
            return Status.Failure;

        timer = 0f;

        boss.StopMove();

        bool jumpLeft = UnityEngine.Random.Range(0, 2) == 0;

        if (jumpLeft)
            boss.PlayLeftSideJump();
        else
            boss.PlayRightSideJump();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        timer += Time.deltaTime;

        if (timer >= boss.sideJumpDuration)
        {
            boss.StartSideJumpCooldown();
            boss.Idle();

            return Status.Success;
        }

        return Status.Running;
    }
}
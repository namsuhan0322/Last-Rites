using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Turn Right",
    story: "[Self] changes to dragon turn right state",
    category: "Action/Dragon",
    id: "dragon_turn_right_action")]
public partial class DragonTurnRightAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private DragonBoss boss;
    private float timer;

    protected override Status OnStart()
    {
        boss = Self.Value.GetComponent<DragonBoss>();
        if (boss == null) return Status.Failure;

        if (boss.IsInGlobalRecovery())
        {
            boss.Idle();
            return Status.Failure;
        }

        timer = 0f;
        boss.StopMove();
        boss.SetMoveType(3);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        timer += Time.deltaTime;

        boss.transform.Rotate(Vector3.up, boss.turnSpeed * Time.deltaTime);

        if (timer >= boss.turnDuration)
        {
            boss.SetMoveType(0);
            return Status.Success;
        }

        return Status.Running;
    }
}
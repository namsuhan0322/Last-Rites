using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Phase2 Roar",
    story: "[Self] phase2 roars",
    category: "Action/Dragon",
    id: "dragon_phase2_roar_action")]
public partial class DragonPhase2RoarAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private DragonBoss boss;
    private float timer;

    protected override Status OnStart()
    {
        boss = Self.Value.GetComponent<DragonBoss>();

        if (boss == null)
            return Status.Failure;

        // 2페이즈 포효 예약이 없으면 실행 안 함
        if (!boss.ShouldPhase2Roar())
            return Status.Failure;

        timer = 0f;

        boss.StopMove();
        boss.SetMoveType(4); 
        boss.EnterPhase2();  

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        timer += Time.deltaTime;

        if (timer >= boss.roarDuration)
        {
            boss.Idle();
            boss.StartFaceCooldown();
            return Status.Success;
        }

        return Status.Running;
    }
}
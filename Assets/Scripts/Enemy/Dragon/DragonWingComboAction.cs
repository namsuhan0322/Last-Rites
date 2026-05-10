using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Dragon Wing Combo",
    story: "[Self] uses left and right wing combo",
    category: "Action/Dragon",
    id: "dragon_wing_combo_action")]
public partial class DragonWingComboAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private DragonBoss boss;
    private float timer;
    private bool playedLeft;
    private bool playedRight;

    protected override Status OnStart()
    {
        Debug.Log("[WingCombo] OnStart");

        boss = Self.Value.GetComponent<DragonBoss>();

        if (boss == null)
        {
            Debug.Log("[WingCombo] 실패: boss 없음");
            return Status.Failure;
        }

        if (!boss.HasPlayerInRange())
        {
            Debug.Log("[WingCombo] 실패: 플레이어 감지 안됨");
            return Status.Failure;
        }

        if (!boss.CanUseAnyAttack())
        {
            Debug.Log("[WingCombo] 실패: 공통 현타 중");
            return Status.Failure;
        }

        if (!boss.CanWingCombo())
        {
            Debug.Log("[WingCombo] 실패: WingCombo 쿨타임 중");
            return Status.Failure;
        }

        Debug.Log("[WingCombo] 실행 성공");

        timer = 0f;
        playedLeft = false;
        playedRight = false;

        boss.StopMove();
        boss.DisableAllWingHitboxes();

        return Status.Running;
    }
    protected override Status OnUpdate()
    {
        timer += Time.deltaTime;

        if (!playedLeft)
        {
            Debug.Log("[WingCombo] 왼쪽 날개 공격 실행");
            playedLeft = true;
            boss.PlayLeftWingSlam();
        }

        if (!playedRight && timer >= boss.rightWingStartDelay)
        {
            Debug.Log("[WingCombo] 오른쪽 날개 공격 실행");
            playedRight = true;
            boss.PlayRightWingSlam();
        }

        if (timer >= boss.wingComboDuration)
        {
            Debug.Log("[WingCombo] 종료");
            boss.DisableAllWingHitboxes();

            boss.StartWingComboCooldown();
            boss.StartGlobalAttackRecovery();
            boss.Idle();

            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (boss != null)
            boss.DisableAllWingHitboxes();
    }
}

using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// [역할] 화면 왼쪽 위에 플레이어 내부 상태(상태·전투·쿨타임·버프·보호막)와 최근 데미지 기록 8개를 보여 주고,
///        데미지 기록은 Console에도 `[DevTest][데미지]`로 남긴다.
/// [이유] 리팩토링 전후 비교를 숫자로 하기 위해 (작업 방식 5장: 눈에 보이는 피드백).
///        Console 기록은 그대로 복사해 결과를 전달하기 쉽게 하려고.
/// [근거] 명세 3.6-3, 결정 R26 (데미지 기록 화면+Console, 사용자 결정 C), 조작감 C10 (선입력·끊는 시점 표시), 공격 판정 H9 (판정 켜짐·맞힌 수)
/// </summary>
public class DevTestPlayerOverlay : MonoBehaviour
{
    public PlayerController player;

    private const int MaxRecentHits = 8;
    private readonly Queue<string> _recentHits = new Queue<string>();

    private void OnEnable() => DevTestDummyTarget.OnAnyDamaged += HandleDamaged;
    private void OnDisable() => DevTestDummyTarget.OnAnyDamaged -= HandleDamaged;

    // 맞은 순간의 무기·상태·스킬을 함께 적는다. 마법탄·블랙홀처럼 늦게 맞는 것은 '맞은 순간' 기준이라 상태가 이미 바뀌어 있을 수 있다.
    private void HandleDamaged(string target, int damage, string source)
    {
        string line = $"{Time.time:F2}초 {target} ← {damage} {source} ({DescribePlayer()})";
        Debug.Log($"[DevTest][데미지] {line}");

        _recentHits.Enqueue(line);
        while (_recentHits.Count > MaxRecentHits) _recentHits.Dequeue();
    }

    private string DescribePlayer()
    {
        if (player == null || player.StateMachine == null || player.StateMachine.CurrentState == null) return "-";
        string weapon = player.CurrentWeapon != null ? WeaponLabel(player.CurrentWeapon.weaponType) : "-";
        // PlayerSkillState → Skill 처럼 짧게
        string state = player.StateMachine.CurrentState.GetType().Name.Replace("Player", "").Replace("State", "");
        return state == "Skill" ? $"{weapon} · {state} {player.CurrentSkillAnim}" : $"{weapon} · {state}";
    }

    private static string WeaponLabel(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.GreatSword: return "대검";
            case WeaponType.Wizard: return "마법사";
            case WeaponType.DualBlade: return "쌍검";
            case WeaponType.Spear: return "창";
            default: return type.ToString();
        }
    }

    private static string BufferLabel(ActionInputBuffer buffer)
    {
        switch (buffer.Kind)
        {
            case BufferedActionKind.Attack: return "평타";
            case BufferedActionKind.Skill: return $"스킬 {buffer.Slot}";
            case BufferedActionKind.Dodge: return "회피";
            default: return "-";
        }
    }

    // 공격·스킬 애니일 때 타격 끝 시점과 지금 진행도 (조작감 C10)
    private string CancelLine()
    {
        AnimatorStateInfo s = player.Anim.GetCurrentAnimatorStateInfo(PlayerAnimParams.BaseLayer);
        if (!s.IsTag(PlayerAnimParams.TagAttack) && !s.IsTag(PlayerAnimParams.TagSkill)) return "끊는 시점 - (공격·스킬 애니 아님)";

        float hitEnd = player.CancelPoints.CurrentHitEnd(float.NaN);
        string end = float.IsNaN(hitEnd) ? "타격 이벤트 없음" : $"{hitEnd * 100f:F0}%";
        // 평타 애니면 슬라이더를 반영한 실제 다음 타 시점도 보여 준다 (C11)
        string next = s.IsTag(PlayerAnimParams.TagAttack) ? $"   다음 타 {player.AttackState.CurrentComboChainPoint() * 100f:F0}%" : "";
        return $"끊는 시점 타격 끝 {end}{next} / 지금 {s.normalizedTime * 100f:F0}%   (스킬 → 다른 스킬·회피 40%)";
    }

    private void OnGUI()
    {
        if (player == null || player.StateMachine == null || player.StateMachine.CurrentState == null) return;

        WeaponSO w = player.CurrentWeapon;
        var sb = new StringBuilder();
        sb.AppendLine($"상태 {player.StateMachine.CurrentState.GetType().Name}   전투 {(player.InCombat ? "ON" : "OFF")}");
        sb.AppendLine($"무기 {(w != null ? $"{w.name} ({w.weaponType})" : "-")}");
        sb.AppendLine($"HP {player.Stats.CurrentHP}/{player.Stats.MaxHP}   스태미나 {player.Stats.CurrentStamina:F1}/{player.Stats.MaxStamina}   포션 {player.currentPotionCount}");
        sb.AppendLine($"쿨 Q {player.Q_Timer:F2}  W {player.W_Timer:F2}  E {player.E_Timer:F2}  R {player.R_Timer:F2}  V {player.V_Timer:F2}");
        sb.AppendLine($"   회피 {player.DashTimer:F2}  포션 {player.Potion_Timer:F2}  공통 {player.globalSkillTimer:F2}  회피후공격 {player.postRollAttackTimer:F2}");
        sb.AppendLine($"스킬 애니 {player.CurrentSkillAnim}  데미지 {player.CurrentSkillDamage}  R값 {player.CurrentSkillVal}");
        sb.AppendLine($"선입력 {BufferLabel(player.InputBuffer)}   좌클릭 누름 {PlayerInputReader.AttackHeld}   콤보 연결 슬라이더 {player.comboChainDelay:F2}");
        sb.AppendLine(CancelLine());
        sb.AppendLine(player.Hitbox != null
            ? $"판정 {(player.Hitbox.IsActive ? "켜짐" : "꺼짐")} · 이번 휘두르기에서 맞힌 수 {player.Hitbox.HitCountThisSwing}"
            : "판정 - (WeaponHitbox 없음)");
        sb.AppendLine($"버프 대검 {player.HasGreatSwordBuff}  쌍검 {player.HasTwinBuff}  창 {player.HasSpearBuff}  검방 {player.HasShieldBuff}");
        sb.AppendLine($"속도 공속배율 {player.AtkSpeedModifier:F2}  애니 {player.Anim.speed:F2}  이동 {player.Agent.speed:F2}");
        sb.AppendLine($"보호막 {(player.isWizardShieldActive ? "ON" : "OFF")}  남은 {player.wizardShieldRemainingHits}");
        sb.AppendLine("F1 대검 F2 마법사 F3 쌍검 F4 창 | F5 HP1 F6 더미초기화 | F7 피격10 F8 피격40 F9 스턴 F10 즉사");
        sb.AppendLine("── 최근 데미지 (Console: [DevTest][데미지]) ──");
        if (_recentHits.Count == 0) sb.AppendLine("(아직 없음)");
        foreach (string hit in _recentHits) sb.AppendLine(hit);

        GUI.Box(new Rect(10f, 10f, 700f, 450f), GUIContent.none);
        GUI.Label(new Rect(18f, 14f, 690f, 445f), sb.ToString());
    }
}

using UnityEngine;

/// <summary>
/// [역할] 테스트 씬 단축키. F1~F4 무기 교체, F5 HP 1, F6 더미 초기화, F7/F8 피격, F9 스턴 공격, F10 즉사.
/// [이유] 무기별·상황별 동작을 실제 맵 진행 없이 바로 확인하기 위해.
/// [근거] 명세 3.6-3 (F1~F4, F5는 설계 섹션 5, F6~F9는 1단계 계획, F10은 1단계 실행 중 추가)
/// </summary>
public class DevTestPlayerHotkeys : MonoBehaviour
{
    public PlayerController player;

    // Assets/WeaponSOs 의 WeaponID: 10 대검(단두대), 11 마법사, 12 쌍검(가위), 13 창(묘비의 쐐기)
    private static readonly int[] WeaponIds = { 10, 11, 12, 13 };
    private static readonly KeyCode[] WeaponKeys = { KeyCode.F1, KeyCode.F2, KeyCode.F3, KeyCode.F4 };

    private void Update()
    {
        if (player == null || player.Stats == null) return;

        for (int i = 0; i < WeaponKeys.Length; i++)
        {
            if (Input.GetKeyDown(WeaponKeys[i])) ChangeWeapon(WeaponIds[i]);
        }

        if (Input.GetKeyDown(KeyCode.F5) && player.Stats.CurrentHP > 1)
            player.Stats.DrainHP(player.Stats.CurrentHP - 1);

        if (Input.GetKeyDown(KeyCode.F6))
        {
            foreach (var dummy in FindObjectsByType<DevTestDummyTarget>(FindObjectsSortMode.None))
                dummy.ResetDummy();
        }

        // 피격 세기는 Actor.CalculateHitSeverity 기준: 10 → 0.5(중간, 0.8초), 40 → 1.0(강함, 1.2초)
        if (Input.GetKeyDown(KeyCode.F7)) player.Stats.TakeDamage(10);
        if (Input.GetKeyDown(KeyCode.F8)) player.Stats.TakeDamage(40);
        // isHeavyAttack = true → 강인도와 상관없이 즉시 스턴(3초)
        if (Input.GetKeyDown(KeyCode.F9)) player.Stats.TakeDamage(5, -1f, true);
        // DrainHP는 무적·마법사 보호막 검사를 거치지 않으므로, 보호막을 켠 채 사망하는 경우(Q4)를 시험할 수 있다
        if (Input.GetKeyDown(KeyCode.F10) && !player.Stats.IsDead) player.Stats.DrainHP(player.Stats.CurrentHP);
    }

    private void ChangeWeapon(int weaponId)
    {
        WeaponSO weapon = player.weaponDatabase != null ? player.weaponDatabase.GetItemById(weaponId) : null;
        if (weapon == null)
        {
            Debug.LogWarning($"[DevTest] 무기 ID {weaponId}를 찾지 못함");
            return;
        }
        player.ChangeWeapon(weapon);
        Debug.Log($"[DevTest] 무기 교체 → {weapon.name} ({weapon.weaponType})");
    }
}

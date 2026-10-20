using UnityEngine;

/// <summary>
/// [역할] 무기 장착: 저장된 무기 불러오기, 무기 적용(히트박스·외형·애니메이터), 강화 반영(데코레이터), 장착 무기 저장.
/// [이유] 무기 교체·저장 흐름을 한곳에 모은다.
/// [근거] 명세 3.3 PlayerWeaponLoadout, 결정 R4, R27
/// </summary>
public class PlayerWeaponLoadout
{
    private const int AttackPerEnhanceLevel = 5;    // 강화 1단계당 무기 공격력 +5 (지금 코드의 level * 5)

    private readonly PlayerController _owner;

    public PlayerWeaponLoadout(PlayerController owner)
    {
        _owner = owner;
    }

    /// Start: 인벤토리에 저장된 무기를 장착. 인벤토리·데이터·DB가 없거나 저장된 무기를 못 찾으면 인스펙터의 CurrentWeapon
    public void LoadEquipped()
    {
        if (InventoryManager.Instance == null || InventoryManager.Instance.GetCurrentData() == null || _owner.weaponDatabase == null)
        {
            if (_owner.CurrentWeapon != null) Apply(_owner.CurrentWeapon);
            return;
        }

        int savedWeaponID = InventoryManager.Instance.GetCurrentData().equippedWeaponID;

        WeaponSO savedWeapon = _owner.weaponDatabase.GetItemById(savedWeaponID);

        if (savedWeapon != null)
        {
            Apply(savedWeapon);
        }
        else if (_owner.CurrentWeapon != null)
        {
            Apply(_owner.CurrentWeapon);
        }
    }

    public void Apply(WeaponSO weapon)
    {
        _owner.CurrentWeapon = weapon;

        if (_owner.Hitbox != null) _owner.Hitbox.SetupColliders(weapon.weaponType);
        if (_owner.VisualManager != null) _owner.VisualManager.SetupVisuals(weapon.weaponType);

        if (weapon.weaponAnimator != null)
        {
            _owner.Anim.runtimeAnimatorController = weapon.weaponAnimator;
            _owner.Anim.Rebind();
            _owner.Anim.Update(0f);
        }

        RefreshDecorated();
    }

    public void RefreshDecorated()
    {
        if (_owner.CurrentWeapon == null || InventoryManager.Instance == null) return;

        InventoryData invData = InventoryManager.Instance.GetCurrentData();

        ISword sword = new BaseWeapon(_owner.CurrentWeapon);

        if (invData.weaponEnhancementLevel > 0)
        {
            sword = new EnhancementDecorator(sword, invData.weaponEnhancementLevel, invData.weaponEnhancementLevel * AttackPerEnhanceLevel);
        }

        _owner.CurrentDecoratedWeapon = sword;
    }

    public void SaveEquipped(WeaponSO weapon)
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SaveEquippedWeapon(weapon.WeaponID);
        }
    }
}

using UnityEngine;

public class BaseWeapon : ISword
{
    private WeaponSO _weaponData;

    public BaseWeapon(WeaponSO data)
    {
        _weaponData = data;
    }

    public string GetName() => _weaponData.name;
    public int GetAttackPower() => _weaponData.Combo_1;
}
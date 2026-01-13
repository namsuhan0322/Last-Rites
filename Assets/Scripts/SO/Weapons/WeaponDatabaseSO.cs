using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDatabaseSO", menuName = "WeaponDatabase")]
public class WeaponDatabaseSO : ScriptableObject
{
    public List<WeaponSO> weapons = new List<WeaponSO>();

    private Dictionary<int, WeaponSO> weaponsById;
    private Dictionary<string, WeaponSO> weaponsByName;

    public void Initialize()
    {
        weaponsById = new Dictionary<int, WeaponSO>();
        weaponsByName = new Dictionary<string, WeaponSO>();

        foreach (var weapon in weapons)
        {
            weaponsById[weapon.WeaponID] = weapon;
            weaponsByName[weapon.name] = weapon;
        }
    }

    public WeaponSO GetItemById(int id)
    {
        if (weaponsById == null)
        {
            Initialize();
        }

        if (weaponsById.TryGetValue(id, out WeaponSO weapon))
            return weapon;

        return null;
    }

    public WeaponSO GetItemByName(string name)
    {
        if (weaponsByName == null)
        {
            Initialize();
        }

        if (weaponsByName.TryGetValue(name, out WeaponSO weapon))
            return weapon;

        return null;
    }

    public List<WeaponSO> GetItemByType(WeaponType type)
    {
        return weapons.FindAll(stage => stage.weaponType == type);
    }
}

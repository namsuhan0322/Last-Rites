using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlacksmithInfusionDatabase", menuName = "Database/BlacksmithInfusionDatabase")]
public class BlacksmithInfusionDatabaseSO : ScriptableObject
{
    public List<BlacksmithInfusionSO> infusionSOs = new List<BlacksmithInfusionSO>();

    private Dictionary<string, BlacksmithInfusionSO> infusionByName;
    private Dictionary<string, List<BlacksmithInfusionSO>> infusionByTargetWeapon;

    public void Initialize()
    {
        infusionByName = new Dictionary<string, BlacksmithInfusionSO>();
        infusionByTargetWeapon = new Dictionary<string, List<BlacksmithInfusionSO>>();

        foreach (var infusion in infusionSOs)
        {
            infusionByName[infusion.infusionName] = infusion;

            // 특정 무기에 바를 수 있는 인퓨전 리스트 캐싱
            if (!infusionByTargetWeapon.ContainsKey(infusion.Target_Weapon))
            {
                infusionByTargetWeapon[infusion.Target_Weapon] = new List<BlacksmithInfusionSO>();
            }
            infusionByTargetWeapon[infusion.Target_Weapon].Add(infusion);
        }
    }

    public BlacksmithInfusionSO GetInfusionByName(string name)
    {
        if (infusionByName == null) Initialize();
        if (infusionByName.TryGetValue(name, out BlacksmithInfusionSO infusion)) return infusion;
        return null;
    }

    public List<BlacksmithInfusionSO> GetInfusionsForWeapon(string weaponId)
    {
        if (infusionByTargetWeapon == null) Initialize();
        if (infusionByTargetWeapon.TryGetValue(weaponId, out List<BlacksmithInfusionSO> list)) return list;
        return new List<BlacksmithInfusionSO>();
    }
}
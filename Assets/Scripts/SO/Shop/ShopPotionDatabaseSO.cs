using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopPotionDatabase", menuName = "Database/ShopPotionDatabase")]
public class ShopPotionDatabaseSO : ScriptableObject
{
    public List<ShopPotionSO> potionSOs = new List<ShopPotionSO>();
    private Dictionary<int, ShopPotionSO> potionByLevel; // Key: Potion_Lv (int)

    public void Initialize()
    {
        potionByLevel = new Dictionary<int, ShopPotionSO>();
        foreach (var potion in potionSOs)
            potionByLevel[potion.Potion_Lv] = potion;
    }

    public ShopPotionSO GetPotionByLevel(int level)
    {
        if (potionByLevel == null) Initialize();
        if (potionByLevel.TryGetValue(level, out ShopPotionSO potion)) return potion;
        return null; // 해당 레벨의 포션이 없으면 null
    }
}
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopAIUnlockDatabase", menuName = "Database/ShopAIUnlockDatabase")]
public class ShopAIUnlockDatabaseSO : ScriptableObject
{
    public List<ShopAIUnlockSO> aiUnlockSOs = new List<ShopAIUnlockSO>();

    private Dictionary<string, ShopAIUnlockSO> unlockByName;
    private Dictionary<string, ShopAIUnlockSO> unlockByAIId;

    public void Initialize()
    {
        unlockByName = new Dictionary<string, ShopAIUnlockSO>();
        unlockByAIId = new Dictionary<string, ShopAIUnlockSO>();

        foreach (var unlock in aiUnlockSOs)
        {
            unlockByName[unlock.unlockName] = unlock;
            unlockByAIId[unlock.AI_Id] = unlock;
        }
    }

    public ShopAIUnlockSO GetUnlockByName(string name)
    {
        if (unlockByName == null) Initialize();
        if (unlockByName.TryGetValue(name, out ShopAIUnlockSO unlock)) return unlock;
        return null;
    }

    public ShopAIUnlockSO GetUnlockByAIId(string aiId)
    {
        if (unlockByAIId == null) Initialize();
        if (unlockByAIId.TryGetValue(aiId, out ShopAIUnlockSO unlock)) return unlock;
        return null;
    }
}
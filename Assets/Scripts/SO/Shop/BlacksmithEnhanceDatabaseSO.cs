using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlacksmithEnhanceDatabase", menuName = "Database/BlacksmithEnhanceDatabase")]
public class BlacksmithEnhanceDatabaseSO : ScriptableObject
{
    public List<BlacksmithEnhanceSO> enhanceSOs = new List<BlacksmithEnhanceSO>();

    private Dictionary<string, BlacksmithEnhanceSO> enhanceByName;

    public void Initialize()
    {
        enhanceByName = new Dictionary<string, BlacksmithEnhanceSO>();
        foreach (var enhance in enhanceSOs)
        {
            enhanceByName[enhance.Enhance_Lv] = enhance;
        }
    }

    public BlacksmithEnhanceSO GetEnhanceByName(string name)
    {
        if (enhanceByName == null) Initialize();
        if (enhanceByName.TryGetValue(name, out BlacksmithEnhanceSO enhance)) return enhance;
        return null;
    }
}
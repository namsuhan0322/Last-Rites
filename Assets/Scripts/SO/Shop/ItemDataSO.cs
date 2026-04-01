using UnityEngine;

// Item Data
public enum ItemType { None, Currency, Raw, Processed, BossSoul }

[CreateAssetMenu(fileName = "New Item", menuName = "GameData/Item Data")]
public class ItemDataSO : ScriptableObject
{
    public string itemName;
    public string ItemId;
    public ItemType itemType;
    public int Max_Stack;
    public string Description;
}

// Shop Recipe
[CreateAssetMenu(fileName = "New Recipe", menuName = "GameData/Shop Recipe")]
public class ShopRecipeSO : ScriptableObject
{
    [Header("결과물")]
    public ItemDataSO ResultItem;
    public int Result_Amt;

    [Header("필요 재료")]
    public ItemDataSO MatItem;
    public int Mat_Amt;

    [Header("소모 재화")]
    public ItemDataSO CostItem;
    public int Cost_Amt;
}

// Shop Potion
[CreateAssetMenu(fileName = "New Potion", menuName = "GameData/Shop Potion")]
public class ShopPotionSO : ScriptableObject
{
    public int Potion_Lv;
    public int Max_Count;
    public int Heal_Percent;

    [Header("필요 재료")]
    public ItemDataSO Req_Mat_1;
    public int Req_Mat_Amt_1;
    public ItemDataSO Req_Mat_2;
    public int Req_Mat_Amt_2;
}

// Shop AI Unlock
[CreateAssetMenu(fileName = "New AI Unlock", menuName = "GameData/Shop AI Unlock")]
public class ShopAIUnlockSO : ScriptableObject
{
    public string unlockName;
    public string AI_Id;

    [Header("해금 필요 재료")]
    public ItemDataSO Req_Mat_1;    // Req_Mat_Id_1 대체
    public int Req_Mat_Amt_1;
    public ItemDataSO Req_Mat_2;    // Req_Mat_Id_2 대체
    public int Req_Mat_Amt_2;

    public string Unlock_Conditon;
}

// Blacksmith Enhance
[CreateAssetMenu(fileName = "New Enhance Data", menuName = "GameData/Blacksmith Enhance")]
public class BlacksmithEnhanceSO : ScriptableObject
{
    public string Enhance_Lv;
    public int Success_Rate;
    public int Fall_Bonus;

    [Header("강화 필요 재료 & 재화")]
    public ItemDataSO Req_Mat;
    public int Req_Mat_Amt;
    public ItemDataSO Req_Cost;
    public int Req_Cost_Amt;
}

// Blacksmith Infusion
[CreateAssetMenu(fileName = "New Infusion Data", menuName = "GameData/Blacksmith Infusion")]
public class BlacksmithInfusionSO : ScriptableObject
{
    public string infusionName;
    public string Soul_Id;
    public string Target_Weapon;
    public string Effect_Value;
    public string Effect_Desc;
}
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using DB_;

public class BGShopDataConverter : EditorWindow
{
    private const string ITEM_PATH = "Assets/Resources/GameData/ItemData";

    [MenuItem("Tools/BGDatabase/상점 및 정비대 SO 변환하기")]
    public static void ConvertAllShopData()
    {
        ConvertItemData(); // 무조건 1순위 실행

        ConvertShopRecipe();
        ConvertShopPotion();
        ConvertShopAIUnlock();
        ConvertBlacksmithEnhance();
        ConvertBlacksmithInfusion();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("<color=cyan><b>[성공] 상점/정비대 SO 변환 및 맵핑이 완료되었습니다!</b></color>");
    }

    private static ItemDataSO GetItemSO(string itemId)
    {
        if (string.IsNullOrEmpty(itemId) || itemId == "None" || itemId == "0") return null;

        string assetPath = $"{ITEM_PATH}/{itemId}.asset";
        ItemDataSO foundSO = AssetDatabase.LoadAssetAtPath<ItemDataSO>(assetPath);

        if (foundSO == null)
            Debug.LogWarning($"[참조 경고] '{itemId}' 아이템 에셋을 찾을 수 없습니다.");

        return foundSO;
    }

    // 아이템 데이터
    private static void ConvertItemData()
    {
        if (!Directory.Exists(ITEM_PATH)) Directory.CreateDirectory(ITEM_PATH);

        _Item_Data.ForEachEntity(entity =>
        {
            if (string.IsNullOrEmpty(entity.ItemId)) return;

            string assetPath = $"{ITEM_PATH}/{entity.ItemId}.asset";
            ItemDataSO so = AssetDatabase.LoadAssetAtPath<ItemDataSO>(assetPath);
            if (so == null) { so = ScriptableObject.CreateInstance<ItemDataSO>(); AssetDatabase.CreateAsset(so, assetPath); }

            so.itemName = entity.name;
            so.ItemId = entity.ItemId;
            so.Max_Stack = entity.Max_Stack;
            so.Description = entity.Description;

            if (System.Enum.TryParse(entity.Type, true, out ItemType parsedType))
                so.itemType = parsedType;
            else
                so.itemType = ItemType.None;

            EditorUtility.SetDirty(so);
        });
    }

    // 상점 레시피
    private static void ConvertShopRecipe()
    {
        string path = "Assets/Resources/GameData/ShopRecipe";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        _Shop_Recipe.ForEachEntity(entity =>
        {
            if (string.IsNullOrEmpty(entity.Result_Id)) return; // 빈 줄 스킵

            string assetPath = $"{path}/Recipe_{entity.Result_Id}.asset";
            ShopRecipeSO so = AssetDatabase.LoadAssetAtPath<ShopRecipeSO>(assetPath);
            if (so == null) { so = ScriptableObject.CreateInstance<ShopRecipeSO>(); AssetDatabase.CreateAsset(so, assetPath); }

            so.Result_Amt = entity.Result_Amt;
            so.Mat_Amt = entity.Mat_Amt;
            so.Cost_Amt = entity.Cost_Amt;

            so.ResultItem = GetItemSO(entity.Result_Id);
            so.MatItem = GetItemSO(entity.Mat_Id);
            so.CostItem = GetItemSO(entity.Cost_Id);

            EditorUtility.SetDirty(so);
        });
    }

    // 상점 포션
    private static void ConvertShopPotion()
    {
        string path = "Assets/Resources/GameData/ShopPotion";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        _Shop_Potion.ForEachEntity(entity =>
        {
            if (entity.Potion_Lv <= 0) return; // 레벨이 0 이하면 스킵

            string assetPath = $"{path}/Potion_Lv_{entity.Potion_Lv}.asset";
            ShopPotionSO so = AssetDatabase.LoadAssetAtPath<ShopPotionSO>(assetPath);
            if (so == null) { so = ScriptableObject.CreateInstance<ShopPotionSO>(); AssetDatabase.CreateAsset(so, assetPath); }

            so.Potion_Lv = entity.Potion_Lv;
            so.Max_Count = entity.Max_Count;
            so.Heal_Percent = entity.Heal_Percent;
            so.Req_Mat_Amt_1 = entity.Req_Mat_Amt_1;
            so.Req_Mat_Amt_2 = entity.Req_Mat_Amt_2;

            so.Req_Mat_1 = GetItemSO(entity.Req_Mat_Id_1);
            so.Req_Mat_2 = GetItemSO(entity.Req_Mat_Id_2);

            EditorUtility.SetDirty(so);
        });
    }

    // AI 동료 해금
    private static void ConvertShopAIUnlock()
    {
        string path = "Assets/Resources/GameData/ShopAIUnlock";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        _Shop_AI_Unlock.ForEachEntity(entity =>
        {
            if (string.IsNullOrEmpty(entity.AI_Id)) return;

            string assetPath = $"{path}/Unlock_{entity.AI_Id}.asset";
            ShopAIUnlockSO so = AssetDatabase.LoadAssetAtPath<ShopAIUnlockSO>(assetPath);
            if (so == null) { so = ScriptableObject.CreateInstance<ShopAIUnlockSO>(); AssetDatabase.CreateAsset(so, assetPath); }

            so.AI_Id = entity.AI_Id;
            so.Req_Mat_Amt_1 = entity.Req_Mat_Amt_1;
            so.Req_Mat_Amt_2 = entity.Req_Mat_Amt_2;
            so.Unlock_Conditon = entity.Unlock_Condition;

            so.Req_Mat_1 = GetItemSO(entity.Req_Mat_Id_1);
            so.Req_Mat_2 = GetItemSO(entity.Req_Mat_Id_2);

            EditorUtility.SetDirty(so);
        });
    }

    // 무기 강화
    private static void ConvertBlacksmithEnhance()
    {
        string path = "Assets/Resources/GameData/BlacksmithEnhance";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        _Blacksmith_Enhance.ForEachEntity(entity =>
        {
            if (string.IsNullOrEmpty(entity.Enhance_Lv)) return;

            string assetPath = $"{path}/Enhance_{entity.Enhance_Lv}.asset";
            BlacksmithEnhanceSO so = AssetDatabase.LoadAssetAtPath<BlacksmithEnhanceSO>(assetPath);
            if (so == null) { so = ScriptableObject.CreateInstance<BlacksmithEnhanceSO>(); AssetDatabase.CreateAsset(so, assetPath); }

            so.Enhance_Lv = entity.Enhance_Lv;
            so.Success_Rate = entity.Success_Rate;
            so.Fall_Bonus = entity.Fall_Bonus;
            so.Req_Mat_Amt = entity.Req_Mat_Amt;
            so.Req_Cost_Amt = entity.Req_Cost_Amt;

            so.Req_Mat = GetItemSO(entity.Req_Mat_Id);
            so.Req_Cost = GetItemSO(entity.Req_Cost_Id);

            EditorUtility.SetDirty(so);
        });
    }

    // 속성 부여(인퓨전)
    private static void ConvertBlacksmithInfusion()
    {
        string path = "Assets/Resources/GameData/BlacksmithInfusion";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        _Blacksmith_Infusion.ForEachEntity(entity =>
        {
            if (string.IsNullOrEmpty(entity.Soul_Id)) return;

            string assetPath = $"{path}/Infusion_{entity.Soul_Id}.asset";
            BlacksmithInfusionSO so = AssetDatabase.LoadAssetAtPath<BlacksmithInfusionSO>(assetPath);
            if (so == null) { so = ScriptableObject.CreateInstance<BlacksmithInfusionSO>(); AssetDatabase.CreateAsset(so, assetPath); }

            so.infusionName = entity.name;
            so.Soul_Id = entity.Soul_Id;
            so.Target_Weapon = entity.Target_Weapon;
            so.Effect_Value = entity.Effect_Value;
            so.Effect_Desc = entity.Effect_Desc;

            EditorUtility.SetDirty(so);
        });
    }
}

#endif
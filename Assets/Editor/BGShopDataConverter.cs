#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using DB_;

public class BGShopDataConverter : EditorWindow
{
    private const string ITEM_PATH = "Assets/Resources/GameData/ItemData";
    private const string DB_PATH = "Assets/Resources/GameData/Databases"; // 데이터베이스 SO가 저장될 폴더

    [MenuItem("Tools/BGDatabase/상점 및 정비대 SO 전체 변환 (Database 자동등록)")]
    public static void ConvertAllShopData()
    {
        // 데이터베이스가 저장될 폴더가 없으면 생성
        if (!Directory.Exists(DB_PATH)) Directory.CreateDirectory(DB_PATH);

        ConvertItemData(); // 무조건 1순위 실행 (다른 SO들이 아이템을 참조해야 하므로)

        ConvertShopRecipe();
        ConvertShopPotion();
        ConvertShopAIUnlock();
        ConvertBlacksmithEnhance();
        ConvertBlacksmithInfusion();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("<color=lime><b>[대성공] 모든 SO 생성 및 Database 자동 등록이 완벽하게 끝났습니다!</b></color>");
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

    // ==========================================================
    // 1. 아이템 데이터 & Database 갱신
    // ==========================================================
    private static void ConvertItemData()
    {
        if (!Directory.Exists(ITEM_PATH)) Directory.CreateDirectory(ITEM_PATH);

        List<ItemDataSO> createdList = new List<ItemDataSO>();

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
            createdList.Add(so);
        });

        // [데이터베이스 연동]
        string dbFile = $"{DB_PATH}/ItemDatabase.asset";
        ItemDatabaseSO db = AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>(dbFile);
        if (db == null) { db = ScriptableObject.CreateInstance<ItemDatabaseSO>(); AssetDatabase.CreateAsset(db, dbFile); }
        db.itemSOs = createdList;
        EditorUtility.SetDirty(db);
    }

    // ==========================================================
    // 2. 상점 레시피 & Database 갱신
    // ==========================================================
    private static void ConvertShopRecipe()
    {
        string path = "Assets/Resources/GameData/ShopRecipe";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        List<ShopRecipeSO> createdList = new List<ShopRecipeSO>();

        _Shop_Recipe.ForEachEntity(entity =>
        {
            if (string.IsNullOrEmpty(entity.Result_Id)) return;

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
            createdList.Add(so);
        });

        string dbFile = $"{DB_PATH}/ShopRecipeDatabase.asset";
        ShopRecipeDatabaseSO db = AssetDatabase.LoadAssetAtPath<ShopRecipeDatabaseSO>(dbFile);
        if (db == null) { db = ScriptableObject.CreateInstance<ShopRecipeDatabaseSO>(); AssetDatabase.CreateAsset(db, dbFile); }
        db.recipeSOs = createdList;
        EditorUtility.SetDirty(db);
    }

    // ==========================================================
    // 3. 상점 포션 & Database 갱신
    // ==========================================================
    private static void ConvertShopPotion()
    {
        string path = "Assets/Resources/GameData/ShopPotion";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        List<ShopPotionSO> createdList = new List<ShopPotionSO>();

        _Shop_Potion.ForEachEntity(entity =>
        {
            if (entity.Potion_Lv <= 0) return;

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
            createdList.Add(so);
        });

        string dbFile = $"{DB_PATH}/ShopPotionDatabase.asset";
        ShopPotionDatabaseSO db = AssetDatabase.LoadAssetAtPath<ShopPotionDatabaseSO>(dbFile);
        if (db == null) { db = ScriptableObject.CreateInstance<ShopPotionDatabaseSO>(); AssetDatabase.CreateAsset(db, dbFile); }
        db.potionSOs = createdList;
        EditorUtility.SetDirty(db);
    }

    // ==========================================================
    // 4. AI 동료 해금 & Database 갱신
    // ==========================================================
    private static void ConvertShopAIUnlock()
    {
        string path = "Assets/Resources/GameData/ShopAIUnlock";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        List<ShopAIUnlockSO> createdList = new List<ShopAIUnlockSO>();

        _Shop_AI_Unlock.ForEachEntity(entity =>
        {
            if (string.IsNullOrEmpty(entity.AI_Id)) return;

            string assetPath = $"{path}/Unlock_{entity.AI_Id}.asset";
            ShopAIUnlockSO so = AssetDatabase.LoadAssetAtPath<ShopAIUnlockSO>(assetPath);
            if (so == null) { so = ScriptableObject.CreateInstance<ShopAIUnlockSO>(); AssetDatabase.CreateAsset(so, assetPath); }

            so.unlockName = entity.name;
            so.AI_Id = entity.AI_Id;
            so.Req_Mat_Amt_1 = entity.Req_Mat_Amt_1;
            so.Req_Mat_Amt_2 = entity.Req_Mat_Amt_2;
            so.Unlock_Conditon = entity.Unlock_Condition;

            so.Req_Mat_1 = GetItemSO(entity.Req_Mat_Id_1);
            so.Req_Mat_2 = GetItemSO(entity.Req_Mat_Id_2);

            EditorUtility.SetDirty(so);
            createdList.Add(so);
        });

        string dbFile = $"{DB_PATH}/ShopAIUnlockDatabase.asset";
        ShopAIUnlockDatabaseSO db = AssetDatabase.LoadAssetAtPath<ShopAIUnlockDatabaseSO>(dbFile);
        if (db == null) { db = ScriptableObject.CreateInstance<ShopAIUnlockDatabaseSO>(); AssetDatabase.CreateAsset(db, dbFile); }
        db.aiUnlockSOs = createdList;
        EditorUtility.SetDirty(db);
    }

    // ==========================================================
    // 5. 무기 강화 & Database 갱신
    // ==========================================================
    private static void ConvertBlacksmithEnhance()
    {
        string path = "Assets/Resources/GameData/BlacksmithEnhance";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        List<BlacksmithEnhanceSO> createdList = new List<BlacksmithEnhanceSO>();

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
            createdList.Add(so);
        });

        string dbFile = $"{DB_PATH}/BlacksmithEnhanceDatabase.asset";
        BlacksmithEnhanceDatabaseSO db = AssetDatabase.LoadAssetAtPath<BlacksmithEnhanceDatabaseSO>(dbFile);
        if (db == null) { db = ScriptableObject.CreateInstance<BlacksmithEnhanceDatabaseSO>(); AssetDatabase.CreateAsset(db, dbFile); }
        db.enhanceSOs = createdList;
        EditorUtility.SetDirty(db);
    }

    // ==========================================================
    // 6. 속성 부여(인퓨전) & Database 갱신
    // ==========================================================
    private static void ConvertBlacksmithInfusion()
    {
        string path = "Assets/Resources/GameData/BlacksmithInfusion";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        List<BlacksmithInfusionSO> createdList = new List<BlacksmithInfusionSO>();

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
            createdList.Add(so);
        });

        string dbFile = $"{DB_PATH}/BlacksmithInfusionDatabase.asset";
        BlacksmithInfusionDatabaseSO db = AssetDatabase.LoadAssetAtPath<BlacksmithInfusionDatabaseSO>(dbFile);
        if (db == null) { db = ScriptableObject.CreateInstance<BlacksmithInfusionDatabaseSO>(); AssetDatabase.CreateAsset(db, dbFile); }
        db.infusionSOs = createdList;
        EditorUtility.SetDirty(db);
    }
}
#endif
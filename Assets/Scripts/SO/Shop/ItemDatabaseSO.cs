using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Database/ItemDatabase")]
public class ItemDatabaseSO : ScriptableObject
{
    public List<ItemDataSO> itemSOs = new List<ItemDataSO>();

    private Dictionary<string, ItemDataSO> itemById;
    private Dictionary<string, ItemDataSO> itemByName;

    public void Initialize()
    {
        itemById = new Dictionary<string, ItemDataSO>();
        itemByName = new Dictionary<string, ItemDataSO>();

        foreach (var item in itemSOs)
        {
            itemById[item.ItemId] = item;
            itemByName[item.itemName] = item;
        }
    }

    public ItemDataSO GetItemById(string id)
    {
        if (itemById == null) Initialize();
        if (itemById.TryGetValue(id, out ItemDataSO item)) return item;
        return null;
    }

    public ItemDataSO GetItemByName(string name)
    {
        if (itemByName == null) Initialize();
        if (itemByName.TryGetValue(name, out ItemDataSO item)) return item;
        return null;
    }

    // Enum 타입으로 필터링해서 가져오는 함수
    public List<ItemDataSO> GetItemsByType(ItemType type)
    {
        return itemSOs.FindAll(item => item.itemType == type);
    }
}
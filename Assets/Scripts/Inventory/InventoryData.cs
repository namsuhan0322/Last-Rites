using System.Collections.Generic;

[System.Serializable]
public class InventoryData
{
    public int currencyAmount = 0; // 망자의 유물
    public int equippedWeaponID = 1;
    public string equippedAISoulID = "AI_001";
    public List<ItemSlot> items = new List<ItemSlot>();
}

[System.Serializable]
public class ItemSlot
{
    public string itemID;
    public int amount;

    public ItemSlot(string id, int amount)
    {
        this.itemID = id;
        this.amount = amount;
    }
}
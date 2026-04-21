using System.Collections.Generic;

[System.Serializable]
public class InventoryData
{
    public int currencyAmount = 0; // ∏¡¿⁄¿« ¿Øπ∞
    public int equippedWeaponID = 1;
    public string equippedAISoulID = "AI_001";
    public string equippedAttributeID = "";
    public int weaponEnhancementLevel = 0;
    public float soulPityGauge = 0f;

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
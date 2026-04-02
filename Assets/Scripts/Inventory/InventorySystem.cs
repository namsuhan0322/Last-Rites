using UnityEngine;

public static class InventorySystem
{
    public static void ProcessAddItem(InventoryData data, string id, int amount)
    {
        if (id == "C_001")
        {
            data.currencyAmount += amount;
            return;
        }

        ItemSlot existingItem = data.items.Find(x => x.itemID == id);
        if (existingItem != null)
        {
            existingItem.amount += amount;
        }
        else
        {
            data.items.Add(new ItemSlot(id, amount));
        }
    }
}
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

    public Sprite itemIcon;
}
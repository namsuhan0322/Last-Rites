using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopRecipeDatabase", menuName = "Database/ShopRecipeDatabase")]
public class ShopRecipeDatabaseSO : ScriptableObject
{
    public List<ShopRecipeSO> recipeSOs = new List<ShopRecipeSO>();

    public ShopRecipeSO GetRecipeByResultItem(ItemDataSO targetItem)
    {
        return recipeSOs.Find(r => r.ResultItem == targetItem);
    }
}
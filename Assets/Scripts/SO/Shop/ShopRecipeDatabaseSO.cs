using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopRecipeDatabase", menuName = "Database/ShopRecipeDatabase")]
public class ShopRecipeDatabaseSO : ScriptableObject
{
    public List<ShopRecipeSO> recipeSOs = new List<ShopRecipeSO>();

    private Dictionary<ItemDataSO, ShopRecipeSO> recipeByResult;

    public void Initialize()
    {
        recipeByResult = new Dictionary<ItemDataSO, ShopRecipeSO>();
        foreach (var recipe in recipeSOs)
        {
            if (recipe.ResultItem != null)
            {
                recipeByResult[recipe.ResultItem] = recipe;
            }
        }
    }

    public ShopRecipeSO GetRecipeByResultItem(ItemDataSO targetItem)
    {
        if (recipeByResult == null) Initialize();

        if (recipeByResult.TryGetValue(targetItem, out ShopRecipeSO recipe))
            return recipe;

        return null;
    }
}
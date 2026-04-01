using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopRecipeDatabase", menuName = "Database/ShopRecipeDatabase")]
public class ShopRecipeDatabaseSO : ScriptableObject
{
    public List<ShopRecipeSO> recipeSOs = new List<ShopRecipeSO>();

    // [추가] 내가 만들고 싶은 아이템(ItemDataSO)을 넣으면 해당 레시피를 찾아주는 함수!
    public ShopRecipeSO GetRecipeByResultItem(ItemDataSO targetItem)
    {
        return recipeSOs.Find(r => r.ResultItem == targetItem);
    }
}
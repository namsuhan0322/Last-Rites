// Shop Recipe
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "GameData/Shop Recipe")]
public class ShopRecipeSO : ScriptableObject
{
    [Header("결과물")]
    public ItemDataSO ResultItem;
    public int Result_Amt;

    [Header("필요 재료")]
    public ItemDataSO MatItem;
    public int Mat_Amt;

    [Header("소모 재화")]
    public ItemDataSO CostItem;
    public int Cost_Amt;
}
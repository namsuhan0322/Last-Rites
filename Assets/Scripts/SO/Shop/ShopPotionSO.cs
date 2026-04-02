// Shop Potion
using UnityEngine;

[CreateAssetMenu(fileName = "New Potion", menuName = "GameData/Shop Potion")]
public class ShopPotionSO : ScriptableObject
{
    public int Potion_Lv;
    public int Max_Count;
    public int Heal_Percent;

    [Header("필요 재료")]
    public ItemDataSO Req_Mat_1;
    public int Req_Mat_Amt_1;
    public ItemDataSO Req_Mat_2;
    public int Req_Mat_Amt_2;
}
// Blacksmith Enhance
using UnityEngine;

[CreateAssetMenu(fileName = "New Enhance Data", menuName = "GameData/Blacksmith Enhance")]
public class BlacksmithEnhanceSO : ScriptableObject
{
    public string Enhance_Lv;
    public int Success_Rate;
    public int Fall_Bonus;

    [Header("강화 필요 재료 & 재화")]
    public ItemDataSO Req_Mat;
    public int Req_Mat_Amt;
    public ItemDataSO Req_Cost;
    public int Req_Cost_Amt;
}
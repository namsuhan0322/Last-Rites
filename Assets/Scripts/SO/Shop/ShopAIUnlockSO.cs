// Shop AI Unlock
using UnityEngine;

[CreateAssetMenu(fileName = "New AI Unlock", menuName = "GameData/Shop AI Unlock")]
public class ShopAIUnlockSO : ScriptableObject
{
    public string unlockName;
    public string AI_Id;

    [Header("해금 필요 재료")]
    public ItemDataSO Req_Mat_1;    // Req_Mat_Id_1 대체
    public int Req_Mat_Amt_1;
    public ItemDataSO Req_Mat_2;    // Req_Mat_Id_2 대체
    public int Req_Mat_Amt_2;

    public string Unlock_Conditon;
}
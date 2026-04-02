// Blacksmith Infusion
using UnityEngine;

[CreateAssetMenu(fileName = "New Infusion Data", menuName = "GameData/Blacksmith Infusion")]
public class BlacksmithInfusionSO : ScriptableObject
{
    public string infusionName;
    public string Soul_Id;
    public string Target_Weapon;
    public string Effect_Value;
    public string Effect_Desc;
}
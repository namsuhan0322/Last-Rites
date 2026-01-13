using UnityEngine;

[CreateAssetMenu(menuName = "Player/PlayerSO")]
public class PlayerSO : ScriptableObject
{
    public int PlayerID;
    public string name;
    public int HP;
    public float Move_Spd;
    public float Dash_Spd;
    public float Dash_Time;
    public float Dash_Cool;
    public int Max_Stamina;
    public float Stamina_Regen;
    public int Dash_Cost;
}

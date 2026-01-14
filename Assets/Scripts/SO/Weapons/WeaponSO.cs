using UnityEngine;

[CreateAssetMenu(fileName = "WeaponSO", menuName = "WeaponSO")]
public class WeaponSO : ScriptableObject
{
    public int WeaponID;
    public string name;
    public WeaponType weaponType;
    public float Atk_Spd;
    public int Combo_1;
    public int Combo_2;
    public int Combo_3;
    public int Q_Dmg;
    public float Q_Cool;
    public int W_Dmg;
    public float W_Cool;
    public int E_Dmg;
    public float E_Cool;
    public float R_Val;
    public float R_Cool;
    public int V_Dmg;
    public float V_Cool;
}

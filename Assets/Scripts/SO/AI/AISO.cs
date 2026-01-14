using UnityEngine;

[CreateAssetMenu(fileName = "AISO", menuName = "AISO")]
public class AISO : ScriptableObject
{
    public int AiID;
    public string name;
    public RoleType roleType;
    public int Hp;
    public int Atk;
    public float Respawn;
    public string S1_Name;
    public float S1_Val;
    public float S1_Cool;
    public string S2_Name;
    public float S2_Val;
    public float S2_Cool;
}

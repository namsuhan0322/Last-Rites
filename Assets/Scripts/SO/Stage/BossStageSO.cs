using UnityEngine;

[CreateAssetMenu(fileName = "New Boss Stage", menuName = "GameData/Boss Stage")]
public class BossStageSO : ScriptableObject
{
    public string bossName;
    public string sceneName;
    [TextArea] public string description;
    public Sprite bossIcon;
    public Sprite bossLargeImage;
}
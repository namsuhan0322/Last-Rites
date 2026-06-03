using UnityEngine;

[CreateAssetMenu(fileName = "TowerFloorSO", menuName = "Tower/Tower Floor")]
public class TowerFloorSO : ScriptableObject
{
    public int floor;
    public string floorName;

    [TextArea]
    public string description;

    public Sprite floorLargeImage;

    [Header("보상 아이콘")]
    public Sprite clearIcon;
}

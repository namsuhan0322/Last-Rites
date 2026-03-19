using UnityEngine;

[System.Serializable]
public class GameProgressData
{
    [Header("진행 상황")]
    public bool isTutorialCleared;  // 튜토리얼 클리어 여부
    public int clearedThemeLevel;   // 클리어한 테마 진행도 (0: 없음, 1: 테마1 클리어, 2: 테마2 클리어...)

    public GameProgressData()
    {
        isTutorialCleared = false;
        clearedThemeLevel = 0;
    }
}
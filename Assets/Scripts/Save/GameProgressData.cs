using UnityEngine;

[System.Serializable]
public class GameProgressData
{
    [Header("진행 상황")]
    public bool isTutorialCleared;  // 튜토리얼 클리어 여부
    public int clearedThemeLevel;   // 클리어한 테마 진행도 (0: 없음, 1: 테마1 클리어, 2: 테마2 클리어...)

    [Header("부활 위치")]
    public bool hasSavedRespawn;    // 한 번이라도 세이브를 했는지 여부
    public float respawnPosX;
    public float respawnPosY;
    public float respawnPosZ;

    public GameProgressData()
    {
        isTutorialCleared = false;
        clearedThemeLevel = 0;

        hasSavedRespawn = false;
        respawnPosX = 0f;
        respawnPosY = 0f;
        respawnPosZ = 0f;
    }
}
using UnityEngine;

public class GameProgressManager : SingletonMono<GameProgressManager>
{
    protected override bool DontDestroy => true;

    [Header("Data")]
    public GameProgressData progressData { get; private set; }

    // DataManager가 게임 시작 시 이 함수를 호출해 데이터를 주입해 줍니다.
    public void InitializeData(GameProgressData data)
    {
        progressData = data ?? new GameProgressData();
        Debug.Log("[GameProgressManager] 진행도 데이터 초기화 완료");
    }


    public void CompleteTutorial()
    {
        progressData.isTutorialCleared = true;
        DataManager.Instance.SaveAllData();
    }

    public void CompleteTheme(int themeLevel)
    {
        progressData.clearedThemeLevel = themeLevel;
        DataManager.Instance.SaveAllData();
    }

    public void SaveRespawnPoint(Vector3 position)
    {
        progressData.hasSavedRespawn = true;
        progressData.respawnPosX = position.x;
        progressData.respawnPosY = position.y;
        progressData.respawnPosZ = position.z;

        DataManager.Instance.SaveAllData();
    }
}
using UnityEngine;

public class GameProgressManager : SingletonMono<GameProgressManager>
{
    protected override bool DontDestroy => true;

    [Header("Data")]
    public GameProgressData progressData { get; private set; }

    public void InitializeData(GameProgressData data)
    {
        progressData = data ?? new GameProgressData();
        Debug.Log("[GameProgressManager] 진행도 데이터 초기화 완료");
    }

    public void CompleteTutorial()
    {
        if (progressData == null)
        {
            progressData = new GameProgressData();
        }

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

    public void SaveEquippedWeapon(int weaponID)
    {
        progressData.equippedWeaponID = weaponID;
        DataManager.Instance.SaveAllData();
        Debug.Log($"[GameProgressManager] 무기 ID 저장 완료: {weaponID}");
    }
}
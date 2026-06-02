using UnityEngine;
using System.IO;

public class DataManager : SingletonMono<DataManager>
{
    protected override bool DontDestroy => true;

    private string saveFilePath;

    // 게임 전체 데이터
    public PlaythroughSaveData CurrentGameData { get; private set; }


    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    protected override void Awake()
    {
        base.Awake();
        saveFilePath = Path.Combine(Application.persistentDataPath, "LastRites_SaveData.json");

        LoadAllData();

        if (GameProgressManager.Instance != null)
            GameProgressManager.Instance.InitializeData(CurrentGameData.progressData);

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.InitializeData(CurrentGameData.inventoryData);
    }

    public void SaveAllData()
    {
        string json = JsonUtility.ToJson(CurrentGameData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"[DataManager] 게임 전체 데이터 저장 완료: {saveFilePath}");
    }

    public void LoadAllData()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                CurrentGameData = JsonUtility.FromJson<PlaythroughSaveData>(json);
                Debug.Log("[DataManager] 게임 데이터 불러오기 성공");
            }
            catch
            {
                Debug.LogError("[DataManager] 데이터 손상됨, 뉴 게임 시작");
                CurrentGameData = new PlaythroughSaveData();
            }
        }
        else
        {
            Debug.Log("[DataManager] 저장된 파일 없음, 뉴 게임 시작");
            CurrentGameData = new PlaythroughSaveData();
        }
    }

    // 게임 데이터 완전 초기화
    public void ResetAllData()
    {
        CurrentGameData = new PlaythroughSaveData();
        SaveAllData();

        // 초기화된 데이터를 다시 매니저들에게 뿌림
        GameProgressManager.Instance.InitializeData(CurrentGameData.progressData);
        InventoryManager.Instance.InitializeData(CurrentGameData.inventoryData);
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (CurrentGameData != null)
        {
            if (GameProgressManager.Instance != null)
                GameProgressManager.Instance.InitializeData(CurrentGameData.progressData);

            if (InventoryManager.Instance != null)
                InventoryManager.Instance.InitializeData(CurrentGameData.inventoryData);

            Debug.Log($"[DataManager] {scene.name} 씬 로드 완료. 데이터를 매니저들에게 다시 동기화했습니다.");
        }
    }
}
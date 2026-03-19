using UnityEngine;
using System.IO;

public class GameProgressManager : SingletonMono<GameProgressManager>
{
    protected override bool DontDestroy => true;

    [Header("Data")]
    public GameProgressData progressData;

    private string saveFilePath;

    protected override void Awake()
    {
        base.Awake();
        // 설정 데이터와 파일 이름을 다르게 지정합니다!
        saveFilePath = Path.Combine(Application.persistentDataPath, "progress_data.json");
        LoadProgress();
    }

    public void SaveProgress()
    {
        string json = JsonUtility.ToJson(progressData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"[GameProgressManager] 진행도 저장 완료: {saveFilePath}");
    }

    public void LoadProgress()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                progressData = JsonUtility.FromJson<GameProgressData>(json);
                Debug.Log("[GameProgressManager] 진행도 불러오기 성공");
            }
            catch
            {
                Debug.LogError("[GameProgressManager] 데이터 손상됨, 기본값(처음부터) 시작");
                progressData = new GameProgressData();
            }
        }
        else
        {
            Debug.Log("[GameProgressManager] 진행도 파일 없음, 뉴 게임 시작");
            progressData = new GameProgressData();
        }
    }

    // 진행도를 아예 초기화하는 기능 (새 게임 시작 시 호출)
    public void ResetProgress()
    {
        progressData = new GameProgressData();
        SaveProgress();
    }
}
using UnityEngine;
using System.IO;

public class SaveDataHolder : SingletonMono<SaveDataHolder>
{
    protected override bool DontDestroy => true;

    [Header("Data")]
    public SaveData currentData; // 현재 메모리에 올라와 있는 데이터

    public bool HasChanges = false;

    private string saveFilePath;

    protected override void Awake()
    {
        base.Awake();
        saveFilePath = Path.Combine(Application.persistentDataPath, "settings_data.json");
        Load();
    }

    // [Y] 키로 호출될 저장 함수
    public void Save()
    {
        // currentData를 JSON 문자열로 변환
        string json = JsonUtility.ToJson(currentData, true);

        // 파일 쓰기
        File.WriteAllText(saveFilePath, json);

        HasChanges = false;

        Debug.Log($"[SaveDataHolder] 저장 완료: {saveFilePath}");
    }

    // 게임 시작 시 호출될 불러오기 함수
    public void Load()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                currentData = JsonUtility.FromJson<SaveData>(json);
                Debug.Log("[SaveDataHolder] 불러오기 성공");
            }
            catch
            {
                Debug.LogError("[SaveDataHolder] 데이터 손상됨, 기본값 사용");
                currentData = new SaveData();
            }
        }
        else
        {
            Debug.Log("[SaveDataHolder] 저장된 파일 없음, 기본값 생성");
            currentData = new SaveData();
        }

        HasChanges = false;
    }

    // [R] 키로 호출될 초기화 함수
    public void ResetData()
    {
        currentData = new SaveData();
        Save(); // 파일도 덮어쓰기
        Debug.Log("[SaveDataHolder] 데이터 초기화 완료");
    }
}
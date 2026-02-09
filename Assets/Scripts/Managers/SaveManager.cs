using UnityEngine;

public class SaveManager : SingletonMono<SaveManager>
{
    protected override bool DontDestroy => true;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            TrySave();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            TryReset();
        }
    }

    public void TrySave()
    {
        UISystemPopup.Instance.ShowPopup(
            "설정 저장",
            "현재 설정을 저장하시겠습니까?",
            () => { // [예] 눌렀을 때
                SaveDataHolder.Instance.Save();
                Debug.Log("저장 완료");
            },
            null // [아니요]는 그냥 닫기
        );
    }

    public void TryReset()
    {
        UISystemPopup.Instance.ShowPopup(
            "설정 초기화",
            "모든 설정을 기본값으로 되돌리시겠습니까?\n이 작업은 되돌릴 수 없습니다.",
            () => { // [예] 눌렀을 때
                ResetSettings();
            },
            null // [아니요]는 그냥 닫기
        );
    }

    public void ResetSettings()
    {
        // 1. 데이터 초기화
        SaveDataHolder.Instance.ResetData();

        // 2. 그래픽 설정 UI 및 적용값 갱신
        var graphics = FindObjectOfType<GraphicsSettingsManager>();
        if (graphics != null) graphics.InitUI();

        // 3. 사운드 설정 UI 및 적용값 갱신
        var sound = FindObjectOfType<MixerController>();
        if (sound != null) sound.InitUI();

        Debug.Log("모든 설정이 초기화되었습니다.");
    }
}
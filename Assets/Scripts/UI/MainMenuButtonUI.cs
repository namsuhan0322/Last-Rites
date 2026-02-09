using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuButtonUI : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject settingMenu;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingMenu != null && settingMenu.activeSelf)
            {
                if (UISystemPopup.Instance.popupPanel.activeSelf)
                {
                    UISystemPopup.Instance.ClosePopup();
                }
                else
                {
                    // 설정창 닫기 시도 (팝업 띄우기)
                    TryCloseSettings();
                }
            }
        }
    }

    public void OpenSetting()
    {
        settingMenu.SetActive(true);
        mainMenu.SetActive(false);
    }

    public void TryCloseSettings()
    {
        if (SaveDataHolder.Instance.HasChanges)
        {
            UISystemPopup.Instance.ShowPopup(
                "변경사항 저장",
                "설정을 저장하고 나가시겠습니까?\n'아니요'를 선택하면 저장되지 않습니다.",
                () => { // [예] 저장 후 닫기
                    SaveDataHolder.Instance.Save();
                    OnCloseSettings();
                },
                () => { // [아니요] 로드(원상복구) 후 닫기
                    SaveDataHolder.Instance.Load();
                    FindObjectOfType<GraphicsSettingsManager>()?.InitUI();
                    FindObjectOfType<MixerController>()?.InitUI();
                    OnCloseSettings();
                }
            );
        }
        else
        {
            OnCloseSettings();
        }
            
    }

    public void OnCloseSettings()
    {
        if (settingMenu != null)
        {
            mainMenu.SetActive(true);
            settingMenu.SetActive(false);
        }     
    }
}

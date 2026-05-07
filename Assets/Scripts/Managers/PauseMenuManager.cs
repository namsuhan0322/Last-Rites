using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(-100)]
public class PauseMenuManager : MonoBehaviour
{
    [Header("퍼즈 UI 패널")]
    public GameObject pausePanel;
    public GameObject settingPanel;

    [Header("버튼 리스트 (4개)")]
    public List<PopupPauseUI> pauseButtons;

    private void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingPanel != null) settingPanel.SetActive(false);

        foreach (var btn in pauseButtons)
        {
            btn.Initialize(OnPauseButtonClicked);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingPanel != null && settingPanel.activeSelf)
            {
                if (UISystemPopup.Instance != null && UISystemPopup.Instance.popupPanel.activeSelf)
                {
                    UISystemPopup.Instance.ClosePopup();
                }
                else
                {
                    TryCloseSettings();
                }

                return;
            }

            if (GameManager.Instance.currentGameState == GameState.Playing)
            {
                if (GameManager.Instance.isInteractUIOpen) return;

                ShowPauseMenu();
            }
            else if (GameManager.Instance.currentGameState == GameState.Paused && pausePanel.activeSelf)
            {
                HidePauseMenu();
            }
        }

        if (settingPanel != null && settingPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                SaveManager.Instance.TrySave();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                SaveManager.Instance.TryReset();
            }
        }
    }

    public void ShowPauseMenu()
    {
        foreach (var btn in pauseButtons)
        {
            btn.SetSelectedState(false);
        }

        GameManager.Instance.PauseGame();
        pausePanel.SetActive(true);
    }

    public void HidePauseMenu()
    {
        GameManager.Instance.ResumeGame();
        pausePanel.SetActive(false);
        if (settingPanel != null) settingPanel.SetActive(false);
    }

    private void OnPauseButtonClicked(PauseOption clickedOption)
    {
        switch (clickedOption)
        {
            case PauseOption.Continue:
                HidePauseMenu();
                break;

            case PauseOption.Settings:
                // 퍼즈 메뉴 숨기고 설정창 열기
                pausePanel.SetActive(false);
                settingPanel.SetActive(true);
                break;

            case PauseOption.MainMenu:
                GameManager.Instance.ResumeGame();

                if (DataManager.Instance != null) DataManager.Instance.SaveAllData();
                if (ScenesManager.Instance != null) ScenesManager.Instance.LoadMainMenu();
                break;

            case PauseOption.QuitGame:
                if (DataManager.Instance != null) DataManager.Instance.SaveAllData();
                GameManager.Instance.QuitGame();
                break;
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            if (GameManager.Instance != null && GameManager.Instance.currentGameState == GameState.Playing)
            {
                if (GameManager.Instance.isInteractUIOpen) return;
                ShowPauseMenu();
            }
        }
    }

    #region 설정창 로직 (MainMenuButtonUI에서 이식됨)

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
        if (settingPanel != null)
        {
            settingPanel.SetActive(false);
            pausePanel.SetActive(true);
        }
    }

    #endregion
}
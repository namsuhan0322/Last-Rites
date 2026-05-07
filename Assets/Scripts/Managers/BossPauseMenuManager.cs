using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(-100)]
public class BossPauseMenuManager : MonoBehaviour
{
    [Header("보스전 퍼즈 UI 패널")]
    public GameObject pausePanel;

    [Header("버튼 리스트 (계속, 로비, 메인, 종료)")]
    public List<PopupPauseUI> pauseButtons;

    private void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);

        foreach (var btn in pauseButtons)
        {
            btn.Initialize(OnPauseButtonClicked);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
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
    }

    private void OnPauseButtonClicked(PauseOption clickedOption)
    {
        switch (clickedOption)
        {
            case PauseOption.Continue:
                HidePauseMenu();
                break;

            case PauseOption.Lobby:
                GameManager.Instance.ResumeGame();
                if (DataManager.Instance != null) DataManager.Instance.SaveAllData();
                if (ScenesManager.Instance != null) ScenesManager.Instance.LoadLobbyScene();
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
}
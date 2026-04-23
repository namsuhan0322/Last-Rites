using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UISystemPopup : SingletonMono<UISystemPopup>
{
    protected override bool DontDestroy => false;

    [Header("UI References")]
    public GameObject popupPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI contentText;
    public Button yesButton;
    public Button noButton;

    // 팝업을 띄우는 함수
    public void ShowPopup(string title, string content, Action onYes, Action onNo = null)
    {
        popupPanel.SetActive(true);

        // 텍스트 설정
        titleText.text = title;
        contentText.text = content;

        // 기존 버튼 이벤트 초기화
        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        // '예' 버튼 설정
        yesButton.onClick.AddListener(() =>
        {
            onYes?.Invoke(); // 전달받은 기능 실행
            ClosePopup();    // 팝업 닫기
        });

        // '아니요' 버튼 설정
        noButton.onClick.AddListener(() =>
        {
            onNo?.Invoke();  // 전달받은 기능 실행
            ClosePopup();    // 팝업 닫기
        });
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false);
    }
}
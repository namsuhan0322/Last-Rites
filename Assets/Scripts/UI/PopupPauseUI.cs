using UnityEngine;
using UnityEngine.EventSystems;

public enum PauseOption
{
    Continue,   // 계속하기
    Settings,   // 설정
    MainMenu,   // 메인 메뉴
    QuitGame    // 게임 종료
}

public class PopupPauseUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    public GameObject Select_Bg;
    public GameObject UnSelect_Bg;

    [Header("버튼 기능 설정")]
    public PauseOption myOption;

    private System.Action<PauseOption> onClickCallback;

    public void Initialize(System.Action<PauseOption> callback)
    {
        onClickCallback = callback;
        SetSelectedState(false);
    }

    public void SetSelectedState(bool isSelected)
    {
        if (Select_Bg != null) Select_Bg.SetActive(isSelected);
        if (UnSelect_Bg != null) UnSelect_Bg.SetActive(!isSelected);
    }

    public void OnPointerEnter(PointerEventData eventData) => SetSelectedState(true);
    public void OnPointerExit(PointerEventData eventData) => SetSelectedState(false);

    public void OnPointerClick(PointerEventData eventData)
    {
        onClickCallback?.Invoke(myOption);
    }
}
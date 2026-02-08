using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class GraphicsOptionItem : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Settings Info (New!)")]
    public string optionName;        
    [TextArea] public string optionDescription;

    [Header("UI References")]
    public GameObject selectBg;
    public GameObject noSelectBg;
    public TextMeshProUGUI labelText; 

    [Header("Right Side Control")]
    public Button leftButton;
    public Button rightButton;
    public TextMeshProUGUI valueText;

    private List<string> options = new List<string>();
    private int currentIndex = 0;

    public System.Action<int> onValueChanged;
    public System.Action<GraphicsOptionItem> onSelected;

    public int CurrentIndex => currentIndex;

    // 초기화
    public void Initialize(List<string> optionList, int initialIndex, System.Action<int> callback)
    {
        options = optionList;
        currentIndex = initialIndex;
        onValueChanged = callback;

        // 버튼 리스너 연결
        leftButton.onClick.RemoveAllListeners();
        rightButton.onClick.RemoveAllListeners();

        leftButton.onClick.AddListener(OnLeftClick);
        rightButton.onClick.AddListener(OnRightClick);

        UpdateUI();
    }

    // 좌측 버튼 클릭
    void OnLeftClick()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = options.Count - 1; // 순환 (끝 -> 처음)

        UpdateUI();
        onValueChanged?.Invoke(currentIndex); // 매니저에게 변경 알림

        // 버튼 눌렀을 때도 이 줄을 선택한 것으로 처리
        onSelected?.Invoke(this);
    }

    // 우측 버튼 클릭
    void OnRightClick()
    {
        currentIndex++;
        if (currentIndex >= options.Count) currentIndex = 0; // 순환 (처음 -> 끝)

        UpdateUI();
        onValueChanged?.Invoke(currentIndex); // 매니저에게 변경 알림

        onSelected?.Invoke(this);
    }

    // UI 갱신 (텍스트 변경)
    void UpdateUI()
    {
        if (options.Count > 0)
        {
            valueText.text = options[currentIndex];
        }
    }

    // 선택 상태 변경 (배경 교체)
    public void SetSelectedState(bool isSelected)
    {
        if (selectBg != null) selectBg.SetActive(isSelected);
        if (noSelectBg != null) noSelectBg.SetActive(!isSelected);
    }

    // 마우스 올렸을 때 자동 선택 처리
    public void OnPointerEnter(PointerEventData eventData)
    {
        onSelected?.Invoke(this);
    }

    // 클릭했을 때 선택 처리
    public void OnPointerClick(PointerEventData eventData)
    {
        onSelected?.Invoke(this);
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SliderOptionItem : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Settings Info")]
    public string optionName;
    [TextArea] public string optionDescription;

    [Header("UI References")]
    public GameObject selectBg;
    public GameObject noSelectBg;
    public TextMeshProUGUI labelText;

    [Header("Slider Control")]
    public Slider targetSlider;

    public System.Action<float> onValueChanged;
    public System.Action<SliderOptionItem> onSelected;

    public void Initialize(float initialValue, System.Action<float> callback)
    {
        targetSlider.value = initialValue;
        onValueChanged = callback;

        targetSlider.onValueChanged.RemoveAllListeners();
        targetSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    void OnSliderValueChanged(float value)
    {
        onValueChanged?.Invoke(value);
        onSelected?.Invoke(this);
    }

    // 선택 상태 변경
    public void SetSelectedState(bool isSelected)
    {
        if (selectBg != null) selectBg.SetActive(isSelected);
        if (noSelectBg != null) noSelectBg.SetActive(!isSelected);
    }

    // 마우스 올리거나 클릭 시 선택 처리
    public void OnPointerEnter(PointerEventData eventData) => onSelected?.Invoke(this);
    public void OnPointerClick(PointerEventData eventData) => onSelected?.Invoke(this);
}
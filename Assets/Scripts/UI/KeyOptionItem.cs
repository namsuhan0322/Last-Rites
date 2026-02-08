using UnityEngine;
using UnityEngine.EventSystems;

public class KeyOptionItem : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Key Info")]
    public string keyName;  
    [TextArea] public string keyDescription; 

    [Header("UI References")]
    public GameObject selectBg;   
    public GameObject noSelectBg;   

    public System.Action<KeyOptionItem> onSelected;

    public void Initialize(System.Action<KeyOptionItem> callback)
    {
        onSelected = callback;
        SetSelectedState(false);
    }

    // 선택 상태 변경
    public void SetSelectedState(bool isSelected)
    {
        if (selectBg != null) selectBg.SetActive(isSelected);
        if (noSelectBg != null) noSelectBg.SetActive(!isSelected);
    }

    // 마우스 올렸을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        onSelected?.Invoke(this);
    }

    // 클릭했을 때
    public void OnPointerClick(PointerEventData eventData)
    {
        onSelected?.Invoke(this);
    }
}
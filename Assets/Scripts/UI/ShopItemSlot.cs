using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopItemSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    public GameObject selectBg;    // 선택되었을 때 켜질 테두리/배경 이미지
    public GameObject noSelectBg;  // 평소 배경 이미지
    public Image itemIcon;         // 슬롯 안의 아이템 아이콘

    [Header("Item Info")]
    public string itemName;        // 이 슬롯이 가진 아이템 이름
    public Sprite iconSprite;      // 아이템 아이콘 (현재는 없으면 비워둠)

    // 어떤 데이터(SO)를 담고 있는지 저장해둘 변수 (나중에 캐스팅해서 사용)
    public ScriptableObject itemData;

    // 클릭 시 매니저에게 알릴 콜백
    public System.Action<ShopItemSlot> onSelected;

    public void Initialize(string name, Sprite icon, ScriptableObject data, System.Action<ShopItemSlot> callback)
    {
        itemName = name;
        iconSprite = icon;
        itemData = data;
        onSelected = callback;

        if (itemIcon != null && iconSprite != null)
        {
            itemIcon.sprite = iconSprite;
            itemIcon.enabled = true;
        }

        SetSelectedState(false);
    }

    public void SetSelectedState(bool isSelected)
    {
        if (selectBg != null) selectBg.SetActive(isSelected);
        if (noSelectBg != null) noSelectBg.SetActive(!isSelected);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onSelected?.Invoke(this);
    }
}
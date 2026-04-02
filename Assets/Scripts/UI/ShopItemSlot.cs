using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopItemSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    public GameObject Select_Bg;    // 프리팹 이름과 동일하게
    public GameObject UnSelect_Bg;  // 프리팹 이름과 동일하게
    public Image Item_Icon;         // 자식 오브젝트인 Item_Icon의 Image 컴포넌트

    public ScriptableObject myData { get; private set; }
    private System.Action<ShopItemSlot> onSelectedCallback;

    // 초기화 시 isOwned(보유 여부)를 받아서 실루엣 처리
    public void Initialize(ScriptableObject data, Sprite icon, bool isOwned, System.Action<ShopItemSlot> callback)
    {
        myData = data;
        onSelectedCallback = callback;

        if (Item_Icon != null && icon != null)
        {
            Item_Icon.sprite = icon;
            Item_Icon.enabled = true;

            // [핵심] 미보유 시 검은색 실루엣 처리, 보유 시 원래 색상
            Item_Icon.color = isOwned ? Color.white : Color.black;
        }
        else if (Item_Icon != null)
        {
            Item_Icon.enabled = false;
        }

        SetSelectedState(false);
    }

    public void SetSelectedState(bool isSelected)
    {
        if (Select_Bg != null) Select_Bg.SetActive(isSelected);
        if (UnSelect_Bg != null) UnSelect_Bg.SetActive(!isSelected);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onSelectedCallback?.Invoke(this);
    }
}
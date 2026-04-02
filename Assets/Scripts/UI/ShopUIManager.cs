using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopUIManager : MonoBehaviour
{
    [Header("Center & Right UI")]
    public Image middleItemImage;               // 가운데 띄울 아이템 이미지
    public TextMeshProUGUI rightItemName;       // 오른쪽 띄울 아이템 이름 텍스트
    // public TextMeshProUGUI rightItemDesc;    // 설명 텍스트

    [Header("Slots Array")]
    public List<ShopItemSlot> itemSlots = new List<ShopItemSlot>();

    private ShopItemSlot currentSelectedSlot; // 현재 선택된 슬롯 기억용

    void Start()
    {
        foreach (var slot in itemSlots)
        {
            slot.onSelected = OnSlotSelected;
            slot.SetSelectedState(false);
        }

        ClearDetailPanel();
    }

    // 슬롯이 클릭되었을 때 실행되는 함수
    public void OnSlotSelected(ShopItemSlot clickedSlot)
    {
        if (currentSelectedSlot != null)
        {
            currentSelectedSlot.SetSelectedState(false);
        }

        currentSelectedSlot = clickedSlot;
        currentSelectedSlot.SetSelectedState(true);

        UpdateDetailPanel(currentSelectedSlot);
    }

    private void UpdateDetailPanel(ShopItemSlot slot)
    {
        if (rightItemName != null)
            rightItemName.text = slot.itemName;

        if (middleItemImage != null)
        {
            if (slot.iconSprite != null)
            {
                middleItemImage.sprite = slot.iconSprite;
                middleItemImage.color = Color.white;
            }
            else
            {
                middleItemImage.sprite = null;
                middleItemImage.color = new Color(1f, 1f, 1f, 0.3f);
            }
        }
    }

    private void ClearDetailPanel()
    {
        if (rightItemName != null) rightItemName.text = "아이템을 선택하세요";
        if (middleItemImage != null) middleItemImage.sprite = null;
    }
}
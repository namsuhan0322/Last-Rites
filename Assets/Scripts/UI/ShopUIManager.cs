using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopUIManager : MonoBehaviour
{
    [Header("Database & Prefabs")]
    public ShopRecipeDatabaseSO recipeDatabase;
    public ShopItemSlot slotPrefab;
    public Transform slotContainer;

    [Header("Center & Right UI")]
    public Image middleItemImage;
    public TextMeshProUGUI rightItemName;
    public TextMeshProUGUI rightItemDesc;
    public TextMeshProUGUI requirementText;

    [Header("Panel")]
    public GameObject shopPanel;

    [Header("Popup UI")]
    public GameObject popupPanel;             // Popup Panel 전체 (활성/비활성 용도)
    public Image popupItemImage;              // Item_01 밑에 있는 아이콘 이미지
    public TextMeshProUGUI popupItemName;     // Item_Name
    public TextMeshProUGUI popupItemDesc;     // Item_Dirc
    public Button confirmBtn;                 // 수락 버튼 (Bottom 등)
    public Button cancelBtn;                  // 거절 버튼 (Bottom (1) 등)

    private ShopItemSlot currentSelectedSlot;
    private List<ShopItemSlot> createdSlots = new List<ShopItemSlot>();

    void Start()
    {
        if (recipeDatabase != null) recipeDatabase.Initialize();

        // 팝업 버튼 리스너 연결
        if (confirmBtn != null) confirmBtn.onClick.AddListener(OnConfirmProcessing);
        if (cancelBtn != null) cancelBtn.onClick.AddListener(ClosePopup);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (popupPanel != null) popupPanel.SetActive(false);
        
        GenerateShopSlots();
        ClearDetailPanel();
    }

    void Update()
    {
        if (shopPanel != null && !shopPanel.activeInHierarchy) return;

        if (popupPanel != null && popupPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClosePopup();
                return;
            }
        }

        if (currentSelectedSlot != null && popupPanel != null && !popupPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OpenPopup(currentSelectedSlot);
            }
        }
    }

    private void GenerateShopSlots()
    {
        foreach (Transform child in slotContainer) Destroy(child.gameObject);
        createdSlots.Clear();

        if (recipeDatabase == null || recipeDatabase.recipeSOs.Count == 0) return;

        InventoryData myInventory = null;
        if (InventoryManager.Instance != null)
            myInventory = InventoryManager.Instance.GetCurrentData();

        foreach (ShopRecipeSO recipe in recipeDatabase.recipeSOs)
        {
            if (recipe.MatItem == null || recipe.MatItem.itemType != ItemType.Raw) continue;

            bool isOwned = false;
            if (myInventory != null)
            {
                ItemSlot foundSlot = myInventory.items.Find(x => x.itemID == recipe.MatItem.ItemId);
                if (foundSlot != null && foundSlot.amount > 0) isOwned = true;
            }

            ShopItemSlot newSlot = Instantiate(slotPrefab, slotContainer);
            newSlot.Initialize(recipe, recipe.MatItem.itemIcon, isOwned, OnSlotSelected);
            createdSlots.Add(newSlot);
        }
    }

    public void OnSlotSelected(ShopItemSlot clickedSlot)
    {
        if (currentSelectedSlot != null) currentSelectedSlot.SetSelectedState(false);

        currentSelectedSlot = clickedSlot;
        currentSelectedSlot.SetSelectedState(true);

        UpdateDetailPanel(currentSelectedSlot);
    }

    private void UpdateDetailPanel(ShopItemSlot slot)
    {
        ShopRecipeSO recipe = slot.myData as ShopRecipeSO;
        if (recipe == null) return;

        ItemDataSO rawItem = recipe.MatItem;

        if (rightItemName != null) rightItemName.text = rawItem.itemName;
        if (rightItemDesc != null) rightItemDesc.text = rawItem.Description;

        int myRawAmount = 0;
        int myCurrencyAmount = 0;
        if (InventoryManager.Instance != null)
        {
            InventoryData inv = InventoryManager.Instance.GetCurrentData();
            myCurrencyAmount = inv.currencyAmount;

            ItemSlot s = inv.items.Find(x => x.itemID == rawItem.ItemId);
            if (s != null) myRawAmount = s.amount;
        }

        if (requirementText != null)
        {
            string reqStr = "<color=yellow>[가공 필요 비용]</color>\n";

            string rawColor = myRawAmount >= recipe.Mat_Amt ? "white" : "red";
            reqStr += $"<color={rawColor}>{rawItem.itemName} : {myRawAmount} / {recipe.Mat_Amt}</color>\n";

            if (recipe.CostItem != null)
            {
                string costColor = myCurrencyAmount >= recipe.Cost_Amt ? "white" : "red";
                reqStr += $"<color={costColor}>{recipe.CostItem.itemName} : {myCurrencyAmount} / {recipe.Cost_Amt}</color>";
            }

            requirementText.text = reqStr;
        }

        if (middleItemImage != null)
        {
            if (rawItem.itemIcon != null)
            {
                middleItemImage.sprite = rawItem.itemIcon;
                middleItemImage.color = (myRawAmount > 0) ? Color.white : Color.black;
            }
            else
            {
                middleItemImage.sprite = null;
                middleItemImage.color = new Color(0, 0, 0, 0.5f);
            }
        }
    }

    private void ClearDetailPanel()
    {
        if (rightItemName != null) rightItemName.text = "가공할 재료를 선택하세요";
        if (rightItemDesc != null) rightItemDesc.text = "";
        if (requirementText != null) requirementText.text = "";
        if (middleItemImage != null) middleItemImage.color = new Color(1f, 1f, 1f, 0f);
    }

    private void OpenPopup(ShopItemSlot slot)
    {
        ShopRecipeSO recipe = slot.myData as ShopRecipeSO;
        if (recipe == null) return;

        // 팝업 UI에 선택한 원석 정보 띄우기
        if (popupItemName != null) popupItemName.text = recipe.MatItem.itemName;
        if (popupItemDesc != null) popupItemDesc.text = recipe.MatItem.Description;
        if (popupItemImage != null) popupItemImage.sprite = recipe.MatItem.itemIcon;

        popupPanel.SetActive(true);
    }

    private void ClosePopup()
    {
        popupPanel.SetActive(false);
    }

    private void OnConfirmProcessing()
    {
        if (currentSelectedSlot == null) return;

        ShopRecipeSO recipe = currentSelectedSlot.myData as ShopRecipeSO;
        if (recipe == null) return;

        // 현재 보유량 다시 체크
        InventoryData inv = InventoryManager.Instance.GetCurrentData();
        ItemSlot s = inv.items.Find(x => x.itemID == recipe.MatItem.ItemId);
        int myRawAmount = (s != null) ? s.amount : 0;
        int myCurrencyAmount = inv.currencyAmount;

        // 재료와 재화가 충분한지 검사
        if (myRawAmount >= recipe.Mat_Amt && myCurrencyAmount >= recipe.Cost_Amt)
        {
            InventoryManager.Instance.AddItem(recipe.MatItem.ItemId, -recipe.Mat_Amt);
            if (recipe.CostItem != null && recipe.Cost_Amt > 0)
            {
                InventoryManager.Instance.AddItem(recipe.CostItem.ItemId, -recipe.Cost_Amt);
            }

            InventoryManager.Instance.AddItem(recipe.ResultItem.ItemId, recipe.Result_Amt);

            DataManager.Instance.SaveAllData();
            Debug.Log($"<color=cyan>[가공 성공] {recipe.ResultItem.itemName}을(를) {recipe.Result_Amt}개 획득했습니다!</color>");

            GenerateShopSlots();
            UpdateDetailPanel(currentSelectedSlot);
            ClosePopup();
        }
        else
        {
            Debug.Log("<color=red>[가공 실패] 재료 또는 재화가 부족합니다.</color>");
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopAIUnlockManager : MonoBehaviour
{
    [Header("Database & Prefabs")]
    public ShopAIUnlockDatabaseSO aiUnlockDatabase;
    public AIDatabaseSO aiDatabase;
    public ShopItemSlot slotPrefab;
    public Transform slotContainer;

    [Header("Center & Right UI")]
    public Image middleAiImage;
    public TextMeshProUGUI rightAiName;
    public TextMeshProUGUI rightAiDesc;
    public TextMeshProUGUI rightAiStats;
    public TextMeshProUGUI requirementText;

    [Header("Panel")]
    public GameObject shopPanel;

    [Header("Popup UI")]
    public GameObject popupPanel;
    public Image popupAiImage;
    public TextMeshProUGUI popupAiName;
    public TextMeshProUGUI popupAiDesc;
    public Button confirmBtn;
    public Button cancelBtn;

    private ShopItemSlot currentSelectedSlot;
    private List<ShopItemSlot> createdSlots = new List<ShopItemSlot>();

    void Start()
    {
        Debug.Log($"ShopAIUnklockManager Start {Time.realtimeSinceStartup}");

        if (confirmBtn != null) confirmBtn.onClick.AddListener(OnConfirmUnlock);
        if (cancelBtn != null) cancelBtn.onClick.AddListener(ClosePopup);

        if (shopPanel != null) shopPanel.SetActive(false);
        if (popupPanel != null) popupPanel.SetActive(false);

        if (aiDatabase != null) aiDatabase.Initialize();

        GenerateShopSlots();
        ClearDetailPanel();
    }

    void Update()
    {
        if (slotContainer != null && !slotContainer.gameObject.activeInHierarchy) return;

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
        float start = Time.realtimeSinceStartup;

        Debug.Log(
            $"GenerateShopSlots Count:{createdSlots.Count} " +
            $"Time:{Time.realtimeSinceStartup - start:F3}"
        );

        foreach (Transform child in slotContainer) Destroy(child.gameObject);
        createdSlots.Clear();

        if (aiUnlockDatabase == null || aiUnlockDatabase.aiUnlockSOs.Count == 0)
        {
            Debug.LogError("<color=red>[오류] 해금 DB(ShopAIUnlockDatabase)가 비어있거나 연결되지 않았습니다!</color>");
            return;
        }

        InventoryData myInventory = null;
        if (InventoryManager.Instance != null)
            myInventory = InventoryManager.Instance.GetCurrentData();

        foreach (ShopAIUnlockSO unlockData in aiUnlockDatabase.aiUnlockSOs)
        {
            AISO aiData = GetAISOById(unlockData.AI_Id);

            if (aiData == null)
            {
                Debug.LogWarning($"<color=orange>[스킵됨]</color> 상점 데이터 '{unlockData.name}'가 요구하는 <color=yellow>AI_Id : [{unlockData.AI_Id}]</color>를 AIDatabase에서 찾을 수 없습니다!");
                continue;
            }

            int mat1Amount = GetItemAmount(myInventory, unlockData.Req_Mat_1?.ItemId);
            int mat2Amount = GetItemAmount(myInventory, unlockData.Req_Mat_2?.ItemId);

            bool canUnlock = true;
            if (unlockData.Req_Mat_1 != null && mat1Amount < unlockData.Req_Mat_Amt_1) canUnlock = false;
            if (unlockData.Req_Mat_2 != null && mat2Amount < unlockData.Req_Mat_Amt_2) canUnlock = false;

            ShopItemSlot newSlot = Instantiate(slotPrefab, slotContainer);
            newSlot.Initialize(unlockData, aiData.aiIcon, canUnlock, OnSlotSelected);
            createdSlots.Add(newSlot);
        }

        Debug.Log($"<color=cyan>[완료] 총 {createdSlots.Count}개의 AI 슬롯이 생성되었습니다.</color>");
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
        ShopAIUnlockSO unlockData = slot.myData as ShopAIUnlockSO;
        if (unlockData == null) return;

        AISO aiData = GetAISOById(unlockData.AI_Id);
        if (aiData == null) return;

        if (rightAiName != null) rightAiName.text = aiData.name;
        if (rightAiDesc != null) rightAiDesc.text = aiData.description;

        if (rightAiStats != null)
        {
            rightAiStats.text = $"<color=#00FF00>[{aiData.roleType}]</color>  HP: {aiData.Hp}  |  ATK: {aiData.Atk}";
        }

        InventoryData inv = InventoryManager.Instance != null ? InventoryManager.Instance.GetCurrentData() : new InventoryData();
        int mat1Amount = GetItemAmount(inv, unlockData.Req_Mat_1?.ItemId);
        int mat2Amount = GetItemAmount(inv, unlockData.Req_Mat_2?.ItemId);

        bool canUnlock = true;

        if (requirementText != null)
        {
            string reqStr = "";

            if (!string.IsNullOrEmpty(unlockData.Unlock_Conditon))
            {
                reqStr += $"<color=orange>[해금 조건]</color>\n- {unlockData.Unlock_Conditon}\n\n";
            }

            reqStr += "<color=yellow>[해금 필요 재료]</color>\n";

            if (unlockData.Req_Mat_1 != null && unlockData.Req_Mat_Amt_1 > 0)
            {
                if (mat1Amount < unlockData.Req_Mat_Amt_1) canUnlock = false;
                string color1 = mat1Amount >= unlockData.Req_Mat_Amt_1 ? "white" : "red";
                reqStr += $"<color={color1}>{unlockData.Req_Mat_1.itemName} : {mat1Amount} / {unlockData.Req_Mat_Amt_1}</color>\n";
            }

            if (unlockData.Req_Mat_2 != null && unlockData.Req_Mat_Amt_2 > 0)
            {
                if (mat2Amount < unlockData.Req_Mat_Amt_2) canUnlock = false;
                string color2 = mat2Amount >= unlockData.Req_Mat_Amt_2 ? "white" : "red";
                reqStr += $"<color={color2}>{unlockData.Req_Mat_2.itemName} : {mat2Amount} / {unlockData.Req_Mat_Amt_2}</color>";
            }

            requirementText.text = reqStr;
        }

        // [핵심 2] 중앙 이미지 실루엣 처리
        if (middleAiImage != null)
        {
            if (aiData.aiIllustration != null)
            {
                middleAiImage.sprite = aiData.aiIllustration;
                middleAiImage.color = canUnlock ? Color.white : Color.black;
            }
            else
            {
                middleAiImage.sprite = null;
                middleAiImage.color = new Color(0, 0, 0, 0.5f);
            }
        }
    }

    private void ClearDetailPanel()
    {
        if (rightAiName != null) rightAiName.text = "동료를 선택하세요";
        if (rightAiDesc != null) rightAiDesc.text = "";
        if (rightAiStats != null) rightAiStats.text = "";
        if (requirementText != null) requirementText.text = "";
        if (middleAiImage != null) middleAiImage.color = new Color(1f, 1f, 1f, 0f);
    }

    private void OpenPopup(ShopItemSlot slot)
    {
        ShopAIUnlockSO unlockData = slot.myData as ShopAIUnlockSO;
        if (unlockData == null) return;

        AISO aiData = GetAISOById(unlockData.AI_Id);
        if (aiData == null) return;

        if (popupAiName != null) popupAiName.text = $"{aiData.name} 해금";
        if (popupAiDesc != null) popupAiDesc.text = "해당 동료를 정말로 해금하시겠습니까?";
        if (popupAiImage != null) popupAiImage.sprite = aiData.aiIcon;

        popupPanel.SetActive(true);
    }

    private void ClosePopup()
    {
        popupPanel.SetActive(false);
    }

    private void OnConfirmUnlock()
    {
        if (currentSelectedSlot == null) return;

        ShopAIUnlockSO unlockData = currentSelectedSlot.myData as ShopAIUnlockSO;
        if (unlockData == null) return;

        InventoryData inv = InventoryManager.Instance.GetCurrentData();
        int mat1Amount = GetItemAmount(inv, unlockData.Req_Mat_1?.ItemId);
        int mat2Amount = GetItemAmount(inv, unlockData.Req_Mat_2?.ItemId);

        if (mat1Amount >= unlockData.Req_Mat_Amt_1 && mat2Amount >= unlockData.Req_Mat_Amt_2)
        {
            if (unlockData.Req_Mat_1 != null && unlockData.Req_Mat_Amt_1 > 0)
                InventoryManager.Instance.AddItem(unlockData.Req_Mat_1.ItemId, -unlockData.Req_Mat_Amt_1);
            if (unlockData.Req_Mat_2 != null && unlockData.Req_Mat_Amt_2 > 0)
                InventoryManager.Instance.AddItem(unlockData.Req_Mat_2.ItemId, -unlockData.Req_Mat_Amt_2);

            InventoryManager.Instance.AddItem(unlockData.AI_Id, 1);

            DataManager.Instance.SaveAllData();
            Debug.Log($"<color=cyan>[해금 성공] 동료 '{unlockData.AI_Id}'가 합류했습니다!</color>");

            GenerateShopSlots(); // UI 실루엣 상태 갱신을 위해 슬롯 다시 생성
            UpdateDetailPanel(currentSelectedSlot);
            ClosePopup();
        }
        else
        {
            Debug.Log("<color=red>[해금 실패] 재료가 부족합니다.</color>");
        }
    }

    private AISO GetAISOById(string aiIdStr)
    {
        if (aiDatabase == null || string.IsNullOrEmpty(aiIdStr)) return null;

        string numberOnly = System.Text.RegularExpressions.Regex.Replace(aiIdStr, "[^0-9]", "");

        if (int.TryParse(numberOnly, out int extractedId))
        {
            int offsetId = extractedId < 100 ? extractedId + 99 : extractedId;

            AISO found = aiDatabase.GetItemById(offsetId);
            if (found != null) return found;

            found = aiDatabase.GetItemById(extractedId);
            if (found != null) return found;
        }

        return aiDatabase.GetItemByName(aiIdStr);
    }

    private int GetItemAmount(InventoryData inv, string itemId)
    {
        if (inv == null || string.IsNullOrEmpty(itemId)) return 0;
        ItemSlot slot = inv.items.Find(x => x.itemID == itemId);
        return slot != null ? slot.amount : 0;
    }
}
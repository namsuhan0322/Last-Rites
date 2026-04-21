using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InfusionUIManager : MonoBehaviour
{
    [Header("데이터베이스 & 프리팹")]
    public BlacksmithInfusionDatabaseSO infusionDB;
    public ShopItemSlot slotPrefab;
    public Transform slotContainer;

    [Header("중앙 & 우측 UI")]
    public Image middleIconImage;
    public TextMeshProUGUI rightNameText;
    public TextMeshProUGUI rightEffectText; // Effect_Value 출력용
    public TextMeshProUGUI rightDescText;   // Effect_Desc 출력용
    public TextMeshProUGUI requirementText; // 필요 소울 출력용

    [Header("팝업 UI")]
    public GameObject popupPanel;
    public Image popupIconImage;
    public TextMeshProUGUI popupNameText;
    public TextMeshProUGUI popupDescText;
    public Button confirmBtn;
    public Button cancelBtn;
    public TextMeshProUGUI confirmBtnText;
    public TextMeshProUGUI cancelBtnText;

    private ShopItemSlot _currentSelectedSlot;
    private List<ShopItemSlot> _createdSlots = new List<ShopItemSlot>();
    private InventoryData _invData;

    void Start()
    {
        ClearDetailPanel();

        if (infusionDB != null) infusionDB.Initialize();
        if (confirmBtn != null) confirmBtn.onClick.AddListener(OnConfirmImbue);
        if (cancelBtn != null) cancelBtn.onClick.AddListener(ClosePopup);

        if (confirmBtnText != null) confirmBtnText.text = "부여하기";
        if (cancelBtnText != null) cancelBtnText.text = "부여 안하기";

        if (popupPanel != null) popupPanel.SetActive(false);

        GenerateSlots();
    }

    void OnEnable()
    {
        if (InventoryManager.Instance == null || InventoryManager.Instance.GetCurrentData() == null) return;

        GenerateSlots();
        ClearDetailPanel();

        if (popupPanel != null) popupPanel.SetActive(false);
    }

    void Update()
    {
        if (slotContainer != null && !slotContainer.gameObject.activeInHierarchy) return;

        if (_currentSelectedSlot != null && popupPanel != null && !popupPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OpenPopup();
            }
        }
    }

    private void GenerateSlots()
    {
        foreach (Transform child in slotContainer) Destroy(child.gameObject);
        _createdSlots.Clear();

        if (InventoryManager.Instance != null)
            _invData = InventoryManager.Instance.GetCurrentData();

        if (_invData == null) return;
        if (infusionDB == null) return;
        if (infusionDB.infusionSOs.Count == 0) return;

        string currentWeaponIDStr = _invData.equippedWeaponID.ToString();

        List<BlacksmithInfusionSO> specificInfusions = infusionDB.GetInfusionsForWeapon(currentWeaponIDStr);
        List<BlacksmithInfusionSO> allWeaponInfusions = infusionDB.GetInfusionsForWeapon("ALL");

        if (allWeaponInfusions == null || allWeaponInfusions.Count == 0)
        {
            allWeaponInfusions = infusionDB.GetInfusionsForWeapon("All");
        }

        List<BlacksmithInfusionSO> availableInfusions = new List<BlacksmithInfusionSO>();
        if (specificInfusions != null) availableInfusions.AddRange(specificInfusions);
        if (allWeaponInfusions != null) availableInfusions.AddRange(allWeaponInfusions);

        foreach (BlacksmithInfusionSO infusion in availableInfusions)
        {
            bool hasSoul = InventoryManager.Instance.GetItemAmount(infusion.Soul_Id) > 0;

            ShopItemSlot newSlot = Instantiate(slotPrefab, slotContainer);
            newSlot.Initialize(infusion, infusion.infusionIcon, hasSoul, OnSlotSelected);
            _createdSlots.Add(newSlot);
        }
    }

    public void OnSlotSelected(ShopItemSlot clickedSlot)
    {
        if (_currentSelectedSlot != null) _currentSelectedSlot.SetSelectedState(false);

        _currentSelectedSlot = clickedSlot;
        _currentSelectedSlot.SetSelectedState(true);

        UpdateDetailPanel();
    }

    private void UpdateDetailPanel()
    {
        BlacksmithInfusionSO infusion = _currentSelectedSlot.myData as BlacksmithInfusionSO;
        if (infusion == null) return;

        if (rightNameText != null) rightNameText.text = infusion.infusionName;
        if (rightEffectText != null) rightEffectText.text = $"<color=#00FF00>[효과] {infusion.Effect_Value}</color>";
        if (rightDescText != null) rightDescText.text = infusion.Effect_Desc;

        if (requirementText != null)
        {
            int soulAmount = InventoryManager.Instance.GetItemAmount(infusion.Soul_Id);
            string colorHex = soulAmount > 0 ? "white" : "red";
            requirementText.text = $"[필요 재료]\n<color={colorHex}>요구 소울 ({infusion.Soul_Id}) : {soulAmount} / 1</color>";
        }

        if (middleIconImage != null)
        {
            middleIconImage.sprite = infusion.infusionIcon;

            int soulAmount = InventoryManager.Instance.GetItemAmount(infusion.Soul_Id);
            middleIconImage.color = soulAmount > 0 ? Color.white : Color.black;
        }
    }

    private void ClearDetailPanel()
    {
        if (_currentSelectedSlot != null) _currentSelectedSlot.SetSelectedState(false);
        _currentSelectedSlot = null;

        if (rightNameText != null) rightNameText.text = "부여할 속성을 선택하세요";
        if (rightEffectText != null) rightEffectText.text = "";
        if (rightDescText != null) rightDescText.text = "";
        if (requirementText != null) requirementText.text = "";
        if (middleIconImage != null) middleIconImage.color = new Color(1, 1, 1, 0);
    }

    private void OpenPopup()
    {
        BlacksmithInfusionSO infusion = _currentSelectedSlot.myData as BlacksmithInfusionSO;
        if (infusion == null) return;

        if (InventoryManager.Instance.GetItemAmount(infusion.Soul_Id) <= 0)
        {
            Debug.LogWarning("[속성 부여] 필요 소울이 부족합니다!");
            return;
        }

        if (popupNameText != null) popupNameText.text = $"{infusion.infusionName} 부여";
        if (popupDescText != null) popupDescText.text = "이 속성을 무기에 부여하시겠습니까?\n<color=red>(사용된 소울은 소모됩니다.)</color>";
        if (popupIconImage != null) popupIconImage.sprite = infusion.infusionIcon;

        popupPanel.SetActive(true);
    }

    private void ClosePopup()
    {
        popupPanel.SetActive(false);
    }

    private void OnConfirmImbue()
    {
        if (_currentSelectedSlot == null) return;

        BlacksmithInfusionSO infusion = _currentSelectedSlot.myData as BlacksmithInfusionSO;
        if (infusion == null) return;

        // 1. 소울 소모 (-1개)
        InventoryManager.Instance.AddItem(infusion.Soul_Id, -1);

        // 2. 인벤토리 데이터에 속성(인퓨전) 부여 기록
        InventoryData inv = InventoryManager.Instance.GetCurrentData();
        inv.equippedAttributeID = infusion.infusionName;

        // 3. 마스터 세이브
        DataManager.Instance.SaveAllData();
        Debug.Log($"<color=cyan>[속성 부여 성공] 장착된 무기에 '{infusion.infusionName}'(이)가 부여되었습니다!</color>");

        ClosePopup();
        GenerateSlots(); // 보유 소울 개수가 변했으므로 슬롯 갱신
        ClearDetailPanel();
    }
}
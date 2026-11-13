using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// [역할] 정비대 무기 교체 탭: 무기 슬롯 목록, 오른쪽 상세, 교체 확인 팝업(엔터 → 확인으로 장착).
/// [이유] 확인 팝업(Popup Panel)과 확인 버튼을 AI 해금·맵 이동과 같이 써서, 확인을 누르면 세 처리가 모두 불린다.
///        확인 처리는 내 목록이 화면에 보일 때만 한다(엔터 키 처리와 같은 기준).
/// [근거] 로비 UI 팝업 명세 2.5·3.4, 결정 P10
/// </summary>
public class WeaponLoadoutManager : MonoBehaviour
{
    [Header("데이터베이스 & 프리팹")]
    [Tooltip("게임에 존재하는 4종의 무기 SO를 모두 드래그해서 넣으세요.")]
    public List<WeaponSO> allWeapons;

    public ShopItemSlot weaponSlotPrefab;
    public Transform contentContainer;

    [Header("우측 패널 (상세 정보)")]
    public Image rightWeaponIcon;
    public TextMeshProUGUI rightWeaponName;
    public TextMeshProUGUI rightWeaponDesc;

    [Header("팝업 UI (교체 확인)")]
    public GameObject popupPanel;
    public Image popupWeaponImage;
    public TextMeshProUGUI popupWeaponName;
    public TextMeshProUGUI popupWeaponDesc;
    public Button confirmBtn;
    public Button cancelBtn;
    public TextMeshProUGUI confirmBtnText;
    public TextMeshProUGUI cancelBtnText;

    private PlayerController _player;
    private ShopItemSlot _currentSelectedSlot;
    private List<ShopItemSlot> _createdSlots = new List<ShopItemSlot>();

    void Start()
    {
        Debug.Log($"WeaponLoadoutManager Start {Time.realtimeSinceStartup}");

        _player = FindFirstObjectByType<PlayerController>();

        // 팝업 버튼 리스너 연결
        if (confirmBtn != null) confirmBtn.onClick.AddListener(OnConfirmEquip);
        if (cancelBtn != null) cancelBtn.onClick.AddListener(ClosePopup);
        if (popupPanel != null) popupPanel.SetActive(false);

        GenerateWeaponSlots();
    }

    private void OnEnable()
    {
        ClearRightPanel();
        if (popupPanel != null) popupPanel.SetActive(false);
    }

    void Update()
    {
        if (contentContainer != null && !contentContainer.gameObject.activeInHierarchy) return;

        if (popupPanel != null && popupPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClosePopup();
                return;
            }
        }

        if (_currentSelectedSlot != null && popupPanel != null && !popupPanel.activeSelf)
        {
            WeaponSO weaponData = _currentSelectedSlot.myData as WeaponSO;
            bool isAlreadyEquipped = (_player != null && _player.CurrentWeapon == weaponData);

            if (!isAlreadyEquipped && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            {
                OpenPopup(weaponData);
            }
        }
    }

    private void GenerateWeaponSlots()
    {
        foreach (Transform child in contentContainer) Destroy(child.gameObject);
        _createdSlots.Clear();

        foreach (WeaponSO weapon in allWeapons)
        {
            ShopItemSlot newSlot = Instantiate(weaponSlotPrefab, contentContainer);
            newSlot.Initialize(weapon, weapon.weaponIcon, true, OnSlotSelected);
            _createdSlots.Add(newSlot);
        }
    }

    private void OnSlotSelected(ShopItemSlot clickedSlot)
    {
        if (_currentSelectedSlot != null)
            _currentSelectedSlot.SetSelectedState(false);

        _currentSelectedSlot = clickedSlot;
        _currentSelectedSlot.SetSelectedState(true);

        WeaponSO weaponData = _currentSelectedSlot.myData as WeaponSO;
        UpdateRightPanel(weaponData);
    }

    private void UpdateRightPanel(WeaponSO data)
    {
        if (data == null) return;

        if (rightWeaponName != null) rightWeaponName.text = data.name;
        if (rightWeaponDesc != null) rightWeaponDesc.text = data.description;

        if (rightWeaponIcon != null)
        {
            rightWeaponIcon.sprite = data.weaponIcon;
            rightWeaponIcon.color = Color.white;
        }
    }

    private void ClearRightPanel()
    {
        if (_currentSelectedSlot != null) _currentSelectedSlot.SetSelectedState(false);
        _currentSelectedSlot = null;

        if (rightWeaponName != null) rightWeaponName.text = "무기를 선택하세요";
        if (rightWeaponDesc != null) rightWeaponDesc.text = "";
        if (rightWeaponIcon != null) rightWeaponIcon.color = new Color(1, 1, 1, 0);
    }

    private void OpenPopup(WeaponSO data)
    {
        if (data == null) return;
        if (confirmBtnText != null) confirmBtnText.text = "장착하기";
        if (cancelBtnText != null) cancelBtnText.text = "취소";
        if (popupWeaponName != null) popupWeaponName.text = data.name;
        if (popupWeaponDesc != null) popupWeaponDesc.text = "이 무기로 장착하시겠습니까?";
        if (popupWeaponImage != null) popupWeaponImage.sprite = data.weaponIcon;

        popupPanel.SetActive(true);
    }

    private void ClosePopup()
    {
        if (popupPanel != null) popupPanel.SetActive(false);
    }

    private void OnConfirmEquip()
    {
        // 같이 쓰는 확인 버튼 — 내 목록이 화면에 보일 때만 (결정 P10)
        if (contentContainer == null || !contentContainer.gameObject.activeInHierarchy) return;
        if (_currentSelectedSlot == null || _player == null) return;

        WeaponSO weaponData = _currentSelectedSlot.myData as WeaponSO;
        if (weaponData == null) return;

        _player.ChangeWeapon(weaponData);
        UpdateRightPanel(weaponData);
        ClosePopup();
    }
}
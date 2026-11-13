using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// [역할] 맵 이동 탭: 보스 스테이지 슬롯 목록, 오른쪽 상세, 이동 확인 팝업(확인 → 저장 후 그 씬으로 이동).
/// [이유] 확인 팝업(Popup Panel)과 확인 버튼을 무기 교체·AI 해금과 같이 써서, 확인을 누르면 세 처리가 모두 불린다.
///        고른 슬롯은 창을 닫아도 남으므로, 확인 처리는 내 목록이 화면에 보일 때만 한다(엔터 키 처리와 같은 기준).
/// [근거] 로비 UI 팝업 명세 2.5·3.4, 결정 P10
/// </summary>
public class MapTeleportManager : MonoBehaviour
{
    [Header("데이터 및 프리팹")]
    public List<BossStageSO> bossStages;
    public BossSelectSlot slotPrefab;
    public Transform slotContainer;

    [Header("우측 상세 패널")]
    public TextMeshProUGUI rightBossName;
    public TextMeshProUGUI rightBossDesc;
    public Image rightBossImage;

    [Header("팝업 UI")]
    public GameObject popupPanel;
    public Image popupBossImage;
    public TextMeshProUGUI popupBossName;
    public TextMeshProUGUI popupBossDesc;
    public Button confirmBtn;
    public Button cancelBtn;

    public TextMeshProUGUI confirmBtnText;
    public TextMeshProUGUI cancelBtnText;

    private BossSelectSlot _currentSelectedSlot;
    private List<BossSelectSlot> _createdSlots = new List<BossSelectSlot>();

    void Start()
    {
        if (confirmBtn != null) confirmBtn.onClick.AddListener(OnConfirmTeleport);
        if (cancelBtn != null) cancelBtn.onClick.AddListener(ClosePopup);

        if (popupPanel != null) popupPanel.SetActive(false);

        GenerateSlots();
        ClearDetailPanel();
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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (popupPanel != null && popupPanel.activeSelf)
            {
                ClosePopup();
                return;
            }
        }

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

        foreach (BossStageSO stage in bossStages)
        {
            BossSelectSlot newSlot = Instantiate(slotPrefab, slotContainer);
            newSlot.Initialize(stage, OnSlotSelected);
            _createdSlots.Add(newSlot);
        }
    }

    public void OnSlotSelected(BossSelectSlot clickedSlot)
    {
        if (_currentSelectedSlot != null) _currentSelectedSlot.SetSelectedState(false);

        _currentSelectedSlot = clickedSlot;
        _currentSelectedSlot.SetSelectedState(true);

        UpdateDetailPanel();
    }

    private void UpdateDetailPanel()
    {
        BossStageSO data = _currentSelectedSlot.myData;
        if (rightBossName != null) rightBossName.text = data.bossName;
        if (rightBossDesc != null) rightBossDesc.text = data.description;
        if (rightBossImage != null)
        {
            rightBossImage.sprite = data.bossLargeImage;
            rightBossImage.color = Color.white;
        }
    }

    private void ClearDetailPanel()
    {
        if (_currentSelectedSlot != null) _currentSelectedSlot.SetSelectedState(false);
        _currentSelectedSlot = null;

        if (rightBossName != null) rightBossName.text = "이동할 지역을 선택하세요";
        if (rightBossDesc != null) rightBossDesc.text = "";
        if (rightBossImage != null) rightBossImage.color = new Color(1, 1, 1, 0);
    }

    private void OpenPopup()
    {
        if (confirmBtnText != null) confirmBtnText.text = "확인";
        if (cancelBtnText != null) cancelBtnText.text = "나가기";

        BossStageSO data = _currentSelectedSlot.myData;
        if (popupBossName != null) popupBossName.text = $"{data.bossName} 토벌";
        if (popupBossDesc != null) popupBossDesc.text = $"이 지역으로 이동하여 토벌을 시작하시겠습니까?";
        if (popupBossImage != null) popupBossImage.sprite = data.bossLargeImage;

        popupPanel.SetActive(true);
    }

    private void ClosePopup() => popupPanel.SetActive(false);

    private void OnConfirmTeleport()
    {
        // 같이 쓰는 확인 버튼 — 내 목록이 화면에 보일 때만 (결정 P10)
        if (slotContainer == null || !slotContainer.gameObject.activeInHierarchy) return;
        if (_currentSelectedSlot == null) return;

        string targetScene = _currentSelectedSlot.myData.sceneName;

        DataManager.Instance.SaveAllData();

        if (ScenesManager.Instance != null)
        {
            ScenesManager.Instance.LoadScene(targetScene);
        }
        else
        {
            SceneManager.LoadScene(targetScene);
        }

        ClosePopup();
    }
}
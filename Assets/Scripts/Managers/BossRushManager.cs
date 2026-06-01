using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class BossRushManager : MonoBehaviour
{
    [Header("보스러쉬 슬롯 프리팹")]
    public BossRushSlot bossRushSlotPrefab;
    public Transform slotContainer;

    [Header("이동할 씬 이름")]
    public string bossRushSceneName = "BossRushScene";

    [Header("우측 상세 패널")]
    public TextMeshProUGUI rightBossName;
    public TextMeshProUGUI rightBossDesc;

    [Header("팝업 UI")]
    public GameObject popupPanel;
    public TextMeshProUGUI popupTitleText;
    public TextMeshProUGUI popupDescText;
    public Button confirmBtn;
    public Button cancelBtn;

    private BossRushSlot _spawnedSlot;

    private bool _isSlotSelected = false;

    void Start()
    {
        if (bossRushSlotPrefab != null && slotContainer != null)
        {
            _spawnedSlot = Instantiate(bossRushSlotPrefab, slotContainer);
            _spawnedSlot.Initialize(OnSlotClicked);
        }

        if (confirmBtn != null) confirmBtn.onClick.AddListener(OnConfirmEnter);
        if (cancelBtn != null) cancelBtn.onClick.AddListener(ClosePopup);

        ClearDetailPanel();
        if (popupPanel != null) popupPanel.SetActive(false);
    }

    void OnEnable()
    {
        if (_spawnedSlot != null) _spawnedSlot.SetSelectedState(false);
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
        if (_isSlotSelected && popupPanel != null && !popupPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OpenPopup();
            }
        }
    }

    private void OnSlotClicked()
    {
        _isSlotSelected = true;
        if (_spawnedSlot != null) _spawnedSlot.SetSelectedState(true);
        UpdateDetailPanel();
        OpenPopup();
    }

    private void UpdateDetailPanel()
    {
        if (rightBossName != null) rightBossName.text = "연속 전투 보스러쉬";
        if (rightBossDesc != null) rightBossDesc.text = "지금까지 만났던 강력한 보스들과 연속으로 전투를 진행합니다.\n자신의 한계에 도전하고 특별한 보상을 획득하세요.";
    }

    private void ClearDetailPanel()
    {
        _isSlotSelected = false;
        if (rightBossName != null) rightBossName.text = "도전할 콘텐츠를 선택하세요";
        if (rightBossDesc != null) rightBossDesc.text = "";
    }

    private void OpenPopup()
    {
        if (popupTitleText != null) popupTitleText.text = "연속 전투";
        if (popupDescText != null) popupDescText.text = "연속 전투 보스러쉬를\n시작하시겠습니까?";

        if (popupPanel != null) popupPanel.SetActive(true);
    }

    private void ClosePopup()
    {
        if (_spawnedSlot != null) _spawnedSlot.SetSelectedState(false);
        ClearDetailPanel();
        if (popupPanel != null) popupPanel.SetActive(false);
    }

    private void OnConfirmEnter()
    {
        if (DataManager.Instance != null) DataManager.Instance.SaveAllData();

        if (ScenesManager.Instance != null) ScenesManager.Instance.LoadScene(bossRushSceneName);
        else SceneManager.LoadScene(bossRushSceneName);

        ClosePopup();
    }
}
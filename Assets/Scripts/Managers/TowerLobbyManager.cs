using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TowerLobbyManager : MonoBehaviour
{
    [Header("데이터 및 프리팹")]
    public List<TowerFloorSO> towerFloors;
    public TowerFloorSlot slotPrefab;
    public Transform slotContainer;

    [Header("이동할 씬 이름")]
    public string towerSceneName = "TowerScene";

    [Header("우측 상세 패널")]
    public TextMeshProUGUI rightFloorName;
    public TextMeshProUGUI rightFloorDesc;
    public Image rightFloorImage;

    [Header("팝업 UI")]
    public GameObject popupPanel;
    public Image popupFloorImage;
    public TextMeshProUGUI popupFloorName;
    public TextMeshProUGUI popupFloorDesc;
    public Button confirmBtn;
    public Button cancelBtn;

    public TextMeshProUGUI confirmBtnText;
    public TextMeshProUGUI cancelBtnText;

    private TowerFloorSlot _currentSelectedSlot;
    private List<TowerFloorSlot> _createdSlots = new List<TowerFloorSlot>();

    void Start()
    {
        if (confirmBtn != null)
            confirmBtn.onClick.AddListener(OnConfirmEnter);

        if (cancelBtn != null)
            cancelBtn.onClick.AddListener(ClosePopup);

        if (popupPanel != null)
            popupPanel.SetActive(false);

        GenerateSlots();
        ClearDetailPanel();
    }

    void OnEnable()
    {
        GenerateSlots();
        ClearDetailPanel();

        if (popupPanel != null)
            popupPanel.SetActive(false);
    }

    void Update()
    {
        if (slotContainer != null && !slotContainer.gameObject.activeInHierarchy)
            return;

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
        if (slotContainer == null || slotPrefab == null)
            return;

        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        _createdSlots.Clear();

        foreach (TowerFloorSO floor in towerFloors)
        {
            if (floor == null)
            {
                Debug.LogWarning("TowerFloorSO가 비어있는 칸이 있습니다.");
                continue;
            }

            TowerFloorSlot newSlot = Instantiate(slotPrefab, slotContainer);
            newSlot.Initialize(floor, OnSlotSelected);
            _createdSlots.Add(newSlot);
        }
    }

    public void OnSlotSelected(TowerFloorSlot clickedSlot)
    {
        if (_currentSelectedSlot != null)
            _currentSelectedSlot.SetSelectedState(false);

        _currentSelectedSlot = clickedSlot;
        _currentSelectedSlot.SetSelectedState(true);

        UpdateDetailPanel();
    }

    private void UpdateDetailPanel()
    {
        TowerFloorSO data = _currentSelectedSlot.myData;

        if (rightFloorName != null)
            rightFloorName.text = data.floorName;

        if (rightFloorDesc != null)
            rightFloorDesc.text = data.description;

        if (rightFloorImage != null)
        {
            rightFloorImage.sprite = data.floorLargeImage;
            rightFloorImage.color = Color.white;
        }
    }

    private void ClearDetailPanel()
    {
        if (_currentSelectedSlot != null)
            _currentSelectedSlot.SetSelectedState(false);

        _currentSelectedSlot = null;

        if (rightFloorName != null)
            rightFloorName.text = "타워를 올라가세요!";

        if (rightFloorDesc != null)
            rightFloorDesc.text = "자원을 얻기 위한 타워";

        if (rightFloorImage != null)
            rightFloorImage.color = Color.white;
    }

    private void OpenPopup()
    {
        if (_currentSelectedSlot == null)
            return;

        if (confirmBtnText != null)
            confirmBtnText.text = "확인";

        if (cancelBtnText != null)
            cancelBtnText.text = "나가기";

        TowerFloorSO data = _currentSelectedSlot.myData;

        if (popupFloorName != null)
            popupFloorName.text = $"{data.floorName} 입장";

        if (popupFloorDesc != null)
            popupFloorDesc.text = $"{data.floorName}에 도전하시겠습니까?";

        if (popupFloorImage != null)
            popupFloorImage.sprite = data.floorLargeImage;

        if (popupPanel != null)
            popupPanel.SetActive(true);
    }

    private void ClosePopup()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }

    private void OnConfirmEnter()
    {
        if (_currentSelectedSlot == null)
            return;

        TowerFloorSO data = _currentSelectedSlot.myData;

        TowerManager.Instance.SelectFloor(data.floor);

        if (DataManager.Instance != null)
            DataManager.Instance.SaveAllData();

        if (ScenesManager.Instance != null)
            ScenesManager.Instance.LoadScene(towerSceneName);
        else
            SceneManager.LoadScene(towerSceneName);

        ClosePopup();
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnhancementUIManager : MonoBehaviour
{
    [Header("Databases")]
    public BlacksmithEnhanceDatabaseSO enhanceDB;
    public WeaponDatabaseSO weaponDB;

    [Header("1. Main UI (무기 교체 화면과 유사)")]
    public GameObject mainPanel;
    public Image leftWeaponIcon;
    public TextMeshProUGUI leftLevelText;
    public Image rightWeaponIcon;
    public TextMeshProUGUI rightLevelText;
    public TextMeshProUGUI probText;
    public TextMeshProUGUI reqMaterialText;
    public Button enhanceButton;

    [Header("2. Progress UI (강화 중 팝업)")]
    public GameObject progressPanel;
    public Image progressWeaponIcon;
    public Slider progressBar;
    public TextMeshProUGUI progressText;

    [Header("3. Result UI (강화 결과 팝업)")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultTitleText;
    public Image resultWeaponIcon;
    public TextMeshProUGUI resultLevelText;
    public TextMeshProUGUI resultPityText;
    public Button resultButton;
    public TextMeshProUGUI resultButtonText;

    private InventoryData _invData;
    private BlacksmithEnhanceSO _currentEnhanceData;
    private WeaponSO _equippedWeaponData;
    private bool _isEnhancing = false;
    private int _lastWeaponID = -1;

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
            _invData = InventoryManager.Instance.GetCurrentData();

        if (enhanceDB != null) enhanceDB.Initialize(); // 딕셔너리 초기화

        UpdateMainUI();

        progressPanel.SetActive(false);
        resultPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    private void Start()
    {
        enhanceButton.onClick.AddListener(OnEnhanceButtonClicked);
        resultButton.onClick.AddListener(OnResultButtonClicked);

        UpdateMainUI();
    }

    private void LateUpdate()
    {
        if (_invData != null && _invData.equippedWeaponID != _lastWeaponID)
        {
            _lastWeaponID = _invData.equippedWeaponID;
            UpdateMainUI();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_invData != null && _invData.equippedWeaponID != _lastWeaponID)
            {
                _lastWeaponID = _invData.equippedWeaponID;
                UpdateMainUI();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (resultPanel != null && resultPanel.activeSelf)
                {
                    resultPanel.SetActive(false);
                    mainPanel.SetActive(true);
                    UpdateMainUI();
                }
            }
        }
    }

    private void UpdateMainUI()
    {
        if (InventoryManager.Instance != null)
        {
            _invData = InventoryManager.Instance.GetCurrentData();
        }

        if (_invData == null) { Debug.LogError("[강화UI] 인벤토리 데이터가 없습니다!"); return; }
        if (weaponDB == null) { Debug.LogError("[강화UI] 무기 DB가 인스펙터에 연결되지 않았습니다!"); return; }
        if (enhanceDB == null) { Debug.LogError("[강화UI] 강화 DB가 인스펙터에 연결되지 않았습니다!"); return; }

        int currentLevel = _invData.weaponEnhancementLevel;
        int nextLevel = currentLevel + 1; // 목표 레벨

        _currentEnhanceData = enhanceDB.GetEnhanceByName(nextLevel.ToString());
        _equippedWeaponData = weaponDB.GetItemById(_invData.equippedWeaponID);

        if (_equippedWeaponData == null)
        {
            Debug.LogError($"[강화UI] 장착 중인 무기 ID({_invData.equippedWeaponID})를 무기 DB에서 찾을 수 없습니다! 무기 DB의 WeaponID 세팅을 확인하세요.");
            return;
        }

        leftWeaponIcon.sprite = _equippedWeaponData.weaponIcon;
        rightWeaponIcon.sprite = _equippedWeaponData.weaponIcon;

        leftLevelText.text = $"+ {currentLevel}";

        if (_currentEnhanceData == null)
        {
            rightLevelText.text = "MAX";
            probText.text = "최대 레벨 도달";
            reqMaterialText.text = "-";
            enhanceButton.interactable = false;
            return;
        }

        rightLevelText.text = $"+ {nextLevel}";

        float displayProb = _invData.soulPityGauge >= 100f ? 100f : _currentEnhanceData.Success_Rate;
        probText.text = $"강화 확률 : {displayProb}% (기운: {_invData.soulPityGauge}%)";

        int myMatAmount = InventoryManager.Instance.GetItemAmount(_currentEnhanceData.Req_Mat.ItemId);
        int myCurrency = _invData.currencyAmount;

        string matColor = myMatAmount >= _currentEnhanceData.Req_Mat_Amt ? "white" : "red";
        string curColor = myCurrency >= _currentEnhanceData.Req_Cost_Amt ? "white" : "red";

        string costName = _currentEnhanceData.Req_Cost != null ? _currentEnhanceData.Req_Cost.itemName : "유물";

        reqMaterialText.text = $"[가공 필요 비용]\n" +
                               $"<color={matColor}>{_currentEnhanceData.Req_Mat.itemName} : {myMatAmount} / {_currentEnhanceData.Req_Mat_Amt}</color>\n" +
                               $"<color={curColor}>{costName} : {myCurrency} / {_currentEnhanceData.Req_Cost_Amt}</color>";

        enhanceButton.interactable = (myMatAmount >= _currentEnhanceData.Req_Mat_Amt && myCurrency >= _currentEnhanceData.Req_Cost_Amt);
    }

    private void OnEnhanceButtonClicked()
    {
        if (_isEnhancing || _currentEnhanceData == null) return;

        // 재화 소모 (Req_Mat, Req_Cost_Amt 기준)
        InventoryManager.Instance.AddItem(_currentEnhanceData.Req_Mat.ItemId, -_currentEnhanceData.Req_Mat_Amt);
        InventoryManager.Instance.AddCurrency(-_currentEnhanceData.Req_Cost_Amt);

        mainPanel.SetActive(false);
        progressPanel.SetActive(true);
        progressWeaponIcon.sprite = _equippedWeaponData.weaponIcon;

        StartCoroutine(EnhanceRoutine());
    }

    private IEnumerator EnhanceRoutine()
    {
        _isEnhancing = true;
        float duration = 3.0f;
        float timePassed = 0f;

        while (timePassed < duration)
        {
            timePassed += Time.deltaTime;
            float progress = timePassed / duration;

            progressBar.value = progress;
            progressText.text = $"{Mathf.RoundToInt(progress * 100f)}%";

            yield return null;
        }

        progressBar.value = 1f;
        progressText.text = "100%";
        yield return new WaitForSeconds(0.2f);

        progressPanel.SetActive(false);
        CalculateAndShowResult();
        _isEnhancing = false;
    }

    private void CalculateAndShowResult()
    {
        resultPanel.SetActive(true);
        resultWeaponIcon.sprite = _equippedWeaponData.weaponIcon;

        bool isSuccess = false;

        // 천장 체크
        if (_invData.soulPityGauge >= 100f)
        {
            isSuccess = true;
            _invData.soulPityGauge = 0f;
        }
        else
        {
            float randomRoll = Random.Range(0f, 100f);
            if (randomRoll <= _currentEnhanceData.Success_Rate)
            {
                isSuccess = true;
                _invData.soulPityGauge = 0f;
            }
            else
            {
                isSuccess = false;
                _invData.soulPityGauge += _currentEnhanceData.Fall_Bonus; // 실패 시 보너스 추가
                _invData.soulPityGauge = Mathf.Clamp(_invData.soulPityGauge, 0f, 100f);
            }
        }

        if (isSuccess)
        {
            _invData.weaponEnhancementLevel++;

            resultTitleText.text = "<color=#FFD700>강화 성공</color>";
            resultLevelText.text = $"+ {_invData.weaponEnhancementLevel}";
            resultPityText.text = "";
            resultButtonText.text = "돌아가기";
        }
        else
        {
            resultTitleText.text = "<color=#FF4500>강화 실패</color>";
            resultLevelText.text = $"+ {_invData.weaponEnhancementLevel}";
            resultPityText.text = $"진혼의 기운 증가! (현재: {_invData.soulPityGauge}%)";
            resultButtonText.text = "다시하기";
        }

        InventoryManager.Instance.SaveGame();
    }

    private void OnResultButtonClicked()
    {
        resultPanel.SetActive(false);
        mainPanel.SetActive(true);

        UpdateMainUI();

        if (resultButtonText.text == "다시하기")
        {
            if (enhanceButton.interactable)
            {
                OnEnhanceButtonClicked();
            }
        }
    }
}
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TowerLobbyUI : MonoBehaviour
{
    [Header("Floor Items")]
    public TowerFloorItem[] floorItems;

    [Header("Detail UI")]
    public TMP_Text titleText;
    public TMP_Text descText;
    public Button confirmButton;

    [Header("Tower Scene")]
    public string towerSceneName = "TowerScene";

    int selectedFloor = 1;

    private void OnEnable()
    {
        RefreshAll();

        int firstUnlockedFloor = GetFirstSelectableFloor();
        SelectFloor(firstUnlockedFloor);
    }

    public void RefreshAll()
    {
        foreach (var item in floorItems)
        {
            item.Init(this);
            item.Refresh();
        }
    }

    int GetFirstSelectableFloor()
    {
        for (int i = 0; i < floorItems.Length; i++)
        {
            if (TowerManager.Instance.IsFloorUnlocked(floorItems[i].floor))
                return floorItems[i].floor;
        }

        return 1;
    }

    public void SelectFloor(int floor)
    {
        selectedFloor = floor;

        foreach (var item in floorItems)
            item.SetSelected(item.floor == selectedFloor);

        titleText.text = $"타워 {floor}층";
        descText.text = GetFloorDescription(floor);

        bool unlocked = TowerManager.Instance.IsFloorUnlocked(floor);
        confirmButton.interactable = unlocked;
    }

    string GetFloorDescription(int floor)
    {
        switch (floor)
        {
            case 1:
                return "자원을 얻기 위한 타워의 첫 번째 층입니다.";
            case 2:
                return "자원을 얻기 위한 타워의 두 번째 층입니다.";
            case 3:
                return "자원을 얻기 위한 타워의 세 번째 층입니다.";
            case 4:
                return "자원을 얻기 위한 타워의 네 번째 층입니다.";
            case 5:
                return "현재 개방된 마지막 타워 층입니다.";
            default:
                return "타워에 도전합니다.";
        }
    }

    public void EnterSelectedFloor()
    {
        if (!TowerManager.Instance.IsFloorUnlocked(selectedFloor))
            return;

        TowerManager.Instance.SelectFloor(selectedFloor);

        if (DataManager.Instance != null)
            DataManager.Instance.SaveAllData();

        if (ScenesManager.Instance != null)
            ScenesManager.Instance.LoadScene(towerSceneName);
    }
}

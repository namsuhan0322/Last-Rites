using System.Collections.Generic;
using UnityEngine;

public class TowerManager : SingletonMono<TowerManager>
{

    [Header("Tower Floor Data")]
    public List<TowerFloorSO> towerFloors = new List<TowerFloorSO>();

    public TowerFloorSO GetSelectedFloorData()
    {
        return towerFloors.Find(x => x.floor == selectedFloor);
    }

    protected override bool DontDestroy => true;

    public int selectedFloor = 1;

    public int HighestClearedFloor
    {
        get
        {
            EnsureTowerData();
            return DataManager.Instance.CurrentGameData.towerData.highestClearedFloor;
        }
    }

    public bool IsFloorUnlocked(int floor)
    {
        if (floor == 1)
            return true;

        return floor <= HighestClearedFloor + 1;
    }

    public void SelectFloor(int floor)
    {
        selectedFloor = floor;
    }

    public void ClearSelectedFloor()
    {
        EnsureTowerData();

        if (selectedFloor > HighestClearedFloor)
        {
            DataManager.Instance.CurrentGameData.towerData.highestClearedFloor = selectedFloor;
            DataManager.Instance.SaveAllData();

            Debug.Log($"{selectedFloor}층 클리어 저장 완료");
        }
    }

    private void EnsureTowerData()
    {
        if (DataManager.Instance == null)
        {
            Debug.LogWarning("DataManager가 없습니다.");
            return;
        }

        if (DataManager.Instance.CurrentGameData == null)
        {
            DataManager.Instance.LoadAllData();
        }

        if (DataManager.Instance.CurrentGameData.towerData == null)
        {
            DataManager.Instance.CurrentGameData.towerData = new PlaythroughSaveData.TowerProgressData();
        }
    }

    //초기화 (테스트용)
    public void ResetTowerProgress()
    {
        EnsureTowerData();

        DataManager.Instance.CurrentGameData.towerData.highestClearedFloor = 0;
        DataManager.Instance.SaveAllData();

        Debug.Log("타워 진행도 초기화 완료");
    }
    //묶어서
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            ResetTowerProgress();
        }
    }
}
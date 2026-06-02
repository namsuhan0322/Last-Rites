[System.Serializable]
public class PlaythroughSaveData
{
    public string version = "1.0.0";

    public GameProgressData progressData;
    public InventoryData inventoryData;
    public TowerProgressData towerData;

    public PlaythroughSaveData()
    {
        progressData = new GameProgressData();
        inventoryData = new InventoryData();
        towerData = new TowerProgressData();
    }

    [System.Serializable]
    public class TowerProgressData
    {
        public int highestClearedFloor = 0;
    }
}
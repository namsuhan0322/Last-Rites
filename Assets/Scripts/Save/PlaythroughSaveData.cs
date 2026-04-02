[System.Serializable]
public class PlaythroughSaveData
{
    public string version = "1.0.0";

    public GameProgressData progressData;
    public InventoryData inventoryData;

    public PlaythroughSaveData()
    {
        progressData = new GameProgressData();
        inventoryData = new InventoryData();
    }
}
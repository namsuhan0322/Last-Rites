using UnityEngine;

public class InventoryHolder : MonoBehaviour
{
    // 외부에서 데이터를 직접 덮어쓰지 못하도록 프로퍼티로 보호
    [field: SerializeField] public InventoryData CurrentData { get; private set; }

    public void InitializeData(InventoryData loadedData)
    {
        CurrentData = loadedData ?? new InventoryData();
    }
}
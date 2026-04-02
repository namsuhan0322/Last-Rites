using UnityEngine;

public class InventoryManager : SingletonMono<InventoryManager>
{
    protected override bool DontDestroy => true;

    private InventoryHolder holder;

    protected override void Awake()
    {
        base.Awake();
        holder = GetComponent<InventoryHolder>();
    }

    public void InitializeData(InventoryData loadedData)
    {
        holder.InitializeData(loadedData);
        Debug.Log("[InventoryManager] 인벤토리 데이터 초기화 완료");
    }

    public void AddItem(string id, int amount)
    {
        InventorySystem.ProcessAddItem(holder.CurrentData, id, amount);
        Debug.Log($"[Manager] {id} 아이템 {amount}개 획득 처리 완료.");

        SaveGame();
    }

    // 인벤토리 변경사항 저장 트리거
    public void SaveGame()
    {
        DataManager.Instance.SaveAllData();
    }

    // 필요 시 외부에서 데이터를 읽기만 할 때 제공하는 Getter
    public InventoryData GetCurrentData()
    {
        return holder.CurrentData;
    }
}
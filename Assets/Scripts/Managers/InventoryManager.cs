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

    public void AddCurrency(int amount)
    {
        if (holder != null && holder.CurrentData != null)
        {
            holder.CurrentData.currencyAmount += amount;
            Debug.Log($"[Manager] 재화 {amount} 획득 완료. (현재 총합: {holder.CurrentData.currencyAmount})");

            SaveGame();
        }
        else
        {
            Debug.LogWarning("[Manager] 인벤토리 데이터가 없어 재화를 추가할 수 없습니다.");
        }
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
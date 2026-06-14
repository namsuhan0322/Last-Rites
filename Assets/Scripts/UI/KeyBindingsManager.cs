using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class KeyBindingsManager : MonoBehaviour
{
    [Header("Description UI")]
    public TextMeshProUGUI descriptionTitle;
    public TextMeshProUGUI descriptionContent;

    [Header("Key Items")]
    public List<KeyOptionItem> keyItems = new List<KeyOptionItem>();

    void Start()
    {
        Debug.Log($"KeyBindingsManager Start {Time.realtimeSinceStartup}");

        // 모든 아이템 초기화
        foreach (var item in keyItems)
        {
            item.Initialize(OnItemSelected);
        }

        // 처음에 첫 번째 아이템(ESC) 선택
        if (keyItems.Count > 0)
        {
            OnItemSelected(keyItems[0]);
        }
    }

    // 아이템 선택 시 호출되는 함수
    void OnItemSelected(KeyOptionItem selectedItem)
    {
        // 모든 아이템 배경 갱신
        foreach (var item in keyItems)
        {
            item.SetSelectedState(item == selectedItem);
        }

        // 우측 설명 텍스트 변경
        if (descriptionTitle != null)
            descriptionTitle.text = selectedItem.keyName;

        if (descriptionContent != null)
            descriptionContent.text = selectedItem.keyDescription;
    }
}
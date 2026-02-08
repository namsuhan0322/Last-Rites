using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LobbyTabManager : MonoBehaviour
{
    [System.Serializable]
    public class TabInfo
    {
        public string tabName;
        public TextMeshProUGUI tabText;
        public GameObject targetPanel;

        [HideInInspector] public GameObject selectedLine;
    }

    [Header("Tab Settings")]
    public List<TabInfo> tabs = new List<TabInfo>();
    public int defaultTabIndex = 0;

    [Header("Text Colors")]
    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    void Start()
    {
        InitializeTabs();
        SelectTab(defaultTabIndex);
    }

    void InitializeTabs()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            int index = i;
            var tab = tabs[i];

            Transform lineTr = tab.tabText.transform.Find("Selected_line");
            if (lineTr != null)
            {
                tab.selectedLine = lineTr.gameObject;
            }
            else
            {
                Debug.LogWarning($"'{tab.tabText.name}' 아래에 'Selected_line' 오브젝트가 없습니다!");
            }

            Button btn = tab.tabText.GetComponent<Button>();
            if (btn == null)
            {
                btn = tab.tabText.gameObject.AddComponent<Button>();
                btn.transition = Selectable.Transition.None;
            }

            // 버튼 클릭 시 SelectTab 함수 호출하도록 연결
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => SelectTab(index));
        }
    }

    // 탭 선택 함수
    public void SelectTab(int index)
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            bool isSelected = (i == index);
            var tab = tabs[i];

            if (tab.targetPanel != null)
                tab.targetPanel.SetActive(isSelected);

            if (tab.selectedLine != null)
                tab.selectedLine.SetActive(isSelected);

            if (tab.tabText != null)
                tab.tabText.color = isSelected ? activeColor : inactiveColor;
        }
    }
}
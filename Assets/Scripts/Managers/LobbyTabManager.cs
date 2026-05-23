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
        public bool isImplemented = true;
        [HideInInspector] public GameObject selectedLine;
    }

    [Header("Tab Settings")]
    public List<TabInfo> tabs = new List<TabInfo>();
    public int defaultTabIndex = 0;

    [Header("Text Colors")]
    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    public Color disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

    void Awake()
    {
        InitializeTabs();
    }

    void OnEnable()
    {
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

            btn.onClick.RemoveAllListeners();

            if (tab.isImplemented)
            {
                btn.onClick.AddListener(() => SelectTab(index));
            }
            else
            {
                btn.onClick.AddListener(() => Debug.Log($"[{tab.tabName}] 탭은 아직 준비 중입니다."));
            }
        }
    }

    public void SelectTab(int index)
    {
        if (index >= 0 && index < tabs.Count && !tabs[index].isImplemented) return;

        for (int i = 0; i < tabs.Count; i++)
        {
            var tab = tabs[i];

            if (!tab.isImplemented)
            {
                if (tab.targetPanel != null) tab.targetPanel.SetActive(false);
                if (tab.selectedLine != null) tab.selectedLine.SetActive(false);
                if (tab.tabText != null) tab.tabText.color = disabledColor;
                continue;
            }

            bool isSelected = (i == index);

            if (tab.targetPanel != null)
                tab.targetPanel.SetActive(isSelected);

            if (tab.selectedLine != null)
                tab.selectedLine.SetActive(isSelected);

            if (tab.tabText != null)
                tab.tabText.color = isSelected ? activeColor : inactiveColor;
        }
    }
}
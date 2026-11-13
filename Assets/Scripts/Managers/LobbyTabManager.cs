using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// [역할] 탭 관리자: 탭 글자에 버튼을 연결하고, 탭을 누르면 같은 팝업 안의 패널·밑줄·글자색을 바꾼다.
///        팝업(탭 패널의 부모)이 새로 열리면 그 팝업의 첫 번째 사용 가능 탭을 고른다. 준비 중 탭은 회색이고 눌러도 옮겨 가지 않는다.
/// [이유] 로비에서는 이 관리자 하나가 팝업 4개(상점·정비대·맵 이동·설정)의 탭 11개를 함께 맡는다.
///        예전에는 씬 시작 때 0번(상점 가공, 준비 중) 하나만 고르려다 실패해 정비대·맵 이동이 빈 화면으로 열렸고,
///        탭을 누르면 다른 팝업의 패널까지 껐다. 탭을 팝업별로 묶어 팝업마다 따로 동작하게 한다.
/// [근거] 로비 UI 팝업 명세 2.1~2.3·3.2, 결정 P4~P7
/// </summary>
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

    [Header("Text Colors")]
    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    public Color disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

    private int[] _groups;                                                      // 탭별 묶음(팝업) 번호
    private bool[] _usable;                                                     // 탭별 사용 가능(isImplemented)
    private readonly List<GameObject> _groupRoots = new List<GameObject>();     // 묶음별 팝업 오브젝트(패널의 부모), 없으면 null
    private bool[] _wasOpen;                                                    // 묶음별 지난 프레임에 열려 있었는지

    void Awake()
    {
        InitializeTabs();
        BuildGroups();

        Debug.Log($"LobbyTabManager Awake {Time.realtimeSinceStartup}");
    }

    void OnEnable()
    {
        // 씬 시작(또는 다시 켜짐): 팝업마다 첫 번째 사용 가능 탭 — 준비 중 탭도 이때 회색이 된다 (결정 P4·P7)
        for (int g = 0; g < _groupRoots.Count; g++)
        {
            SelectFirst(g);
            _wasOpen[g] = IsOpen(g);
        }
    }

    // 화면을 그리기 직전: 이번 프레임에 새로 열린 팝업은 첫 번째 사용 가능 탭으로 — F키·설정 버튼 어느 쪽으로 열어도 같다 (결정 P4·P5)
    void LateUpdate()
    {
        for (int g = 0; g < _groupRoots.Count; g++)
        {
            bool isOpen = IsOpen(g);
            if (LobbyTabRule.JustOpened(_wasOpen[g], isOpen)) SelectFirst(g);
            _wasOpen[g] = isOpen;
        }
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

    // 탭마다 "패널의 부모 = 속한 팝업"으로 묶는다 — 한 팝업의 패널은 같은 부모 밑에 둔다 (결정 P6)
    void BuildGroups()
    {
        var keys = new List<object>(tabs.Count);
        for (int i = 0; i < tabs.Count; i++)
        {
            Transform parent = tabs[i].targetPanel != null ? tabs[i].targetPanel.transform.parent : null;
            keys.Add(parent);
        }
        _groups = LobbyTabRule.AssignGroups(keys);
        _usable = new bool[tabs.Count];

        // 묶음 번호는 처음 나온 순서대로 0,1,2…이므로, 처음 보는 번호일 때 그 팝업 오브젝트를 넣는다
        _groupRoots.Clear();
        for (int i = 0; i < tabs.Count; i++)
        {
            if (_groups[i] < _groupRoots.Count) continue;
            Transform parent = (Transform)keys[i];
            _groupRoots.Add(parent != null ? parent.gameObject : null);
        }
        _wasOpen = new bool[_groupRoots.Count];
    }

    bool IsOpen(int group)
    {
        GameObject root = _groupRoots[group];
        return root != null && root.activeInHierarchy;
    }

    public void SelectTab(int index)
    {
        if (index < 0 || index >= tabs.Count || !tabs[index].isImplemented) return;

        Apply(_groups[index], index);
    }

    void SelectFirst(int group)
    {
        RefreshUsable();
        Apply(group, LobbyTabRule.FirstUsable(_groups, _usable, group));
    }

    void RefreshUsable()
    {
        for (int i = 0; i < tabs.Count; i++) _usable[i] = tabs[i].isImplemented;
    }

    // 같은 묶음(팝업)의 탭만 바꾼다 — 다른 팝업의 패널은 건드리지 않는다 (문제 2, 결정 P4)
    void Apply(int group, int selected)
    {
        RefreshUsable();

        for (int i = 0; i < tabs.Count; i++)
        {
            LobbyTabRule.TabLook look = LobbyTabRule.Decide(_groups, _usable, group, selected, i);
            if (look == LobbyTabRule.TabLook.Untouched) continue;

            var tab = tabs[i];

            if (look == LobbyTabRule.TabLook.Disabled)
            {
                if (tab.targetPanel != null) tab.targetPanel.SetActive(false);
                if (tab.selectedLine != null) tab.selectedLine.SetActive(false);
                if (tab.tabText != null) tab.tabText.color = disabledColor;
                continue;
            }

            bool isSelected = look == LobbyTabRule.TabLook.Selected;

            if (tab.targetPanel != null)
                tab.targetPanel.SetActive(isSelected);

            if (tab.selectedLine != null)
                tab.selectedLine.SetActive(isSelected);

            if (tab.tabText != null)
                tab.tabText.color = isSelected ? activeColor : inactiveColor;
        }
    }
}
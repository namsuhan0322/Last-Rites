using System.Collections.Generic;

/// <summary>
/// [역할] 로비 탭 규칙(엔진 없음): 탭을 속한 팝업별로 묶고, 묶음의 첫 번째 사용 가능 탭을 찾고,
///        탭 하나를 고를 때 각 탭의 모습(손대지 않음·선택·선택 안 됨·준비 중)과 팝업이 막 열렸는지를 정한다.
/// [이유] 탭 관리자 하나가 여러 팝업의 탭을 맡는데, 예전에는 탭 하나를 고르면 모든 팝업의 패널을 바꿨고
///        씬 시작 때 0번(준비 중) 하나만 고르려다 실패했다. 판단을 한 곳에 모아 엔진 밖에서 테스트한다.
/// [근거] 로비 UI 팝업 명세 2.2·2.3·3.1, 결정 P4·P5·P8
/// </summary>
public static class LobbyTabRule
{
    public enum TabLook
    {
        Untouched,   // 다른 팝업의 탭 — 건드리지 않음
        Selected,    // 고른 탭 — 패널·밑줄 켬, 밝은 글자
        Unselected,  // 같은 팝업의 다른 사용 가능 탭 — 패널·밑줄 끔
        Disabled,    // 같은 팝업의 준비 중 탭 — 패널·밑줄 끔, 회색 글자
    }

    /// 같은 키 = 같은 묶음 번호(처음 나온 순서대로 0,1,2…). null 키는 혼자 한 묶음
    public static int[] AssignGroups(IReadOnlyList<object> keys)
    {
        var result = new int[keys.Count];
        var seen = new Dictionary<object, int>();
        int next = 0;

        for (int i = 0; i < keys.Count; i++)
        {
            object key = keys[i];
            if (key == null)
            {
                result[i] = next++;
                continue;
            }

            if (!seen.TryGetValue(key, out int group))
            {
                group = next++;
                seen.Add(key, group);
            }
            result[i] = group;
        }

        return result;
    }

    /// 그 묶음의 첫 번째 사용 가능 탭 번호, 없으면 -1
    public static int FirstUsable(IReadOnlyList<int> groups, IReadOnlyList<bool> usable, int group)
    {
        for (int i = 0; i < groups.Count; i++)
        {
            if (groups[i] == group && usable[i]) return i;
        }
        return -1;
    }

    /// group 묶음에서 selected를 골랐을 때 탭 index의 모습 (selected가 -1이면 사용 가능 탭은 모두 Unselected)
    public static TabLook Decide(IReadOnlyList<int> groups, IReadOnlyList<bool> usable, int group, int selected, int index)
    {
        if (groups[index] != group) return TabLook.Untouched;
        if (!usable[index]) return TabLook.Disabled;
        return index == selected ? TabLook.Selected : TabLook.Unselected;
    }

    /// 지난 프레임에 닫혀 있다가 이번 프레임에 열렸는지
    public static bool JustOpened(bool wasOpen, bool isOpen) => !wasOpen && isOpen;
}

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// [역할] 메뉴 `SuHan/Lobby/팝업 데이터 맞추기`: 로비 씬의 팝업 데이터 두 가지를 맞추고 씬을 저장한다.
///        ① 탭 관리자의 가공 탭(Processing Tap)을 사용 가능으로 ② 맵 이동 창의 "닫지 않을 팝업 목록"에 탑·보스 러시 확인 팝업 추가.
/// [이유] 인스펙터에서 손으로 바꾸면 항목을 헷갈리기 쉽다. 메뉴는 바꿀 것을 먼저 보여 주고, 여러 번 눌러도 결과가 같고,
///        저장된 씬 파일로 확인할 수 있다. 확인 팝업은 이름이 아니라 두 매니저가 실제로 쓰는 popupPanel을 읽어 넣는다.
/// [근거] 로비 UI 팝업 명세 3.5, 결정 P3·P9·P11
/// </summary>
public static class LobbyPopupDataTool
{
    private const string Title = "팝업 데이터 맞추기";
    private const string LogTag = "[LobbyPopupDataTool]";
    private const string LobbyScenePath = "Assets/Scenes/LobbyScene.unity";
    private const string ProcessingTabName = "Processing Tap";  // 상점 가공 탭 (결정 P3)
    private const string MapPanelName = "Map2Map_Panel";        // 맵 이동 창 = ShowShopUI.shopUI

    [MenuItem("SuHan/Lobby/팝업 데이터 맞추기")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Dialog("Play 중에는 쓸 수 없습니다. Play를 멈춘 뒤 다시 실행하세요.");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != LobbyScenePath)
        {
            Dialog($"{LobbyScenePath}를 연 뒤 다시 실행하세요.\n지금 씬: {scene.path}");
            return;
        }

        LobbyTabManager tabManager = FindOne<LobbyTabManager>(c => true, "LobbyTabManager");
        ShowShopUI mapOpener = FindOne<ShowShopUI>(c => c.shopUI != null && c.shopUI.name == MapPanelName, $"shopUI가 {MapPanelName}인 ShowShopUI");
        TowerLobbyManager tower = FindOne<TowerLobbyManager>(c => true, "TowerLobbyManager");
        BossRushManager bossRush = FindOne<BossRushManager>(c => true, "BossRushManager");
        if (tabManager == null || mapOpener == null || tower == null || bossRush == null) return;

        var changes = new List<string>();

        // ① 가공 탭을 사용 가능으로 (결정 P3)
        var tabSo = new SerializedObject(tabManager);
        SerializedProperty processing = FindTabImplemented(tabSo.FindProperty("tabs"), ProcessingTabName);
        if (processing == null)
        {
            Dialog($"탭 관리자에 '{ProcessingTabName}' 탭이 없습니다.");
            return;
        }
        bool turnOnProcessing = !processing.boolValue;
        if (turnOnProcessing) changes.Add($"탭 관리자: '{ProcessingTabName}'(가공)을 사용 가능으로");

        // ② 맵 이동 창의 닫지 않을 팝업 목록에 탑·보스 러시 확인 팝업 (결정 P9)
        var mapSo = new SerializedObject(mapOpener);
        SerializedProperty blockList = mapSo.FindProperty("popupsToBlockClose");
        var toAdd = new List<GameObject>();
        foreach (Object owner in new Object[] { tower, bossRush })
        {
            var popup = new SerializedObject(owner).FindProperty("popupPanel").objectReferenceValue as GameObject;
            if (popup == null)
            {
                Dialog($"{owner.GetType().Name}의 popupPanel이 비어 있습니다.");
                return;
            }
            if (Contains(blockList, popup) || toAdd.Contains(popup)) continue;

            toAdd.Add(popup);
            changes.Add($"맵 이동 창 닫지 않을 목록: '{popup.name}' 추가 ({owner.GetType().Name}의 확인 팝업)");
        }

        if (changes.Count == 0)
        {
            Debug.Log($"{LogTag} 변경 없음 — 이미 맞춰져 있음");
            Dialog("변경 없음 — 이미 맞춰져 있습니다.");
            return;
        }

        string list = "- " + string.Join("\n- ", changes);
        if (!EditorUtility.DisplayDialog(Title, $"아래를 바꾸고 로비 씬을 저장합니다.\n\n{list}", "적용", "취소"))
        {
            Debug.Log($"{LogTag} 취소");
            return;
        }

        if (turnOnProcessing)
        {
            processing.boolValue = true;
            tabSo.ApplyModifiedProperties();
        }

        foreach (GameObject popup in toAdd)
        {
            int index = blockList.arraySize;
            blockList.arraySize++;
            blockList.GetArrayElementAtIndex(index).objectReferenceValue = popup;
        }
        mapSo.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene);

        foreach (string change in changes) Debug.Log($"{LogTag} {change}");
        Debug.Log($"{LogTag} 적용 {changes.Count}건, 씬 저장 {(saved ? "성공" : "실패")}");
    }

    // 씬에서(꺼진 것 포함) 조건에 맞는 컴포넌트를 정확히 1개 찾는다. 0개·여러 개면 안내 창 후 null
    private static T FindOne<T>(System.Func<T, bool> match, string what) where T : Object
    {
        var found = new List<T>();
        foreach (T c in Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (match(c)) found.Add(c);
        }

        if (found.Count == 1) return found[0];

        Dialog($"{what}를 정확히 1개 찾아야 하는데 {found.Count}개입니다.");
        return null;
    }

    private static SerializedProperty FindTabImplemented(SerializedProperty tabs, string tabName)
    {
        for (int i = 0; i < tabs.arraySize; i++)
        {
            SerializedProperty tab = tabs.GetArrayElementAtIndex(i);
            if (tab.FindPropertyRelative("tabName").stringValue == tabName) return tab.FindPropertyRelative("isImplemented");
        }
        return null;
    }

    private static bool Contains(SerializedProperty list, GameObject target)
    {
        for (int i = 0; i < list.arraySize; i++)
        {
            if (list.GetArrayElementAtIndex(i).objectReferenceValue == target) return true;
        }
        return false;
    }

    private static void Dialog(string message) => EditorUtility.DisplayDialog(Title, message, "확인");
}

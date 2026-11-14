using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

/// <summary>
/// [역할] 메뉴 `SuHan/Lobby/투명 벽 장애물 맞추기`: 로비 맵의 투명 벽(켜진 렌더러가 없고 서 있는 BoxCollider)마다 NavMeshObstacle(파내기)을
///        붙이거나 값을 맞추고 씬을 저장한다. 장애물의 중심·크기는 그 콜라이더와 같다. 기울어진 상자(밟고 오르는 경사로)는 빼고,
///        이 메뉴가 붙였던 장애물이 있으면 지운다.
/// [이유] NavMesh는 화면에 그려지는 메쉬로 굽기 때문에 렌더러가 없거나 꺼진 투명 벽이 빠져, 길은 벽을 통과하고 몸만 막혀
///        계속 걷는 동작이 났다. NavMesh를 다시 굽지 않고 실행 중에 벽 자리를 파낸다. 바꿀 것을 먼저 보여 주고,
///        여러 번 눌러도 결과가 같다. LobbyMap 프리팹 파일은 건드리지 않고 씬의 인스턴스에 더한다.
/// [근거] 로비 이동 막힘 명세 2.3·3.3, 결정 N2·N6·N7·N10·N11
/// </summary>
public static class LobbyWallObstacleTool
{
    private const string Title = "투명 벽 장애물 맞추기";
    private const string LogTag = "[LobbyWallObstacleTool]";
    private const string LobbyScenePath = "Assets/Scenes/LobbyScene.unity";
    private const string LobbyMapName = "LobbyMap";
    private const int MaxListLines = 20;   // 확인 창에 보여 줄 줄 수 (넘으면 "외 N개")
    private const float MaxWallTiltDegrees = 10f;   // 세 축 중 하나가 세계 위쪽과 이 각도 안이면 서 있는 상자(벽). 계단 경사로 Cube는 약 45° (결정 N11)

    [MenuItem("SuHan/Lobby/투명 벽 장애물 맞추기")]
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

        var maps = new List<GameObject>();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == LobbyMapName) maps.Add(root);
        }
        if (maps.Count != 1)
        {
            Dialog($"씬 최상위에서 '{LobbyMapName}'를 정확히 1개 찾아야 하는데 {maps.Count}개입니다.");
            return;
        }
        GameObject map = maps[0];

        // 투명 벽 = 켜진 BoxCollider · 트리거 아님 · 켜진 렌더러 없음 (켜진 렌더러가 있으면 이미 NavMesh 굽기에 들어감, 결정 N6)
        // 씬은 투명 벽마다 꺼진 MeshRenderer(Cube)를 더해 두었다 — 꺼진 렌더러는 굽기에서 빠지므로 없는 것과 같다 (결정 N10, 테스트 후 추가)
        // 기울어진 상자는 밟고 오르는 경사로(커밋 9d26c635 계단 Cube) — 파내면 계단 NavMesh가 사라진다 (결정 N11, 테스트 후 추가)
        var walls = new List<BoxCollider>();
        var ramps = new List<BoxCollider>();
        foreach (BoxCollider box in map.GetComponentsInChildren<BoxCollider>(true))
        {
            if (!box.enabled || !box.gameObject.activeInHierarchy || box.isTrigger) continue;
            Renderer renderer = box.GetComponent<Renderer>();
            if (renderer != null && renderer.enabled) continue;
            if (IsUpright(box.transform)) walls.Add(box);
            else ramps.Add(box);
        }

        var toAdd = new List<BoxCollider>();
        var toFix = new List<BoxCollider>();
        var toRemove = new List<NavMeshObstacle>();
        var lines = new List<string>();
        foreach (BoxCollider box in walls)
        {
            NavMeshObstacle obstacle = box.GetComponent<NavMeshObstacle>();
            if (obstacle == null)
            {
                toAdd.Add(box);
                lines.Add($"+ {PathOf(box.transform, map.transform)} — 장애물 새로");
            }
            else if (!Matches(obstacle, box))
            {
                toFix.Add(box);
                lines.Add($"~ {PathOf(box.transform, map.transform)} — 값 맞춤");
            }
        }
        foreach (BoxCollider box in ramps)
        {
            // 이 메뉴가 붙인 값과 같은 장애물만 지운다 (다른 이유로 붙인 장애물은 건드리지 않음)
            NavMeshObstacle obstacle = box.GetComponent<NavMeshObstacle>();
            if (obstacle == null || !Matches(obstacle, box)) continue;
            toRemove.Add(obstacle);
            lines.Add($"− {PathOf(box.transform, map.transform)} — 장애물 지움(기울어진 경사로)");
        }

        if (lines.Count == 0)
        {
            Debug.Log($"{LogTag} 변경 없음 — 투명 벽 {walls.Count}개 모두 맞춰져 있음");
            Dialog($"변경 없음 — 투명 벽 {walls.Count}개 모두 맞춰져 있습니다.");
            return;
        }

        string list = string.Join("\n", lines.GetRange(0, Mathf.Min(lines.Count, MaxListLines)));
        if (lines.Count > MaxListLines) list += $"\n… 외 {lines.Count - MaxListLines}개";
        if (!EditorUtility.DisplayDialog(Title,
                $"투명 벽(렌더러가 없거나 꺼지고 서 있는 BoxCollider) {walls.Count}개를 찾았습니다.\n아래를 바꾸고 로비 씬을 저장합니다.\n\n{list}", "적용", "취소"))
        {
            Debug.Log($"{LogTag} 취소");
            return;
        }

        foreach (BoxCollider box in toAdd)
        {
            NavMeshObstacle obstacle = Undo.AddComponent<NavMeshObstacle>(box.gameObject);
            Configure(obstacle, box);
        }
        foreach (BoxCollider box in toFix)
        {
            NavMeshObstacle obstacle = box.GetComponent<NavMeshObstacle>();
            Undo.RecordObject(obstacle, Title);
            Configure(obstacle, box);
        }
        foreach (NavMeshObstacle obstacle in toRemove)
        {
            Undo.DestroyObjectImmediate(obstacle);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene);

        foreach (string line in lines) Debug.Log($"{LogTag} {line}");
        Debug.Log($"{LogTag} 적용 {lines.Count}건(새로 {toAdd.Count}, 맞춤 {toFix.Count}, 지움 {toRemove.Count}) / 투명 벽 {walls.Count}개, 씬 저장 {(saved ? "성공" : "실패")}");
    }

    // 상자의 세 축(오른쪽·위·앞)이 세계 위쪽과 이루는 각도로 판단 — 상자는 어느 축을 세워도 같은 모양이라 세 축을 다 본다
    private static bool IsUpright(Transform t)
    {
        float[] angles =
        {
            Vector3.Angle(t.right, Vector3.up),
            Vector3.Angle(t.up, Vector3.up),
            Vector3.Angle(t.forward, Vector3.up),
        };
        return ObstacleTargetRule.IsUpright(angles, MaxWallTiltDegrees);
    }

    // 몸이 막히는 범위 = 길찾기가 막히는 범위: 중심·크기를 콜라이더와 같게(둘 다 오브젝트 크기를 따름), 멈춰 있을 때 한 번 파냄
    private static void Configure(NavMeshObstacle obstacle, BoxCollider box)
    {
        obstacle.shape = NavMeshObstacleShape.Box;
        obstacle.center = box.center;
        obstacle.size = box.size;
        obstacle.carving = true;
        obstacle.carveOnlyStationary = true;
        PrefabUtility.RecordPrefabInstancePropertyModifications(obstacle);
    }

    private static bool Matches(NavMeshObstacle obstacle, BoxCollider box)
    {
        return obstacle.shape == NavMeshObstacleShape.Box
            && obstacle.center == box.center
            && obstacle.size == box.size
            && obstacle.carving
            && obstacle.carveOnlyStationary;
    }

    private static string PathOf(Transform t, Transform root)
    {
        string path = t.name;
        while (t.parent != null && t != root)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }

    private static void Dialog(string message) => EditorUtility.DisplayDialog(Title, message, "확인");
}

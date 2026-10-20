using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// [역할] 메뉴 한 번으로 플레이어 리팩토링 테스트 씬을 처음부터 다시 만든다.
///        바닥+NavMesh, 플레이어 프리팹, 카메라, 매니저, 쿨타임 UI·HUD, 연습용 적 3마리·연습용 보스(약점 1개), 테스트 도구.
/// [이유] 손으로 만든 씬은 망가지면 되살리기 어렵다. 리팩토링 전후를 같은 씬에서 비교한다.
/// [근거] 작업 방식 5장, 명세 3.6-3, 결정 R19, 프로젝트 설정 결정 5, 공격 판정 H9
/// </summary>
public static class PlayerRefactorTestSceneBuilder
{
    private const string SceneDir = "Assets/Scenes/DevTest";
    private const string ScenePath = SceneDir + "/PlayerRefactorTest.unity";
    private const string NavMeshPath = SceneDir + "/PlayerRefactorTest_NavMesh.asset";

    private const string PlayerPrefab = "Assets/Prefabs/Player/Player.prefab";
    private const string CameraPrefab = "Assets/Prefabs/Camera/Main Camera.prefab";
    private const string UICooldownPrefab = "Assets/Prefabs/UI/UICooldown.prefab";
    private const string PlayerHUDPrefab = "Assets/Prefabs/UI/PlayerHUD.prefab";

    // EffectManager·GameManager: 플레이어 코드가 null 검사 없이 부름 (이동 클릭 이펙트, 사망 처리)
    // SFXManager: 효과음 / DamageEffectManager: 적 머리 위 데미지 숫자
    // ScenesManager: 부활 때 페이드 (없으면 GameManager가 오류 로그) / EventSystemManager: UI 입력
    private static readonly string[] ManagerPrefabs =
    {
        "Assets/Prefabs/Managers/GameManager.prefab",
        "Assets/Prefabs/Managers/EffectManager.prefab",
        "Assets/Prefabs/Managers/SFXManager.prefab",
        "Assets/Prefabs/Managers/DamageEffectManager.prefab",
        "Assets/Prefabs/Managers/ScenesManager.prefab",
        "Assets/Prefabs/Managers/EventSystemManager.prefab",
    };

    [MenuItem("SuHan/Setup Player Refactor Test")]
    public static void Build()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("[DevTest] 플레이 중에는 테스트 씬을 만들 수 없습니다.");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        // AssetDatabase가 아는 폴더여야 NavMesh 에셋을 만들 수 있으므로 AssetDatabase로 만든다
        if (!AssetDatabase.IsValidFolder(SceneDir)) AssetDatabase.CreateFolder("Assets/Scenes", "DevTest");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateLight();
        GameObject ground = CreateGround();
        BakeNavMesh(ground);

        GameObject player = Spawn(PlayerPrefab);
        player.transform.position = Vector3.zero;
        var controller = player.GetComponent<PlayerController>();
        var stats = player.GetComponent<PlayerStats>();

        GameObject cam = Spawn(CameraPrefab);
        cam.AddComponent<DevTestCameraFollow>().target = player.transform;
        // 실제 씬처럼 피격 화면 효과를 카메라의 ScreenBloodController에 연결 (씬 덮어쓰기와 같은 방식)
        var blood = cam.GetComponentInChildren<ScreenBloodController>(true);
        if (blood != null) SetReference(controller, "screenBloodEffect", blood);

        Transform respawn = new GameObject("DevTest_RespawnPoint").transform;
        foreach (string path in ManagerPrefabs)
        {
            GameObject manager = Spawn(path);
            var gameManager = manager.GetComponent<GameManager>();
            if (gameManager != null) SetReference(gameManager, "respawnPoint", respawn);
        }

        CreateUI(controller, stats);
        CreateDummies();
        CreateDummyBoss();

        var tools = new GameObject("DevTest_Tools");
        tools.AddComponent<DevTestPlayerHotkeys>().player = controller;
        tools.AddComponent<DevTestPlayerOverlay>().player = controller;

        EditorSceneManager.SaveScene(scene, ScenePath);
        Debug.Log($"[DevTest] 테스트 씬 생성 완료: {ScenePath}");
    }

    private static GameObject Spawn(string path)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) throw new System.Exception($"[DevTest] 프리팹을 찾지 못함: {path}");
        return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
    }

    /// 프리팹 인스턴스의 직렬화 필드에 참조를 넣는다 (씬의 덮어쓰기 값으로 저장됨).
    private static void SetReference(Object target, string fieldName, Object value)
    {
        if (target == null)
        {
            Debug.LogWarning($"[DevTest] {fieldName}를 넣을 대상이 없음");
            return;
        }
        var so = new SerializedObject(target);
        SerializedProperty prop = so.FindProperty(fieldName);
        if (prop == null)
        {
            Debug.LogWarning($"[DevTest] {target.GetType().Name}.{fieldName} 필드가 없음");
            return;
        }
        prop.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateLight()
    {
        var go = new GameObject("Directional Light");
        var light = go.AddComponent<Light>();
        light.type = LightType.Directional;
        go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    private static GameObject CreateGround()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "DevTest_Ground";
        ground.transform.localScale = new Vector3(5f, 1f, 5f);     // 50m x 50m
        // 이동 클릭 레이캐스트가 PlayerController.GroundLayer(프리팹 값 = Ground, 3번)만 맞추므로 같은 레이어로 둔다
        ground.layer = LayerMask.NameToLayer("Ground");
        return ground;
    }

    private static void BakeNavMesh(GameObject ground)
    {
        var surface = ground.AddComponent<NavMeshSurface>();
        // 바닥만 굽는다 → 나중에 놓는 플레이어·더미가 NavMesh에 구멍을 내지 않음
        surface.collectObjects = CollectObjects.Children;
        surface.BuildNavMesh();
        AssetDatabase.DeleteAsset(NavMeshPath);
        AssetDatabase.CreateAsset(surface.navMeshData, NavMeshPath);
    }

    private static void CreateUI(PlayerController controller, PlayerStats stats)
    {
        // UICooldown·PlayerHUD 프리팹에는 Canvas가 없어서 (실제 씬에서도 Canvas 아래에 둠) 새 Canvas를 만든다
        var canvasGo = new GameObject("DevTest_Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.layer = LayerMask.NameToLayer("UI");
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject cooldown = Spawn(UICooldownPrefab);
        cooldown.transform.SetParent(canvasGo.transform, false);
        SetReference(cooldown.GetComponentInChildren<UICooldown>(true), "Player", controller);

        GameObject hud = Spawn(PlayerHUDPrefab);
        hud.transform.SetParent(canvasGo.transform, false);
        SetReference(hud.GetComponentInChildren<PlayerHUD>(true), "playerStats", stats);
    }

    private static void CreateDummies()
    {
        // 프리팹 DetectionRadius = 5, ViewAngle = 150 → 1·2번은 감지 반경 안, 3번(거리 약 7.4)은 밖
        Vector3[] positions = { new Vector3(0f, 1f, 3f), new Vector3(-2.5f, 1f, 4f), new Vector3(2.5f, 1f, 7f) };
        // PlayerController.EnemyLayer(프리팹 값) = 6번 Enemy
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        for (int i = 0; i < positions.Length; i++)
        {
            var dummy = GameObject.CreatePrimitive(PrimitiveType.Capsule);    // 높이 2, 중심 y=1 → 바닥 위에 섬
            dummy.name = $"Dummy_{i + 1}";
            dummy.layer = enemyLayer;
            dummy.transform.position = positions[i];
            dummy.AddComponent<DevTestDummyTarget>();
        }
    }

    // 연습용 보스: 더미 몸통 + 오른쪽 위에 진짜 WeakPoint 구체 (공격 판정 H9). 한 번 휘둘러 몸통과 약점이 같이 닿도록 붙여 둔다
    private static void CreateDummyBoss()
    {
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "DummyBoss";
        body.layer = enemyLayer;
        body.transform.position = new Vector3(3f, 1f, 2f);   // 감지 반경 5 안 (거리 약 3.6)
        body.AddComponent<DevTestDummyTarget>();
        body.AddComponent<DevTestDummyBoss>();

        var weak = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        weak.name = "DummyBoss_WeakPoint";
        weak.layer = enemyLayer;
        weak.transform.SetParent(body.transform, false);
        weak.transform.localPosition = new Vector3(0.6f, 0.4f, 0f);
        weak.transform.localScale = Vector3.one * 0.6f;
        weak.AddComponent<WeakPoint>();
    }
}

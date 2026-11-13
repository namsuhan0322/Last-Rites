using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct SceneLoadingData
{
    public string targetSceneName;      // 이동할 씬 이름
    public Sprite loadingImage;         // 보여줄 배경 일러스트
    [TextArea(3, 5)]
    public string loadingDescription;   // 보여줄 설명/팁
}

/// <summary>
/// [역할] 씬 이동 담당(싱글턴): 로딩 화면을 거치는 이동(페이드 → 로딩 씬 → 비동기 로딩·게이지 → 100% → 씬 켜기 → 데이터 준비 대기 → 페이드),
///        로딩 화면 없이 바로 이동, 씬별 BGM.
/// [이유] 모든 씬 이동을 한 곳에서 처리해 페이드·로딩 화면·BGM을 맞춘다. 게이지 계산은 LoadingGaugeRule에 맡긴다.
/// [근거] 로딩 게이지 명세 2.1·3.2, 결정 L1~L4 (머리 주석은 로딩 게이지 단계에서 추가)
/// </summary>
public class ScenesManager : SingletonMono<ScenesManager>
{
    [Header("Scene Settings")]
    public string mainMenuSceneName = "MainScene";
    public string tutorialSceneName = "TutorialScene";
    public string loadingSceneName = "LoadingScene";
    public string LobbySceneName = "LobbyScene";
    public string Thema1SceneName = "Thema1Scene";
    public string Thema2SceneName = "Thema2Scene";
    public string Thema3SceneName = "Thema3Scene";
    public string Tower1SceneName = "Tower1";
    public string BossRushSceneName = "BossRushScene";
    public string TowerSceneName = "TowerScene";

    [Header("Loading Settings")]
    public float minimumLoadingTime = 5f;
    public bool useLoadingScreen = true;
    public bool useFadeEffect = true;

    [Header("Fade Settings")]
    public float fadeSpeed = 2f;
    public Color fadeColor = Color.black;

    private string currentSceneName;
    private bool isLoading = false;
    private CanvasGroup fadeCanvasGroup;
    private GameObject fadeObject;

    public float LoadingProgress { get; private set; }

    [HideInInspector] public bool isDataLoaded = false;

    [Header("Loading UI Settings")]
    public List<SceneLoadingData> loadingTips;
    public string TargetSceneName { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        currentSceneName = SceneManager.GetActiveScene().name;
        if (useFadeEffect) CreateFadeUI();

        Debug.Log($"SceneManager Awake {Time.realtimeSinceStartup}");
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        PlaySceneBGM(currentSceneName);

        Debug.Log($"SceneManager Start {Time.realtimeSinceStartup}");
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    public void LoadScene(string sceneName)
    {
        if (isLoading) return;

        if (useLoadingScreen)
            StartCoroutine(LoadSceneWithLoading(sceneName));
        else
            StartCoroutine(LoadSceneDirectly(sceneName));
    }

    private IEnumerator LoadSceneWithLoading(string targetScene)
    {
        isLoading = true;
        isDataLoaded = false;
        LoadingProgress = 0f;
        TargetSceneName = targetScene;

        GameManager.Instance?.ChangeGameState(GameState.Loading);

        if (useFadeEffect) yield return StartCoroutine(FadeIn());

        SceneManager.LoadScene(loadingSceneName);
        yield return null;

        if (useFadeEffect) yield return StartCoroutine(FadeOut());

        System.GC.Collect();
        yield return Resources.UnloadUnusedAssets();

        AsyncOperation op = SceneManager.LoadSceneAsync(targetScene);
        op.allowSceneActivation = false;

        // 1) 실제 로딩: 진행도 0~0.9 → 게이지 0~90% (씬 켜기를 막아 두면 진행도는 0.9에서 멈춤)
        while (op.progress < LoadingGaugeRule.LoadEnd)
        {
            LoadingProgress = LoadingGaugeRule.FromLoad(op.progress, LoadingProgress);
            yield return null;
        }
        LoadingProgress = LoadingGaugeRule.FromLoad(op.progress, LoadingProgress);

        // 2) 연출: 지금 위치에서 100%까지 (예전에는 90%에서 다시 시작해 게이지가 내려갔다, 로딩 게이지 L1)
        while (!LoadingGaugeRule.IsFull(LoadingProgress))
        {
            LoadingProgress = LoadingGaugeRule.Creep(LoadingProgress, Time.unscaledDeltaTime);
            yield return null;
        }

        // 3) 100%를 화면에 보여 준 뒤 씬을 켠다 (실제 시간 — 게임이 멈춰 있어도 진행)
        yield return new WaitForSecondsRealtime(LoadingGaugeRule.FullHoldSeconds);
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;

        if (useFadeEffect && fadeCanvasGroup != null) fadeCanvasGroup.alpha = 1f;

        float timeout = 10f;
        while (!isDataLoaded && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (timeout <= 0) Debug.LogWarning("[ScenesManager] 데이터 로드 대기 시간 초과! 강제로 화면을 엽니다.");

        yield return new WaitForEndOfFrame();

        LoadingProgress = 1.0f;

        yield return new WaitForSecondsRealtime(0.5f);

        if (useFadeEffect) yield return StartCoroutine(FadeOut());

        isLoading = false;
    }

    //타워씬 로드
    public void LoadTowerScene()
    {
        LoadScene(TowerSceneName);
    }

    private IEnumerator LoadSceneDirectly(string targetScene)
    {
        isLoading = true;
        isDataLoaded = false;
        if (useFadeEffect) yield return StartCoroutine(FadeIn());

        AsyncOperation op = SceneManager.LoadSceneAsync(targetScene);
        while (!op.isDone) yield return null;

        float timeout = 5f;
        while (!isDataLoaded && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (useFadeEffect) yield return StartCoroutine(FadeOut());
        isLoading = false;
    }

    public void DataLoadCompleted()
    {
        Debug.Log($"DataLoadCompleted {Time.realtimeSinceStartup}");
        isDataLoaded = true;  
    }

    #region UI & Fade Logic (동일)
    private void CreateFadeUI()
    {
        if (fadeObject != null) return;
        fadeObject = new GameObject("FadeCanvas");
        Canvas canvas = fadeObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30000;
        fadeCanvasGroup = fadeObject.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(fadeObject.transform, false);
        UnityEngine.UI.Image img = imageObj.AddComponent<UnityEngine.UI.Image>();
        img.color = fadeColor;
        RectTransform rect = img.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        DontDestroyOnLoad(fadeObject);
    }

    public IEnumerator FadeIn()
    {
        if (fadeCanvasGroup == null) yield break;
        fadeCanvasGroup.blocksRaycasts = true;
        while (fadeCanvasGroup.alpha < 1f)
        {
            fadeCanvasGroup.alpha += Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }
        fadeCanvasGroup.alpha = 1f;
    }

    public IEnumerator FadeOut()
    {
        if (fadeCanvasGroup == null) yield break;
        while (fadeCanvasGroup.alpha > 0f)
        {
            fadeCanvasGroup.alpha -= Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
    }
    #endregion

    #region Scene Events & Utils (동일)
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene Loaded Event {Time.realtimeSinceStartup}");

        currentSceneName = scene.name;

        try
        {
            if (scene.name != loadingSceneName) GameEvents.SceneChanged(currentSceneName);
            PlaySceneBGM(scene.name);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ScenesManager] 씬 로드 이벤트 중 에러 발생 (로딩은 계속 진행됨): {e.Message}");
        }

        Debug.Log($"씬 로드 : {scene.name}");

        if (scene.name != loadingSceneName)
        {
            DataLoadCompleted();
        }
    }

    private void PlaySceneBGM(string sceneName)
    {
        if (SoundManager.Instance == null) return;

        if (sceneName == mainMenuSceneName) SoundManager.Instance.PlayBGM("MainBGM");
        else if (sceneName == LobbySceneName) SoundManager.Instance.PlayBGM("LobbyBGM");
        else if (sceneName == tutorialSceneName) SoundManager.Instance.PlayBGM("TutorialBGM");
        else if (sceneName == Thema1SceneName) SoundManager.Instance.PlayBGM("Thema1BGM");
        else if (sceneName == Thema2SceneName) SoundManager.Instance.PlayBGM("Thema2BGM");
        else if (sceneName == BossRushSceneName) SoundManager.Instance.PlayBGM("BossRushBGM");
        else if (sceneName == Tower1SceneName) SoundManager.Instance.PlayBGM("TowerBGM");
        else if (sceneName == loadingSceneName) SoundManager.Instance.PlayBGM("");
    }

    private void OnSceneUnloaded(Scene scene)
    {
        Debug.Log($"[Scene Unloaded] {scene.name}");
    }
    public void LoadMainMenu() => LoadScene(mainMenuSceneName);
    public void LoadGameScene()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem("S_000", 1, false);
            InventoryManager.Instance.AddItem("R_001", 99, false);
            InventoryManager.Instance.AddItem("P_001", 99, false);
            InventoryManager.Instance.AddItem("P_002", 99, false);
            InventoryManager.Instance.AddItem("P_003", 99, false);
            InventoryManager.Instance.AddCurrency(999999, false);

            InventoryData invData = InventoryManager.Instance.GetCurrentData();
            if (invData != null) invData.equippedWeaponID = 10;
            if (DataManager.Instance != null) DataManager.Instance.SaveAllData();
        }
        LoadScene(LobbySceneName);
    }
    public void LoadTestGameScene() => LoadScene(Thema1SceneName);
    public void LoadLobbyScene() => LoadScene(LobbySceneName);
    public void LoadTower1Scene() => LoadScene(Tower1SceneName);
    public void LoadBossRushScene() => LoadScene(BossRushSceneName);
    public void ReloadCurrentScene() => LoadScene(currentSceneName);
    #endregion
}
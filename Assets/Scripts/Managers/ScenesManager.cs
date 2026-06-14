using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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

        GameManager.Instance?.ChangeGameState(GameState.Loading);

        if (useFadeEffect) yield return StartCoroutine(FadeIn());

        SceneManager.LoadScene(loadingSceneName);
        yield return null;

        if (useFadeEffect) yield return StartCoroutine(FadeOut());

        System.GC.Collect();
        yield return Resources.UnloadUnusedAssets();

        AsyncOperation op = SceneManager.LoadSceneAsync(targetScene);
        op.allowSceneActivation = false;

        bool hasActivated = false;

        while (!op.isDone)
        {
            if (!hasActivated)
            {
                LoadingProgress = Mathf.Clamp01((op.progress / 0.9f) * 0.99f);
            }

            if (op.progress >= 0.9f && !hasActivated)
            {
                hasActivated = true;

                float fakeProgress = 0.9f;
                float creepSpeed = 0.05f;

                while (fakeProgress < 0.99f)
                {
                    fakeProgress += Time.unscaledDeltaTime * creepSpeed;

                    if (fakeProgress > 0.99f) fakeProgress = 0.99f;

                    LoadingProgress = fakeProgress;

                    yield return null;
                }

                LoadingProgress = 0.99f;
                yield return new WaitForSeconds(0.1f);
                op.allowSceneActivation = true;
            }
            yield return null;
        }

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
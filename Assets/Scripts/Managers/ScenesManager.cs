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

    [Header("Loading Settings")]
    [Tooltip("로딩을 최소 몇 초 동안 유지할지 설정합니다.")]
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

    protected override void Awake()
    {
        base.Awake();
        currentSceneName = SceneManager.GetActiveScene().name;
        if (useFadeEffect) CreateFadeUI();
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        PlaySceneBGM(currentSceneName);
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

        LoadingProgress = 0f;

        GameManager.Instance?.ChangeGameState(GameState.Loading);

        // 페이드 인
        if (useFadeEffect) yield return StartCoroutine(FadeIn());

        // 로딩 씬 로드
        SceneManager.LoadScene(loadingSceneName);
        yield return null;

        // 페이드 아웃
        if (useFadeEffect) yield return StartCoroutine(FadeOut());

        AsyncOperation op = SceneManager.LoadSceneAsync(targetScene);
        op.allowSceneActivation = false;

        float timer = 0.0f;

        // 로딩 루프
        while (!op.isDone)
        {
            timer += Time.deltaTime;

            float opProgress = Mathf.Clamp01(op.progress / 0.9f);
            float timeProgress = Mathf.Clamp01(timer / minimumLoadingTime);

            LoadingProgress = Mathf.Min(opProgress, timeProgress);

            if (opProgress >= 1f && timeProgress >= 1f)
            {
                LoadingProgress = 1f;

                if (useFadeEffect) yield return StartCoroutine(FadeIn());

                op.allowSceneActivation = true;

                yield return new WaitUntil(() => op.isDone);
            }
            else
            {
                yield return null;
            }
        }

        // 새 씬 로드 후 페이드 아웃
        if (useFadeEffect) yield return StartCoroutine(FadeOut());

        isLoading = false;
    }

    private IEnumerator LoadSceneDirectly(string targetScene)
    {
        isLoading = true;
        if (useFadeEffect) yield return StartCoroutine(FadeIn());

        AsyncOperation op = SceneManager.LoadSceneAsync(targetScene);
        while (!op.isDone) yield return null;

        if (useFadeEffect) yield return StartCoroutine(FadeOut());
        isLoading = false;
    }

    #region UI & Fade Logic
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

    #region Scene Events & Utils
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;

        if (scene.name != loadingSceneName) GameEvents.SceneChanged(currentSceneName);

        PlaySceneBGM(scene.name);

        Debug.Log($"씬 로드 : {scene.name}");
    }

    private void PlaySceneBGM(string sceneName)
    {
        if (SoundManager.Instance == null) return;

        if (sceneName == mainMenuSceneName)
        {
            SoundManager.Instance.PlayBGM("MainBGM");
        }
        else if (sceneName == LobbySceneName)
        {
            SoundManager.Instance.PlayBGM("LobbyBGM");
        }
        else if (sceneName == tutorialSceneName)
        {
            SoundManager.Instance.PlayBGM("TutorialBGM");
        }
        else if (sceneName == loadingSceneName)
        {
            SoundManager.Instance.PlayBGM(""); 
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        Debug.Log($"[Scene Unloaded] {scene.name}");
    }

    public void LoadMainMenu() => LoadScene(mainMenuSceneName);
    public void LoadGameScene()
    {
        Debug.Log("<color=yellow>[테스트 빌드] 튜토리얼을 스킵하고 바로 로비로 이동하며, 테스트용 아이템을 지급합니다!</color>");

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem("S_000", 1);
            InventoryManager.Instance.AddItem("R_001", 99);
            InventoryManager.Instance.AddItem("P_001", 99);
            InventoryManager.Instance.AddItem("P_002", 99);
            InventoryManager.Instance.AddItem("P_003", 99);
            InventoryManager.Instance.AddCurrency(999999);

            InventoryData invData = InventoryManager.Instance.GetCurrentData();
            if (invData != null)
            {
                invData.equippedWeaponID = 10;
            }

            if (DataManager.Instance != null)
            {
                DataManager.Instance.SaveAllData();
            }
        }
        else
        {
            Debug.LogWarning("[테스트 빌드] InventoryManager를 찾을 수 없어 아이템 지급에 실패했습니다.");
        }

        LoadScene(LobbySceneName);

        /* 기존 코드는 나중을 위해 주석 처리
        if (GameProgressManager.Instance == null || GameProgressManager.Instance.progressData == null)
        {
            Debug.LogWarning("[ScenesManager] GameProgressData를 찾을 수 없어 기본 튜토리얼 씬으로 이동합니다.");
            LoadScene(tutorialSceneName);
            return;
        }

        if (GameProgressManager.Instance.progressData.isTutorialCleared)
        {
            LoadScene(LobbySceneName);
        }
        else
        {
            LoadScene(tutorialSceneName);
        }
        */
    }

    public void LoadTestGameScene()
    {
        LoadScene(Thema1SceneName);
    }
    public void LoadLobbyScene() => LoadScene(LobbySceneName);
    public void ReloadCurrentScene() => LoadScene(currentSceneName);

    #endregion
}
using BansheeGz.BGDatabase;
using System.Collections;
using UnityEngine;

public enum GameState
{
    Menu,
    Playing,
    Paused,
    GameOver,
    Loading
}

public class GameManager : SingletonMono<GameManager>
{
    [Header("Game Settings")]
    public GameState currentGameState = GameState.Menu;
    public bool isGamePaused = false;

    [Header("Respawn Settings")]
    public Transform respawnPoint;      // 유니티 에디터에서 부활시킬 빈 오브젝트(Transform)를 넣어주세요.
    public float respawnDelay = 3.0f;   // 죽고 나서 부활하기까지 걸리는 대기 시간

    [Header("Game Stats")]
    public int currentScore = 0;
    public int currentLevel = 1;
    public float gameTime = 0f;

    private GameState previousGameState;

    public bool isInteractUIOpen = false;

    protected override void Awake()
    {
        base.Awake();
        InitializeGame();

        Debug.Log($"GameManager Awake {Time.realtimeSinceStartup}");
    }

    private void Start()
    {
        // 다른 매니저들이 초기화된 후 실행
        StartCoroutine(InitializeManagers());

        Debug.Log($"GameManager Start {Time.realtimeSinceStartup}");
    }

    private void Update()
    {
        if (currentGameState == GameState.Playing && !isGamePaused)
        {
            gameTime += Time.deltaTime;
        }
    }

    private void InitializeGame()
    {
        // 게임 시작 시 기본 설정
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        UpdateCursorState(currentGameState);

        Debug.Log("GameManager 초기화 완료");
    }

    private IEnumerator InitializeManagers()
    {
        // 모든 스크립트의 Awake와 Start가 완전히 끝날 때까지 1프레임 대기합니다.
        yield return new WaitForEndOfFrame();

        Debug.Log("<color=cyan>=== 시스템 매니저 초기화 및 점검 시작 ===</color>");

        if (DataManager.Instance != null) Debug.Log("DataManager 준비 완료");
        else Debug.LogWarning("DataManager가 씬에 없습니다!");

        if (GameProgressManager.Instance != null) Debug.Log("GameProgressManager 준비 완료");
        if (InventoryManager.Instance != null) Debug.Log("InventoryManager 준비 완료");

        // (기존 세이브 시스템을 병행 사용 중이시라면 유지)
        if (SaveDataHolder.Instance != null)
        {
            if (SaveDataHolder.Instance.currentData == null)
                SaveDataHolder.Instance.Load();
            Debug.Log("SaveDataHolder 준비 완료");
        }
        if (SaveManager.Instance != null) Debug.Log("SaveManager 준비 완료");

        if (ScenesManager.Instance != null) Debug.Log("ScenesManager 준비 완료");
        else Debug.LogError("ScenesManager가 없습니다! 씬 이동이 불가능합니다.");

        if (SoundManager.Instance != null) Debug.Log("SoundManager 준비 완료");
        if (EffectManager.Instance != null) Debug.Log("EffectManager 준비 완료");

        var graphicsManager = FindFirstObjectByType<GraphicsSettingsManager>();
        if (graphicsManager != null)
        {
            graphicsManager.InitUI();
            Debug.Log("GraphicsSettings 적용 완료");
        }

        var soundSettings = FindFirstObjectByType<MixerController>();
        if (soundSettings != null)
        {
            soundSettings.InitUI();
            Debug.Log("SoundSettings 적용 완료");
        }

        Debug.Log("<color=cyan>=== 모든 매니저 초기화 완료! ===</color>");

        if (ScenesManager.Instance != null)
        {
            ScenesManager.Instance.DataLoadCompleted();
        }
    }

    #region Game State Management

    public void ChangeGameState(GameState newState)
    {
        if (currentGameState == newState) return;

        previousGameState = currentGameState;
        currentGameState = newState;

        OnGameStateChanged(newState);

        Debug.Log($"게임 상태 변경: {previousGameState} -> {currentGameState}");
    }

    private void OnGameStateChanged(GameState newState)
    {
        UpdateCursorState(newState);

        switch (newState)
        {
            case GameState.Menu:

                break;
            case GameState.Playing:

                break;
            case GameState.Paused:

                break;
            case GameState.GameOver:

                break;
            case GameState.Loading:

                break;
        }
    }

    public void StartGame()
    {
        ResetGameStats();
        ChangeGameState(GameState.Playing);
        GameEvents.GameResumed();
    }

    public void PauseGame()
    {
        if (currentGameState != GameState.Playing) return;

        isGamePaused = true;
        Time.timeScale = 0f;

        ChangeGameState(GameState.Paused);
        GameEvents.GamePaused();

        // 일시정지 UI 표시
        //UIManager.Instance?.ShowPauseMenu();
    }

    public void ResumeGame()
    {
        if (currentGameState != GameState.Paused) return;

        isGamePaused = false;
        Time.timeScale = 1f;

        ChangeGameState(GameState.Playing);
        GameEvents.GameResumed();

        // 일시정지 UI 숨기기
        //UIManager.Instance?.HidePauseMenu();
    }

    public void GameOver()
    {
        ChangeGameState(GameState.GameOver);

        StartCoroutine(RespawnRoutine());

        // 게임 오버 처리
        //SaveManager.Instance?.SaveHighScore(currentScore);
        //UIManager.Instance?.ShowGameOverUI();
    }

    public void RestartGame()
    {
        ResetGameStats();
        ChangeGameState(GameState.Playing);
    }

    public void GoToMainMenu()
    {
        ChangeGameState(GameState.Menu);
        //SceneManager.Instance?.LoadScene("MainMenu");
    }

    public void UpdateCursorState(GameState state)
    {
        if (isInteractUIOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        switch (state)
        {
            case GameState.Playing:
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
                break;

            case GameState.Menu:
            case GameState.Paused:
            case GameState.GameOver:
            case GameState.Loading:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
        }
    }

    #endregion

    #region 리스폰 관련
    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSecondsRealtime(respawnDelay);

        if (ScenesManager.Instance != null)
            yield return ScenesManager.Instance.StartCoroutine(ScenesManager.Instance.FadeIn());

        PlayerController player = FindObjectOfType<PlayerController>();

        if (player != null)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "BossRushScene")
            {
                GameObject rushRespawn = GameObject.Find("BossRushRespawnPoint");

                if (rushRespawn != null) player.Revive(rushRespawn.transform.position);
                else player.Revive(respawnPoint.position);
            }
            else if (GameProgressManager.Instance != null && GameProgressManager.Instance.progressData.hasSavedRespawn)
            {
                Vector3 savedPos = new Vector3(
                    GameProgressManager.Instance.progressData.respawnPosX,
                    GameProgressManager.Instance.progressData.respawnPosY,
                    GameProgressManager.Instance.progressData.respawnPosZ
                );

                player.Revive(savedPos);
            }
            else if (respawnPoint != null)
            {
                player.Revive(respawnPoint.position);
            }

            ResetAllEnemies();
            ResetAllBossRooms();
            ResetAllFractures();
            ChangeGameState(GameState.Playing);
        }

        yield return new WaitForSecondsRealtime(0.5f);

        if (ScenesManager.Instance != null)
            yield return ScenesManager.Instance.StartCoroutine(ScenesManager.Instance.FadeOut());
    }

    private void ResetAllEnemies()
    {
        Actor[] allActors = FindObjectsOfType<Actor>();

        foreach (Actor actor in allActors)
        {
            if (actor.GetComponent<PlayerController>() == null)
            {
                actor.InitActor(actor.MaxHP);

                Enemy enemy = actor.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.ResetEnemy();
                }
            }
        }
    }

    private void ResetAllBossRooms()
    {
        BossRoomTrigger[] triggers = FindObjectsByType<BossRoomTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var trigger in triggers)
        {
            trigger.ResetRoom();
        }
    }

    private void ResetAllFractures()
    {
        FractureThis[] fractures = FindObjectsByType<FractureThis>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var f in fractures)
        {
            f.ResetFracture();
        }
    }

    #endregion

    #region Score & Stats Management

    public void AddScore(int points)
    {
        currentScore += points;
        GameEvents.ScoreChanged(currentScore);

        Debug.Log($"점수 추가: +{points}, 총 점수: {currentScore}");
    }

    public void SetScore(int score)
    {
        currentScore = score;
        GameEvents.ScoreChanged(currentScore);
    }

    public void NextLevel()
    {
        currentLevel++;
        Debug.Log($"레벨업! 현재 레벨: {currentLevel}");
    }

    private void ResetGameStats()
    {
        currentScore = 0;
        currentLevel = 1;
        gameTime = 0f;
        GameEvents.ScoreChanged(currentScore);
    }

    #endregion

    #region Utility Methods

    public bool IsGamePlaying()
    {
        return currentGameState == GameState.Playing && !isGamePaused;
    }

    public void QuitGame()
    {
        Debug.Log("게임 종료");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion

/*    private void OnApplicationPause(bool pauseStatus)
    {
#if !UNITY_EDITOR
        if (pauseStatus && currentGameState == GameState.Playing)
        {
            PauseGame();
        }
#endif
    }

    private void OnApplicationFocus(bool hasFocus)
    {
#if !UNITY_EDITOR
        if (!hasFocus && currentGameState == GameState.Playing)
        {
            PauseGame();
        }
#endif
    }*/
}
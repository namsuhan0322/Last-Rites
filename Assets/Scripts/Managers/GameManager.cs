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

    protected override void Awake()
    {
        base.Awake();
        InitializeGame();
    }

    private void Start()
    {
        // 다른 매니저들이 초기화된 후 실행
        StartCoroutine(InitializeManagers());
    }

    private void Update()
    {
        if (currentGameState == GameState.Playing && !isGamePaused)
        {
            gameTime += Time.deltaTime;
        }

        HandleInput();
    }

    private void InitializeGame()
    {
        // 게임 시작 시 기본 설정
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        Debug.Log("GameManager 초기화 완료");
    }

    private IEnumerator InitializeManagers()
    {
        yield return new WaitForEndOfFrame();

        Debug.Log("=== 매니저 초기화 시작 ===");

        if (SaveDataHolder.Instance != null)
        {
            // 혹시 데이터가 없다면 여기서 강제 로드
            if (SaveDataHolder.Instance.currentData == null)
                SaveDataHolder.Instance.Load();

            Debug.Log("SaveDataHolder 준비 완료");
        }
        if (ScenesManager.Instance != null) Debug.Log("ScenesManager 준비 완료");
        if (SoundManager.Instance != null) Debug.Log("SoundManager 준비 완료");
        if (EffectManager.Instance != null) Debug.Log("EffectManager 준비 완료");

        var graphicsManager = FindObjectOfType<GraphicsSettingsManager>();
        if (graphicsManager != null)
        {
            graphicsManager.InitUI();
            Debug.Log("GraphicsSettings 적용 완료");
        }

        var soundSettings = FindObjectOfType<MixerController>();
        if (soundSettings != null)
        {
            soundSettings.InitUI();
            Debug.Log("SoundSettings 적용 완료");
        }

        if (SaveManager.Instance != null) Debug.Log("SaveManager 준비 완료");

        Debug.Log("=== 모든 매니저 초기화 완료 ===");
    }

    private void HandleInput()
    {
        // ESC 키로 게임 일시정지/재개
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentGameState == GameState.Playing)
            {
                PauseGame();
            }
            else if (currentGameState == GameState.Paused)
            {
                ResumeGame();
            }
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
        ChangeGameState(GameState.Paused);
        GameEvents.GamePaused();

        // 일시정지 UI 표시
        //UIManager.Instance?.ShowPauseMenu();
    }

    public void ResumeGame()
    {
        if (currentGameState != GameState.Paused) return;

        isGamePaused = false;
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
            if (GameProgressManager.Instance != null && GameProgressManager.Instance.progressData.hasSavedRespawn)
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

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && currentGameState == GameState.Playing)
        {
            PauseGame();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && currentGameState == GameState.Playing)
        {
            PauseGame();
        }
    }
}
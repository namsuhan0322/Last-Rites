using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cinemachine;
using TMPro;
using UnityEngine.UI;

public class BossRushGameplayManager : MonoBehaviour
{
    [Header("보스 러쉬 설정")]
    public List<GameObject> bossPrefabs;
    public Transform spawnPoint;
    public float startDelay = 2.0f;
    public float spawnDelay = 3.0f;

    [Header("제한 시간 설정")]
    public float timeLimit = 300f;
    public GameObject timerObj;
    public TextMeshProUGUI timerText;

    [Header("보스방 트리거 및 기본 UI 설정")]
    public BossHealthUI bossHealthUI;
    public GameObject doorObj;
    public CinemachineVirtualCamera bossCamera;
    public TextMeshProUGUI countdownText;

    [Header("클리어 매니저 연결")]
    public BossRushClearManager rushClearManager;

    [Header("팝업 UI")]
    public GameObject failPopupObj;
    public TextMeshProUGUI failMainText;
    public Button retryButton;
    public Button exitButton;

    [Header("현재 진행 상태")]
    public int currentBossIndex = 0;
    private GameObject currentBossInstance;
    private bool isCurrentBossDead = false;
    private bool isRushStarted = false;
    private bool isCleared = false;
    private float currentTime;
    private bool isTimerPaused = false;

    private PlayerController _player;

    public event Action<int, int> OnWaveChanged;
    public event Action OnBossRushCleared;

    private void Start()
    {
        if (bossCamera != null) bossCamera.Priority = 0;
        if (doorObj != null) doorObj.SetActive(false);
        if (failPopupObj != null) failPopupObj.SetActive(false);
        if (retryButton != null) retryButton.onClick.AddListener(OnClickRetry);
        if (exitButton != null) exitButton.onClick.AddListener(OnClickExit);

        if (timerObj != null) timerObj.SetActive(false);
        currentTime = timeLimit;
        UpdateTimerUI(currentTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isRushStarted && other.CompareTag("Player"))
        {
            isRushStarted = true;
            isCleared = false;
            isTimerPaused = false;

            _player = other.GetComponent<PlayerController>();
            if (_player != null)
            {
                _player.Stats.OnDeath += HandlePlayerDeath;
            }
            Debug.Log("[BossRush] 플레이어 입장! 보스러쉬를 시작합니다.");

            if (doorObj != null) doorObj.SetActive(true);
            if (bossCamera != null) bossCamera.Priority = 20;

            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            StartCoroutine(BossRushRoutine());
        }
    }

    private IEnumerator TimerRoutine()
    {
        while (currentTime > 0 && !isCleared)
        {
            if (!isTimerPaused)
            {
                currentTime -= Time.deltaTime;
                UpdateTimerUI(currentTime);
            }
            yield return null;
        }

        if (isCleared) yield break;

        currentTime = 0;
        UpdateTimerUI(0);
        Debug.Log("<color=red>[BossRush] 시간 초과! 플레이어 사망 처리</color>");

        if (_player != null && _player.Stats != null && !_player.Stats.IsDead)
        {
            _player.Stats.TakeDamage(Mathf.RoundToInt(_player.Stats.MaxHP * 10f));
        }
        else
        {
            HandlePlayerDeath();
        }
    }

    private void UpdateTimerUI(float time)
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        timerText.color = time <= 30f ? Color.red : Color.white;
    }

    private IEnumerator BossRushRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        if (timerObj != null) timerObj.SetActive(true);
        currentTime = timeLimit;
        isTimerPaused = false;
        StartCoroutine(TimerRoutine());

        while (currentBossIndex < bossPrefabs.Count)
        {
            if (currentBossIndex > 0)
            {
                yield return StartCoroutine(ShowCountdown());
            }

            SpawnBoss(currentBossIndex);
            yield return new WaitUntil(() => isCurrentBossDead);

            currentBossIndex++;

            if (currentBossIndex < bossPrefabs.Count)
                yield return new WaitForSeconds(spawnDelay);
        }

        isCleared = true;

        if (_player != null) _player.Stats.OnDeath -= HandlePlayerDeath;
        if (bossCamera != null) bossCamera.Priority = 0;
        if (doorObj != null) doorObj.SetActive(false);
        if (timerObj != null) timerObj.SetActive(false);

        if (rushClearManager != null)
        {
            rushClearManager.ShowBossRushClearSequence();
        }

        OnBossRushCleared?.Invoke();
    }

    private void HandlePlayerDeath()
    {
        if (_player != null) _player.Stats.OnDeath -= HandlePlayerDeath;

        isTimerPaused = true;
        StopAllCoroutines();

        if (currentBossInstance != null) Destroy(currentBossInstance);
        if (bossHealthUI != null) bossHealthUI.HideBossUI();
        if (doorObj != null) doorObj.SetActive(false);
        if (bossCamera != null) bossCamera.Priority = 0;
        if (timerObj != null) timerObj.SetActive(false);

        StartCoroutine(ShowFailPopupRoutine());
    }

    private IEnumerator ShowFailPopupRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (failPopupObj != null)
        {
            failPopupObj.SetActive(true);
            if (failMainText != null)
            {
                failMainText.text = "실패하였습니다.\n다시 하시겠습니까?";
            }
        }

        Time.timeScale = 0f;
    }

    private void OnClickRetry()
    {
        Time.timeScale = 1f;

        if (ScenesManager.Instance != null)
            ScenesManager.Instance.ReloadCurrentScene();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void OnClickExit()
    {
        Time.timeScale = 1f;

        if (ScenesManager.Instance != null)
            ScenesManager.Instance.LoadLobbyScene();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
    }

    private void SpawnBoss(int index)
    {
        if (bossPrefabs[index] == null) return;
        isCurrentBossDead = false;
        isTimerPaused = false;

        currentBossInstance = Instantiate(bossPrefabs[index], spawnPoint.position, spawnPoint.rotation);
        Actor bossActor = currentBossInstance.GetComponent<Actor>();

        if (bossActor != null)
        {
            bossActor.OnDeath += HandleBossDeath;
            if (bossHealthUI != null)
            {
                bossHealthUI.UpdateBossReference(bossActor);
                bossHealthUI.ShowBossUI();
            }
        }
    }

    private void HandleBossDeath()
    {
        if (bossHealthUI != null) bossHealthUI.HideBossUI();
        isCurrentBossDead = true;
        isTimerPaused = true;
    }

    private void OnDestroy()
    {
        if (_player != null) _player.Stats.OnDeath -= HandlePlayerDeath;
    }

    IEnumerator ShowCountdown()
    {
        if (countdownText == null) yield break;

        countdownText.transform.parent.gameObject.SetActive(true);
        countdownText.gameObject.SetActive(true);

        Color baseColor = countdownText.color;
        string[] numbers = { "3", "2", "1" };

        foreach (var n in numbers)
        {
            countdownText.text = n;
            countdownText.transform.localScale = Vector3.one * 0.3f;
            countdownText.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);

            float duration = 1f;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float normalized = t / duration;

                float scale = Mathf.Lerp(0.3f, 1.2f, normalized);
                countdownText.transform.localScale = Vector3.one * scale;

                float alpha = 1f - Mathf.Clamp01((normalized - 0.4f) / 0.6f);
                countdownText.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

                yield return null;
            }
            yield return new WaitForSeconds(0.1f);
        }
        countdownText.gameObject.SetActive(false);
    }
}
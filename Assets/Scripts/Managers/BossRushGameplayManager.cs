using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cinemachine;
using TMPro;

public class BossRushGameplayManager : MonoBehaviour
{
    [Header("보스 러쉬 설정")]
    public List<GameObject> bossPrefabs;
    public Transform spawnPoint;
    public float startDelay = 2.0f;
    public float spawnDelay = 3.0f;

    [Header("보스방 트리거 및 UI 설정")]
    public BossHealthUI bossHealthUI;
    public GameObject doorObj;
    public CinemachineVirtualCamera bossCamera;
    public TextMeshProUGUI countdownText;

    [Header("클리어 매니저 연결")]
    public BossRushClearManager rushClearManager;

    [Header("현재 진행 상태")]
    public int currentBossIndex = 0;
    private GameObject currentBossInstance;
    private bool isCurrentBossDead = false;
    private bool isRushStarted = false; // 트리거 중복 실행 방지용

    private PlayerController _player;

    public event Action<int, int> OnWaveChanged;
    public event Action OnBossRushCleared;

    private void Start()
    {
        // 시작 시 카메라와 문 초기화
        if (bossCamera != null) bossCamera.Priority = 0;
        if (doorObj != null) doorObj.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isRushStarted && other.CompareTag("Player"))
        {
            isRushStarted = true; // 중복 실행 방지

            _player = other.GetComponent<PlayerController>();
            if (_player != null)
            {
                _player.Stats.OnDeath += HandlePlayerDeath;
            }
            Debug.Log("[BossRush] 플레이어 입장! 보스러쉬를 시작합니다.");

            // 문 닫기 & 카메라 전환
            if (doorObj != null) doorObj.SetActive(true);
            if (bossCamera != null) bossCamera.Priority = 20;

            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // 보스러쉬 루틴 가동
            StartCoroutine(BossRushRoutine());
        }
    }

    private void HandlePlayerDeath()
    {
        if (_player != null) _player.Stats.OnDeath -= HandlePlayerDeath;

        StopAllCoroutines();

        if (currentBossInstance != null) Destroy(currentBossInstance);
        if (bossHealthUI != null) bossHealthUI.HideBossUI();
        if (doorObj != null) doorObj.SetActive(false);
        if (bossCamera != null) bossCamera.Priority = 0;

        isRushStarted = false;
        currentBossIndex = 0;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;
    }

    private IEnumerator BossRushRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        while (currentBossIndex < bossPrefabs.Count)
        {
            OnWaveChanged?.Invoke(currentBossIndex + 1, bossPrefabs.Count);

            yield return StartCoroutine(ShowCountdown());

            SpawnBoss(currentBossIndex);

            yield return new WaitUntil(() => isCurrentBossDead);

            currentBossIndex++;

            if (currentBossIndex < bossPrefabs.Count)
            {
                yield return new WaitForSeconds(spawnDelay);
            }
        }

        if (_player != null) _player.Stats.OnDeath -= HandlePlayerDeath;
        if (bossCamera != null) bossCamera.Priority = 0;
        if (doorObj != null) doorObj.SetActive(false);

        if (rushClearManager != null)
        {
            rushClearManager.ShowBossRushClearSequence();
        }

        OnBossRushCleared?.Invoke();
    }

    private void SpawnBoss(int index)
    {
        if (bossPrefabs[index] == null) return;

        isCurrentBossDead = false;

        currentBossInstance = Instantiate(bossPrefabs[index], spawnPoint.position, spawnPoint.rotation);
        Debug.Log($"[BossRush] {currentBossInstance.name} 등장!");

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
    }

    private void OnDestroy()
    {
        if (_player != null) _player.Stats.OnDeath -= HandlePlayerDeath;
    }

    //카운트다운
    IEnumerator ShowCountdown()
    {
        if (countdownText == null)
            yield break;

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
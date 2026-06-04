using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cinemachine;

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

    [Header("클리어 매니저 연결")]
    public BossRushClearManager rushClearManager;

    [Header("현재 진행 상태")]
    public int currentBossIndex = 0;
    private GameObject currentBossInstance;
    private bool isCurrentBossDead = false;
    private bool isRushStarted = false; // 트리거 중복 실행 방지용

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

    private IEnumerator BossRushRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        while (currentBossIndex < bossPrefabs.Count)
        {
            OnWaveChanged?.Invoke(currentBossIndex + 1, bossPrefabs.Count);

            SpawnBoss(currentBossIndex);

            yield return new WaitUntil(() => isCurrentBossDead);

            currentBossIndex++;

            if (currentBossIndex < bossPrefabs.Count)
            {
                yield return new WaitForSeconds(spawnDelay);
            }
        }

        Debug.Log("[BossRush] 보스 러쉬 완벽 클리어!");

        // 카메라 원상 복구 및 문 열기
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
        Debug.Log("[BossRush] 현재 보스가 쓰러졌습니다!");

        if (bossHealthUI != null) bossHealthUI.HideBossUI();

        isCurrentBossDead = true;
    }
}
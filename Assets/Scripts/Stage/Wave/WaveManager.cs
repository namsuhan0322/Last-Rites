using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


[System.Serializable]
public class TowerFloorSetting
{
    public int floor;

    [Header("Floor Type")]
    public bool isBombSurvivalFloor = false;

    [Header("Bomb Survival Settings")]
    public EnemyData suicideBombEnemy;
    public float survivalTime = 60f;
    public float bombSpawnInterval = 2f;

    [Header("Wave 1")]
    public List<TowerEnemySpawnInfo> wave1Enemies = new List<TowerEnemySpawnInfo>();

    [Header("Wave 2")]
    public List<TowerEnemySpawnInfo> wave2Enemies = new List<TowerEnemySpawnInfo>();

    [Header("Wave 3")]
    public List<TowerEnemySpawnInfo> wave3Enemies = new List<TowerEnemySpawnInfo>();
}
[System.Serializable]
public class TowerEnemySpawnInfo
{
    public EnemyData enemy;

    [Min(1)]
    public int count = 1;
}

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public int maxWave = 3;

    [Header("Spawn Area")]
    public Transform spawnAreaQuad;

    [Header("Elite Spawn Points")]
    public Transform eliteSpawnPointLeft;
    public Transform eliteSpawnPointRight;

    public float delayBeforeNextWave = 3f;

    [Header("Spawn")]
    public GameObject spawnIndicatorPrefab;
    public float spawnDelay = 1.5f;

    [Header("UI")]
    public TextMeshProUGUI countdownText;

    [Header("Clear")]
    public StageClearManager stageClearManager;

    [Header("Boss UI")]
    public BossHealthUI bossHealthUI;

    [Header("Tower Floor Settings")]
    public List<TowerFloorSetting> floorSettings = new List<TowerFloorSetting>();

    private TowerFloorSetting currentFloorSetting;

    int waveIndex = 1;
    int aliveEnemies = 0;
    bool isClear = false;
    private int eliteSpawnIndex = 0;
    private bool useLeftFirst;
    GameObject[] plans;

    void Start()
    {
        ApplyTowerFloorSetting();

        if (currentFloorSetting != null && currentFloorSetting.isBombSurvivalFloor)
            StartCoroutine(StartBombSurvivalFloor());
        else
            StartCoroutine(StartWaveLoop());
    }

    IEnumerator StartWaveLoop()
    {
        while (waveIndex <= maxWave)
        {
            List<TowerEnemySpawnInfo> enemies = GetEnemiesForCurrentWave();

            if (enemies == null || enemies.Count == 0)
            {
                waveIndex++;
                continue;
            }

            yield return StartCoroutine(ShowCountdown());

            yield return StartCoroutine(StartWave());

            if (waveIndex >= maxWave)
            {
                ClearTower();
                yield break;
            }

            yield return new WaitForSeconds(delayBeforeNextWave);
            waveIndex++;
        }

        ClearTower();
    }

    IEnumerator StartBombSurvivalFloor()
    {
        yield return StartCoroutine(ShowCountdown());

        float timer = 0f;
        float spawnTimer = 0f;

        while (timer < currentFloorSetting.survivalTime)
        {
            timer += Time.deltaTime;
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= currentFloorSetting.bombSpawnInterval)
            {
                spawnTimer = 0f;

                if (currentFloorSetting.suicideBombEnemy != null)
                {
                    StartCoroutine(SpawnEnemyWithWarning(currentFloorSetting.suicideBombEnemy));
                }
            }

            yield return null;
        }

        ClearTower();
    }

    IEnumerator StartWave()
    {
        eliteSpawnIndex = 0;
        useLeftFirst = Random.value < 0.5f;

        List<TowerEnemySpawnInfo> spawnInfos = GetEnemiesForCurrentWave();

        List<Coroutine> coroutines = new List<Coroutine>();

        foreach (TowerEnemySpawnInfo info in spawnInfos)
        {
            if (info == null || info.enemy == null)
                continue;

            for (int i = 0; i < info.count; i++)
            {
                coroutines.Add(StartCoroutine(SpawnEnemyWithWarning(info.enemy)));
            }
        }

        foreach (var co in coroutines)
            yield return co;

        while (aliveEnemies > 0)
            yield return null;
    }

    List<TowerEnemySpawnInfo> GetEnemiesForCurrentWave()
    {
        if (currentFloorSetting == null)
        {
            Debug.LogWarning("현재 층 설정이 없습니다.");
            return new List<TowerEnemySpawnInfo>();
        }

        if (waveIndex == 1)
            return currentFloorSetting.wave1Enemies;

        if (waveIndex == 2)
            return currentFloorSetting.wave2Enemies;

        return currentFloorSetting.wave3Enemies;
    }

    public void OnEnemyDead()
    {
        aliveEnemies--;

        Debug.Log("적 사망 처리됨 / 남은 적 수 : " + aliveEnemies);

        if (aliveEnemies < 0)
            aliveEnemies = 0;
    }

    IEnumerator SpawnEnemyWithWarning(EnemyData data)
    {
        Vector3 pos = GetSpawnPosition(data);

        GameObject indicator = Instantiate(
            spawnIndicatorPrefab,
            pos + Vector3.up * 0.02f,
            Quaternion.Euler(90, 0, 0)
        );

        SpawnIndicator effect = indicator.GetComponent<SpawnIndicator>();

        if (effect != null)
            StartCoroutine(effect.Play());

        aliveEnemies++;
        Debug.Log("적 생성 예약 / 현재 적 수 : " + aliveEnemies);

        yield return new WaitForSeconds(spawnDelay);

        GameObject go = Instantiate(data.prefab, pos + Vector3.up, Quaternion.identity);

        Enemy enemy = go.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.Init(this, data);

            // 엘리트나 보스면 HP UI 연결
            if (data.rank == EnemyRank.Elite || data.rank == EnemyRank.Boss)
            {
                if (bossHealthUI != null)
                {
                    bossHealthUI.SetBossOnSpawn(enemy);
                }
            }
        }
        else
        {
            aliveEnemies--;
            Debug.LogWarning(go.name + "에 Enemy 컴포넌트가 없습니다.");
        }

        Destroy(indicator);
    }

    void ClearTower()
    {
        if (isClear)
            return;

        isClear = true;

        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (Enemy enemy in enemies)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }

        if (TowerManager.Instance != null)
            TowerManager.Instance.ClearSelectedFloor();

        if (stageClearManager != null)
            stageClearManager.ShowClearSequence();
        else
            Debug.LogWarning("StageClearManager가 연결되지 않았습니다.");
    }

    Vector3 GetSpawnPosition(EnemyData data)
    {
        if (data.rank == EnemyRank.Elite || data.rank == EnemyRank.Boss)
        {
            return GetEliteSpawnPosition();
        }

        return GetRandomSpawnPosition();
    }

    //엘리트 전용 스폰 포인트
    Vector3 GetEliteSpawnPosition()
    {
        if (eliteSpawnPointLeft == null || eliteSpawnPointRight == null)
        {
            Debug.LogWarning("Elite Spawn Point Left/Right가 연결되지 않았습니다.");
            return GetRandomSpawnPosition();
        }

        Transform first = useLeftFirst ? eliteSpawnPointLeft : eliteSpawnPointRight;
        Transform second = useLeftFirst ? eliteSpawnPointRight : eliteSpawnPointLeft;

        Transform selected;

        if (eliteSpawnIndex == 0)
            selected = first;
        else
            selected = second;

        eliteSpawnIndex++;

        return selected.position;
    }

    Vector3 GetRandomSpawnPosition()
    {
        if (spawnAreaQuad == null)
        {
            Debug.LogWarning("Spawn Area Quad가 연결되지 않았습니다.");
            return transform.position;
        }

        Vector3 center = spawnAreaQuad.position;

        float halfX = spawnAreaQuad.localScale.x * 0.5f;
        float halfZ = spawnAreaQuad.localScale.y * 0.5f;

        float randomX = Random.Range(-halfX, halfX);
        float randomZ = Random.Range(-halfZ, halfZ);

        return center + new Vector3(randomX, 0f, randomZ);
    }

    IEnumerator ShowCountdown()
    {

        if (countdownText == null)
        {
            Debug.LogWarning("Countdown Text가 WaveManager에 연결되지 않았습니다.");
            yield break;
        }

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

    //클리어한 층수
    void ApplyTowerFloorSetting()
    {
        int selectedFloor = 1;

        if (TowerManager.Instance != null)
            selectedFloor = TowerManager.Instance.selectedFloor;

        currentFloorSetting = floorSettings.Find(x => x.floor == selectedFloor);

        if (currentFloorSetting == null)
        {
            Debug.LogWarning($"{selectedFloor}층 설정이 없습니다.");
            return;
        }

        Debug.Log($"{selectedFloor}층 설정 적용 완료");
    }
}
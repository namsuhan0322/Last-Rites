using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class WaveEntry
{
    public EnemyData enemy;

    [Tooltip("Minion / Elite 구분용")]
    public EnemyGrade grade;

    [Tooltip("가중치")]
    public int weight = 1;
}

public enum EnemyGrade
{
    Minion,
    Elite
}

public class WaveManager : MonoBehaviour
{
    [Header("Enemy Pool")]
    public List<WaveEntry> enemyPool = new List<WaveEntry>();

    [Header("Wave Settings")]
    public int maxWave = 3;
    public int minionCountWave1 = 5;
    public int minionCountWave2 = 7;
    public int eliteCountWave3 = 1;

    [Header("Spawn Area")]
    public Transform spawnAreaQuad;

    [Header("Elite Spawn Point")]
    public Transform eliteSpawnPoint;

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

    int waveIndex = 1;
    int aliveEnemies = 0;
    bool isClear = false;

    GameObject[] plans;

    void Start()
    {
        StartCoroutine(StartWaveLoop());
    }

    IEnumerator StartWaveLoop()
    {
        while (waveIndex <= maxWave)
        {
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
    }

    IEnumerator StartWave()
    {
        int spawnCount = GetSpawnCount();
        EnemyGrade grade = GetWaveGrade();

        List<Coroutine> coroutines = new List<Coroutine>();

        for (int i = 0; i < spawnCount; i++)
        {
            EnemyData enemy = PickEnemyByGrade(grade);

            if (enemy != null)
                coroutines.Add(StartCoroutine(SpawnEnemyWithWarning(enemy)));
        }

        foreach (var co in coroutines)
            yield return co;

        while (aliveEnemies > 0)
            yield return null;
    }

    int GetSpawnCount()
    {
        if (waveIndex == 1)
            return minionCountWave1;

        if (waveIndex == 2)
            return minionCountWave2;

        return eliteCountWave3;
    }

    EnemyGrade GetWaveGrade()
    {
        if (waveIndex == 3)
            return EnemyGrade.Elite;

        return EnemyGrade.Minion;
    }

    EnemyData PickEnemyByGrade(EnemyGrade grade)
    {
        List<WaveEntry> list = enemyPool.FindAll(e => e.grade == grade);

        if (list.Count == 0)
        {
            Debug.LogWarning($"{grade} 등급 몬스터가 EnemyPool에 없습니다.");
            return null;
        }

        int totalWeight = 0;

        foreach (var e in list)
            totalWeight += e.weight;

        int r = Random.Range(0, totalWeight);

        foreach (var e in list)
        {
            if (r < e.weight)
                return e.enemy;

            r -= e.weight;
        }

        return list[0].enemy;
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

        if (stageClearManager != null)
            stageClearManager.ShowClearSequence();
        else
            Debug.LogWarning("StageClearManager가 연결되지 않았습니다.");
    }

    Vector3 GetSpawnPosition(EnemyData data)
    {
        if (data.rank == EnemyRank.Elite && eliteSpawnPoint != null)
        {
            return eliteSpawnPoint.position;
        }

        return GetRandomSpawnPosition();
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
}
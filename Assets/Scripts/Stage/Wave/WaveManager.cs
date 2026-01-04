using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


[System.Serializable]
public class WaveEntry
{
    public EnemyData enemy;

    [Tooltip("적이 생성되는 웨이브")]
    public int unlockWave = 1;

    [Tooltip("가중치")]
    public int weight = 1;
}

public class WaveManager : MonoBehaviour
{
    [Header("Enemy Pool")]
    public List<WaveEntry> enemyPool = new List<WaveEntry>();

    [Header("Wave Settings")]
    public int baseEnemiesPerWave = 5;
    public float delayBeforeNextWave = 3f;
    public float difficultyGrowth = 1.2f;  //난이도 조정 수치

    //생성효과들
    public GameObject spawnIndicatorPrefab;
    public float spawnDelay = 1.5f;

    //UI
    public TextMeshProUGUI countdownText;

    //변수들
    int waveIndex = 1;
    int aliveEnemies = 0;
    GameObject[] plans;

    void Start()
    {
        plans = GameObject.FindGameObjectsWithTag("Plan");
        StartCoroutine(StartWaveLoop());
    }

    #region 웨이브 반복문
    IEnumerator StartWaveLoop()
    {
        while (true)
        {
            yield return StartCoroutine(ShowCountdown());

            yield return StartCoroutine(StartWave());

            yield return new WaitForSeconds(delayBeforeNextWave);
            waveIndex++;
        }
    }
    #endregion

    //웨이브 시작
    IEnumerator StartWave()
    {
        int enemiesThisWave = Mathf.RoundToInt(baseEnemiesPerWave * Mathf.Pow(difficultyGrowth, waveIndex - 1));

        List<Coroutine> coroutines = new List<Coroutine>();

        for (int i = 0; i < enemiesThisWave; i++)
        {
            coroutines.Add(StartCoroutine(SpawnEnemyWithWarning(PickEnemyForWave())));
        }

        foreach (var co in coroutines)
        {
            yield return co;
        }

        while (aliveEnemies > 0)
            yield return null;
    }

    #region 웨이브에서 적 뽑기
    EnemyData PickEnemyForWave()
    {
        List<WaveEntry> unlocked = enemyPool.FindAll(e => e.unlockWave <= waveIndex);

        int totalWeight = 0;
        foreach (var e in unlocked) totalWeight += e.weight;

        int r = Random.Range(0, totalWeight);
        foreach (var e in unlocked)
        {
            if (r < e.weight) return e.enemy;
            r -= e.weight;
        }
        return unlocked[0].enemy;
    }
    #endregion
    public void OnEnemyDead() => aliveEnemies--;
    #region 적 소환
    IEnumerator SpawnEnemyWithWarning(EnemyData data)
    {
        GameObject plan = plans[Random.Range(0, plans.Length)];
        Bounds b = plan.GetComponent<Renderer>().bounds;

        Vector3 pos = new Vector3(
            Random.Range(b.min.x, b.max.x),
            b.max.y + 0.05f,
            Random.Range(b.min.z, b.max.z)
        );

        var indicator = Instantiate(
            spawnIndicatorPrefab,
            pos + Vector3.up * 0.02f,
            Quaternion.Euler(90, 0, 0)
        );

        var effect = indicator.GetComponent<SpawnIndicator>();
        if (effect != null)
            StartCoroutine(effect.Play());

        yield return new WaitForSeconds(spawnDelay);

        var go = Instantiate(data.prefab, pos + Vector3.up, Quaternion.identity);
        go.GetComponent<Enemy>().Init(this, data);

        aliveEnemies++;

        Destroy(indicator);
    }
    #endregion
    #region 카운트다운
    IEnumerator ShowCountdown()
    {
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
    #endregion
}

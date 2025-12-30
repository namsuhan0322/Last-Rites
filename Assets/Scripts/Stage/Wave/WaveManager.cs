using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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

        for (int i = 0; i < enemiesThisWave; i++)
        {
            SpawnEnemy(PickEnemyForWave());
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
    #region 적 소환
    void SpawnEnemy(EnemyData data)
    {
        GameObject plan = plans[Random.Range(0, plans.Length)];
        Bounds b = plan.GetComponent<Renderer>().bounds;

        Vector3 pos = new Vector3(
            Random.Range(b.min.x, b.max.x),
            b.max.y + 1f,
            Random.Range(b.min.z, b.max.z)
        );

        var go = Instantiate(data.prefab, pos, Quaternion.identity);
        go.GetComponent<Enemy>().Init(this, data);
        aliveEnemies++;
    }
    #endregion

    public void OnEnemyDead() => aliveEnemies--;
}

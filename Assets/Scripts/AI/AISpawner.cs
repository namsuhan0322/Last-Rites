using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[System.Serializable]
public class InitialAISpawn
{
    public int aiId;
    public int count = 1;
}

public class AISpawner : MonoBehaviour
{

    [Header("데이터")]
    [SerializeField] private AIDatabaseSO database;

    [Header("프리팹")]
    [SerializeField] private AIBase knightPrefab;
    [SerializeField] private AIBase healerPrefab;
    [SerializeField] private AIBase dealerPrefab;
    [SerializeField] private AIBase rangerPrefab;

    [Header("초기 소환")]
    [SerializeField] private List<InitialAISpawn> initialSpawnList;

    [Header("스폰 옵션")]
    [SerializeField] private float spawnRadius = 3f;
    [SerializeField] private float minDistanceBetweenAI = 1.5f;
    [SerializeField] private LayerMask aiLayer;

    void Start()
    {
        SpawnInitialAI();
    }

    void SpawnInitialAI()
    {
        foreach (var spawn in initialSpawnList)
        {
            for (int i = 0; i < spawn.count; i++)
            {
                SpawnById(spawn.aiId);
            }
        }
    }
    public AIBase SpawnById(int aiId)
    {
        AISO data = database.GetItemById(aiId);
        if (data == null)
        {
            Debug.LogError($"AISO not found : {aiId}");
            return null;
        }

        Vector3 pos = GetSpawnPositionNearPlayer();
        AIBase prefab = GetPrefab(data.roleType);

        AIBase ai = Instantiate(prefab, pos, Quaternion.identity);
        ai.Setup(data);

        return ai;
    }

    Vector3 GetSpawnPositionNearPlayer()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player not found");
            return transform.position;
        }

        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPos =
                player.position +
                Random.insideUnitSphere * spawnRadius;

            randomPos.y = player.position.y;

            bool overlapped = Physics.CheckSphere(
                randomPos,
                minDistanceBetweenAI,
                aiLayer
            );

            if (!overlapped)
                return randomPos;
        }

        return player.position + player.right * spawnRadius;
    }

    AIBase GetPrefab(RoleType type)
    {
        switch (type)
        {
            case RoleType.Tanker: return knightPrefab;
            case RoleType.Healer: return healerPrefab;
            case RoleType.Dealer: return dealerPrefab;
            case RoleType.Ranger: return rangerPrefab;
        }

        Debug.LogError($"No prefab for role {type}");
        return null;
    }
}

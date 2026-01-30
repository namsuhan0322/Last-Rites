using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Wave/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public GameObject prefab;

    [Header("Stats")]
    public int enemyHp;
    public float detectRadius;
    public float patrolRadius;
    public float patrolWaitTime;
    public float patrolSpeed;
    public float chaseSpeed;
}

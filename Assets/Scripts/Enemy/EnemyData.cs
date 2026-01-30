using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Wave/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public GameObject prefab;

    [Header("Stats")]
    public int enemyHp = 10;

    [Header("이동")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float detectRadius = 6f;
    public float patrolRadius = 0f;
    public float patrolWaitTime = 0f;

    [Header("공격")]
    public int attackDamage = 5;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
}

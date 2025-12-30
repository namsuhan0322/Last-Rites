using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Wave/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public GameObject prefab;

    [Header("Stats")]
    public int Enemyhp = 10;
    public float moveSpeed = 3f;
}

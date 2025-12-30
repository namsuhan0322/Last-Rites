using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    WaveManager manager;
    EnemyData data;
    int hp;   

    public void Init(WaveManager manager, EnemyData data)
    {
        this.manager = manager;
        this.data = data;

        hp = data.Enemyhp;
    }

    //데미지를 받았나?
    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        if (hp <= 0)
            Destroy(gameObject);
    }


    //죽었나?
    void OnDestroy()
    {
        if (manager != null)
            manager.OnEnemyDead();
    }
}

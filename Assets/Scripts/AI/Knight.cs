using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : AIBase
{
    [Header("도발 스킬")]
    public float tauntRadius = 6f;      // 스킬 범위
    public float tauntDuration = 5f;    // 도발 지속 시간
    public float tauntCooldown = 20f;   // 쿨타임

    bool canTaunt = true;

    protected override void Update()
    {
        base.Update();     
        TryTaunt();         //도발
    }


    // ----------- 도발 체크 -------------
    void TryTaunt()
    {
        if (!canTaunt) return;

        Collider[] enemies = Physics.OverlapSphere(
            transform.position,
            tauntRadius,
            enemyLayer
        );

        if (enemies.Length >= 3)
        {
            canTaunt = false;
            StartCoroutine(Taunt(enemies));
        }
    }

    //-------------도발-----------
    IEnumerator Taunt(Collider[] enemies)
    {
        foreach (var e in enemies)
        {
            Enemy enemy = e.GetComponent<Enemy>();

            if (enemy != null)
                enemy.ForceTarget(transform, tauntDuration);
        }

        yield return new WaitForSeconds(tauntCooldown);
        canTaunt = true;
    }

    // -------- 걷기 애니메이션 ----------
    protected override void SetWalking(bool walking)
    {
    }
}

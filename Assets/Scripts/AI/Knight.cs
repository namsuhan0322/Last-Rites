using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : AIBase
{
    [Header("도발 스킬")]
    public float tauntRadius = 6f;
    public float tauntCooldown = 20f;

    bool canTaunt = true;

    protected override void Update()
    {
        base.Update();
        TryTaunt();
    }

    void TryTaunt()
    {
        if (!canTaunt) return;

        Collider[] aroundPlayer = Physics.OverlapSphere(
            player.position,
            attackDetectRadius,
            enemyLayer
        );

        if (aroundPlayer.Length < 3)
            return;
        StartCoroutine(Taunt());
    }

    IEnumerator Taunt()
    {
        canTaunt = false;

        Collider[] enemies = Physics.OverlapSphere(
            transform.position,
            tauntRadius,
            enemyLayer
        );

        foreach (var e in enemies)
        {
            Enemy enemy = e.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.ForceTarget(transform, Mathf.Infinity);
            }
        }
        yield return new WaitForSeconds(tauntCooldown);
        canTaunt = true;
    }

    protected override void SetWalking(bool walking) { }
}

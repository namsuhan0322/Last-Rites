using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : AIBase
{
    [Header("도발 스킬")]
    [SerializeField] float tauntRadius = 6f;     // 고정 반경
    float tauntDuration;
    float tauntCooldown;
    public GameObject speechBubblePrefab;   
    Transform speechPoint;

    [Header("방패치기")]
    [SerializeField] float bashRadius = 6f;      // 근접 반경
    float bashStunTime;
    float bashCooldown;

    bool canBash = true;
    bool canTaunt = true;

    //AISOs에서 가져오는 스킬 쿨, 수치
    public override void Setup(AISO aiData)
    {
        base.Setup(aiData);

        // S1 = 도발
        tauntDuration = data.S1_Val;
        tauntCooldown = data.S1_Cool;

        // S2 = 방패치기
        bashStunTime = data.S2_Val;
        bashCooldown = data.S2_Cool;
    }

    protected override void Update()
    {
        base.Update();
        TryTaunt();
        TryShieldBash();
    }

    //------------도발이 가능한가?--------------
    void TryTaunt()
    {
        if (!canTaunt) return;

        if (!IsTauntConditionMet())
            return;

        canTaunt = false;            
        StartCoroutine(Taunt());
    }
       bool IsTauntConditionMet()
    {
        Collider[] aroundPlayer = Physics.OverlapSphere(
            player.position,
            attackDetectRadius,
            enemyLayer
        );

        return aroundPlayer.Length >= 3;
    }

    // -------------도발--------------
    IEnumerator Taunt()
    {
        canTaunt = false;

        ShowTauntSpeech("내 뒤로 숨게!\n이놈들은 내가 맡지!", 3f);

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
                enemy.ForceTarget(transform, tauntDuration);
                enemy.ShowTauntMark(tauntDuration);
            }
        }

        yield return new WaitForSeconds(tauntCooldown);
        canTaunt = true;
    }

    //도발 메세지 생성 위치
    public void ShowTauntSpeech(string message, float duration)
    {
        Vector3 pos = transform.position + Vector3.up * 2f;
        GameObject bubble = Instantiate(speechBubblePrefab, pos, Quaternion.identity);

        bubble.transform.SetParent(transform);

        var tmp = bubble.GetComponentInChildren<TMPro.TextMeshPro>();

        StartCoroutine(TypeText(tmp, message, 0.04f));

        StartCoroutine(HideSpeech(bubble, duration));
    }

    //대화 숨기기
    IEnumerator HideSpeech(GameObject bubble, float t)
    {
        yield return new WaitForSeconds(t);
        Destroy(bubble);
    }


    //타이핑효과
    IEnumerator TypeText(TMPro.TextMeshPro text, string message, float speed)
    {
        text.text = "";

        foreach (char c in message)
        {
            text.text += c;
            yield return new WaitForSeconds(speed);
        }
    }

    //방패치기
    void TryShieldBash()
    {
        if (!canBash) return;

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            bashRadius,
            enemyLayer
        );

        if (hits.Length == 0) return;

        Enemy target = hits[0].GetComponent<Enemy>();
        if (target != null)
        {
            canBash = false; 
            Debug.Log($"[Knight] Shield Bash HIT → {target.name}");
            StartCoroutine(ShieldBash(target));
        }
    }


    //방패치기 
    IEnumerator ShieldBash(Enemy target)
    {
        Debug.Log("[Knight] Shield Bash START");

        yield return new WaitForSeconds(0.4f);

        target.ApplyStun(bashStunTime);

        yield return new WaitForSeconds(bashCooldown);
        canBash = true;

        Debug.Log("[Knight] Shield Bash Cooldown END");
    }


    //기즈모 보여주깅
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, tauntRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, bashRadius);
    }

    protected override void SetWalking(bool walking) { }
}

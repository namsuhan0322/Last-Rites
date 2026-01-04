using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : AIBase
{
    [Header("도발 스킬")]
    public float tauntRadius = 6f;
    public float tauntCooldown = 20f;
    public GameObject speechBubblePrefab;   
    Transform speechPoint;


    bool canTaunt = true;

    protected override void Update()
    {
        base.Update();
        TryTaunt();
    }

    //------------도발이 가능한가?--------------
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
                enemy.ForceTarget(transform, Mathf.Infinity);
                enemy.ShowTauntMark(3f);   // ← 아까 만들었던 느낌표
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

    protected override void SetWalking(bool walking) { }
}

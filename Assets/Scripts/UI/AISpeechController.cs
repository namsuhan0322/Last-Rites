using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AISpeechController : MonoBehaviour   // 이건 그냥 텍스트 채팅용 스크립트 함수
{
    [Header("Speech")]
    [SerializeField] GameObject speechBubblePrefab;
    [SerializeField] Vector3 offset = new Vector3(0, 2f, 0);
    [SerializeField] float typingSpeed = 0.04f;

    GameObject currentBubble;
    Coroutine typingCoroutine;
    Coroutine hideCoroutine;

    // 외부에서 호출
    public void Speak(string message, float duration)
    {
        Clear();

        Vector3 pos = transform.position + offset;
        currentBubble = Instantiate(speechBubblePrefab, pos, Quaternion.identity);
        currentBubble.transform.SetParent(transform);

        var tmp = currentBubble.GetComponentInChildren<TextMeshPro>();
        typingCoroutine = StartCoroutine(TypeText(tmp, message));

        hideCoroutine = StartCoroutine(HideAfter(duration));
    }

    IEnumerator TypeText(TextMeshPro text, string message)
    {
        text.text = "";

        foreach (char c in message)
        {
            text.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    IEnumerator HideAfter(float time)
    {
        yield return new WaitForSeconds(time);
        Clear();
    }

    public void Clear()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        if (currentBubble != null)
            Destroy(currentBubble);

        typingCoroutine = null;
        hideCoroutine = null;
        currentBubble = null;
    }
}

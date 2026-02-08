using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class MenuButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Target Object")]
    public GameObject selectedLine; // 켜질 밑줄 오브젝트

    [Header("Animation Settings")]
    public float blinkSpeed = 4f;   // 깜빡이는 속도 (높을수록 빠름)
    public float minAlpha = 0.2f;   // 가장 투명할 때의 알파값 (0~1)
    public float maxAlpha = 1.0f;   // 가장 진할 때의 알파값 (0~1)

    private Image _lineImage;       // 이미지 컴포넌트 캐싱
    private Coroutine _blinkCoroutine;

    private void Awake()
    {
        if (selectedLine == null)
        {
            Transform find = transform.Find("Selected_line");
            if (find != null) selectedLine = find.gameObject;
        }

        if (selectedLine != null)
        {
            _lineImage = selectedLine.GetComponent<Image>();
            selectedLine.SetActive(false);
        }
    }

    // 마우스가 들어왔을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (selectedLine != null)
        {
            selectedLine.SetActive(true);

            if (_blinkCoroutine != null) StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = StartCoroutine(BlinkEffect());
        }
    }

    // 마우스가 나갔을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        if (selectedLine != null)
        {
            if (_blinkCoroutine != null) StopCoroutine(_blinkCoroutine);
            ResetAlpha();
            selectedLine.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (selectedLine != null)
        {
            ResetAlpha();
            selectedLine.SetActive(false);
        }
    }

    IEnumerator BlinkEffect()
    {
        float time = 0f;

        while (true)
        {
            time += Time.unscaledDeltaTime * blinkSpeed;

            float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(time) + 1f) * 0.5f);

            if (_lineImage != null)
            {
                Color color = _lineImage.color;
                color.a = alpha;
                _lineImage.color = color;
            }

            yield return null;
        }
    }

    void ResetAlpha()
    {
        if (_lineImage != null)
        {
            Color color = _lineImage.color;
            color.a = maxAlpha;
            _lineImage.color = color;
        }
    }
}
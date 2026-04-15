using UnityEngine;
using TMPro;
using System.Collections;

public class DamageTextEffect : MonoBehaviour
{
    [Header("이동 세팅")]
    [SerializeField] private float minUpwardSpeed = 60f;
    [SerializeField] private float maxUpwardSpeed = 100f;
    private float _finalUpSpeed;

    [Header("수명 세팅")]
    [SerializeField] private float lifeTime = 1.0f;

    private TextMeshProUGUI _textMesh;
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private float _timer = 0f;

    private void Start()
    {
        _textMesh = GetComponent<TextMeshProUGUI>();
        _rectTransform = GetComponent<RectTransform>();

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        _finalUpSpeed = Random.Range(minUpwardSpeed, maxUpwardSpeed);

        if (_rectTransform != null)
        {
            _rectTransform.rotation = Quaternion.Euler(0, 0, Random.Range(-4f, 4f));
        }

        Destroy(gameObject, lifeTime);

        StartCoroutine(PunchScale(1.3f));
    }

    private void Update()
    {
        if (_rectTransform == null) return;

        _rectTransform.position += Vector3.up * _finalUpSpeed * Time.deltaTime;

        _timer += Time.deltaTime;
        float halfLife = lifeTime * 0.5f;

        if (_timer >= halfLife)
        {
            float alphaProgress = (_timer - halfLife) / halfLife;
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, alphaProgress);
            }
        }
    }

    private IEnumerator PunchScale(float intensity)
    {
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = originalScale * intensity;

        float duration = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
    }
}
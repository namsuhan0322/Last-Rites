using UnityEngine;
using TMPro;

public class DamageEffectManager : SingletonMono<DamageEffectManager>
{
    protected override bool DontDestroy => false;

    [SerializeField] private GameObject textPrefab;
    [SerializeField] private Canvas uiCanvas;

    [SerializeField] private Transform damageTextContainer;

    [Header("기본 데미지 텍스트 색상")]
    public Color damageColor = Color.white;

    protected override void Awake()
    {
        base.Awake();

        if (uiCanvas == null)
        {
            uiCanvas = FindObjectOfType<Canvas>();
            if (uiCanvas == null)
            {
                Debug.LogError("UI 캔버스를 찾을 수 없습니다.");
            }
        }

        // 컨테이너를 깜빡하고 안 넣었다면 임시로 캔버스를 부모로 사용
        if (damageTextContainer == null && uiCanvas != null)
        {
            damageTextContainer = uiCanvas.transform;
        }
    }

    public void ShowDamage(Vector3 worldPosition, int amount)
    {
        ShowDamage(worldPosition, amount, damageColor);
    }

    public void ShowDamage(Vector3 worldPosition, int amount, Color customColor)
    {
        if (textPrefab == null || uiCanvas == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        if (screenPos.z < 0) return;

        GameObject damageText = Instantiate(textPrefab, damageTextContainer);
        RectTransform rect = damageText.GetComponent<RectTransform>();

        if (rect != null)
        {
            float randomOffsetX = Random.Range(-25f, 25f);
            float randomOffsetY = Random.Range(-10f, 10f);
            rect.position = screenPos + new Vector3(randomOffsetX, randomOffsetY, 0);
        }

        TextMeshProUGUI tmp = damageText.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = amount.ToString();
            tmp.color = customColor;
        }
    }
}
using UnityEngine;
using TMPro;

public class DamageEffectManager : SingletonMono<DamageEffectManager>
{
    protected override bool DontDestroy => true;

    [SerializeField] private GameObject textPrefab;
    [SerializeField] private Canvas uiCanvas;

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
    }

    // 외부에서 부를 때는 위치와 데미지 양만 넘겨주면 끝납니다!
    public void ShowDamage(Vector3 worldPosition, int amount)
    {
        if (textPrefab == null || uiCanvas == null) return;

        // 몬스터의 3D 월드 좌표를 2D 화면 UI 좌표로 변환
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);

        // 카메라 뒤에 있는 적을 때렸다면 텍스트를 띄우지 않음
        if (screenPos.z < 0) return;

        GameObject damageText = Instantiate(textPrefab, uiCanvas.transform);
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
            tmp.color = damageColor;
        }
    }
}
using System.Collections;
using UnityEngine;

public class HitFlashEffect : MonoBehaviour
{
    [Header("플래시 세팅")]
    [Tooltip("맞았을 때 번쩍일 색상 (예: 빨간색, 노란색 등)")]
    [ColorUsage(true, true)]
    public Color flashColor = Color.white;
    [Tooltip("평상시 원래 색상 (기본: 흰색)")]
    [ColorUsage(true, true)]
    public Color originalColor = Color.white;

    public float maxFlashAmount = 0.7f;
    public float flashDuration = 0.1f;

    [Header("Piloto 쉐이더 프로퍼티 이름")]
    public string colorPropertyName = "_BaseColorTint";
    public string amountPropertyName = "_Self_Ilumination_Intensity";

    private Renderer[] _renderers;
    private MaterialPropertyBlock _propBlock;
    private int _colorID;
    private int _amountID;
    private Coroutine _flashCoroutine;

    private void Awake()
    {
        InitializeIfNeeded();
    }

    private void InitializeIfNeeded()
    {
        if (_propBlock != null) return;

        _renderers = GetComponentsInChildren<Renderer>(true);
        _propBlock = new MaterialPropertyBlock();
        _colorID = Shader.PropertyToID(colorPropertyName);
        _amountID = Shader.PropertyToID(amountPropertyName);
    }

    public void PlayFlash()
    {
        InitializeIfNeeded();

        if (!gameObject.activeInHierarchy) return;

        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        SetFlash(maxFlashAmount, flashColor);

        yield return new WaitForSeconds(flashDuration);

        SetFlash(0f, originalColor);
    }

    private void SetFlash(float amount, Color color)
    {
        foreach (var r in _renderers)
        {
            if (r == null) continue;

            r.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(_colorID, color);
            _propBlock.SetFloat(_amountID, amount);
            r.SetPropertyBlock(_propBlock);
        }
    }
}
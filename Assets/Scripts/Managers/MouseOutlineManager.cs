using UnityEngine;
using System.Collections.Generic;

public class MouseOutlineManager : MonoBehaviour
{
    [Header("감지 설정")]
    [Tooltip("마우스가 감지할 레이어")]
    public LayerMask TargetLayer;

    [Header("아웃라인 에셋 설정")]
    [Tooltip("에셋 설정에 맞춰둔 아웃라인 전용 레이어 이름")]
    public string OutlineLayerName = "Outline";

    private int _outlineLayerIndex;
    private int _combinedLayerMask;

    private GameObject _currentTarget;
    private Dictionary<GameObject, int> _originalLayers = new Dictionary<GameObject, int>();

    private void Start()
    {
        _outlineLayerIndex = LayerMask.NameToLayer(OutlineLayerName);

        if (_outlineLayerIndex == -1)
        {
            Debug.LogError($"[경고] '{OutlineLayerName}' 이라는 레이어가 없습니다!");
            return;
        }

        _combinedLayerMask = TargetLayer | (1 << _outlineLayerIndex);
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, _combinedLayerMask))
        {
            GameObject hitObject = hit.collider.transform.root.gameObject;

            if (_currentTarget != hitObject)
            {
                RemoveOutline();
                _currentTarget = hitObject;
                ApplyOutline(_currentTarget);
            }
        }
        else
        {
            RemoveOutline();
        }
    }

    private void ApplyOutline(GameObject target)
    {
        _originalLayers.Clear();
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            if (r is ParticleSystemRenderer) continue;

            // 이미 아웃라인 레이어라면 무시
            if (r.gameObject.layer == _outlineLayerIndex) continue;

            // 원래 레이어 번호 저장 후 아웃라인 레이어로 변경
            _originalLayers.Add(r.gameObject, r.gameObject.layer);
            r.gameObject.layer = _outlineLayerIndex;
        }
    }

    private void RemoveOutline()
    {
        if (_currentTarget == null) return;

        foreach (var kvp in _originalLayers)
        {
            if (kvp.Key != null)
            {
                kvp.Key.layer = kvp.Value; // 원래 레이어로 되돌림
            }
        }

        _originalLayers.Clear();
        _currentTarget = null;
    }
}
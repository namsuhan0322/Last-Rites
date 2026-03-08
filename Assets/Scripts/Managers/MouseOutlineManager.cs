using UnityEngine;
using System.Collections.Generic;

public class MouseOutlineManager : MonoBehaviour
{
    [Header("적 아웃라인 설정")]
    public LayerMask EnemyLayer;
    public string EnemyOutlineLayerName = "Outline";

    [Header("파괴 기믹 아웃라인 설정")]
    public LayerMask BreakableLayer;
    public string BreakableOutlineLayerName = "Outline_Breakable";

    private int _enemyOutlineIndex;
    private int _breakableOutlineIndex;
    private int _combinedLayerMask;

    private GameObject _currentTarget;
    private Dictionary<GameObject, int> _originalLayers = new Dictionary<GameObject, int>();

    private void Start()
    {
        _enemyOutlineIndex = LayerMask.NameToLayer(EnemyOutlineLayerName);
        _breakableOutlineIndex = LayerMask.NameToLayer(BreakableOutlineLayerName);

        if (_enemyOutlineIndex == -1 || _breakableOutlineIndex == -1)
        {
            Debug.LogError("[경고] 아웃라인 전용 레이어 이름이 정확한지 확인해주세요!");
            return;
        }

        _combinedLayerMask = EnemyLayer | BreakableLayer | (1 << _enemyOutlineIndex) | (1 << _breakableOutlineIndex);
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

        // 현재 마우스가 올라간 타겟이 적군인지, 파괴물인지 판단해서 알맞은 아웃라인 레이어를 고릅니다.
        int targetOutlineIndex = _enemyOutlineIndex; // 기본값은 적군 아웃라인

        // 대상의 원래 레이어가 파괴물 레이어에 포함되어 있거나, 이미 파괴물 아웃라인 상태라면
        if ((BreakableLayer.value & (1 << target.layer)) > 0 || target.layer == _breakableOutlineIndex)
        {
            targetOutlineIndex = _breakableOutlineIndex;
        }

        foreach (Renderer r in renderers)
        {
            if (r is ParticleSystemRenderer) continue;
            if (r.gameObject.layer == targetOutlineIndex) continue;

            // 원래 레이어 번호 저장 후 아웃라인 레이어로 변경
            _originalLayers.Add(r.gameObject, r.gameObject.layer);
            r.gameObject.layer = targetOutlineIndex;
        }
    }

    private void RemoveOutline()
    {
        if (_currentTarget == null) return;

        foreach (var kvp in _originalLayers)
        {
            if (kvp.Key != null)
            {
                kvp.Key.layer = kvp.Value;
            }
        }

        _originalLayers.Clear();
        _currentTarget = null;
    }
}
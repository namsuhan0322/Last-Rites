using UnityEngine;
using System.Collections.Generic;

public class MouseOutlineManager : MonoBehaviour
{
    [Header("적 아웃라인 설정")]
    public LayerMask EnemyLayer;
    public string EnemyOutlineLayerName = "Outline";

    [Header("상점/정비소(상호작용) 아웃라인 설정")]
    public LayerMask InteractableLayer;
    public string InteractableOutlineLayerName = "Outline_Interactable";

    private int _enemyOutlineIndex;
    private int _interactableOutlineIndex;
    private int _combinedLayerMask;

    private GameObject _currentTarget;
    private Dictionary<GameObject, int> _originalLayers = new Dictionary<GameObject, int>();

    private void Start()
    {
        _enemyOutlineIndex = LayerMask.NameToLayer(EnemyOutlineLayerName);
        _interactableOutlineIndex = LayerMask.NameToLayer(InteractableOutlineLayerName);

        if (_enemyOutlineIndex == -1 || _interactableOutlineIndex == -1) return;

        _combinedLayerMask = EnemyLayer | InteractableLayer | (1 << _enemyOutlineIndex) | (1 << _interactableOutlineIndex);
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, _combinedLayerMask))
        {
            GameObject hitObject = FindSafeTargetRoot(hit.collider.gameObject);

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

    private GameObject FindSafeTargetRoot(GameObject startObj)
    {
        Transform current = startObj.transform;
        Transform safeRoot = current;

        while (current.parent != null)
        {
            int parentLayer = current.parent.gameObject.layer;

            bool isEnemy = (EnemyLayer.value & (1 << parentLayer)) != 0;
            bool isInteractable = (InteractableLayer.value & (1 << parentLayer)) != 0;
            bool isOutline = (parentLayer == _enemyOutlineIndex || parentLayer == _interactableOutlineIndex);

            if (isEnemy || isInteractable || isOutline)
            {
                current = current.parent;
                safeRoot = current;
            }
            else
            {
                break;
            }
        }

        return safeRoot.gameObject;
    }

    private void ApplyOutline(GameObject target)
    {
        _originalLayers.Clear();
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

        int targetOutlineIndex = _enemyOutlineIndex;

        if ((InteractableLayer.value & (1 << target.layer)) > 0 || target.layer == _interactableOutlineIndex)
        {
            targetOutlineIndex = _interactableOutlineIndex;
        }

        foreach (Renderer r in renderers)
        {
            if (r is ParticleSystemRenderer) continue;
            if (r.gameObject.layer == targetOutlineIndex) continue;

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
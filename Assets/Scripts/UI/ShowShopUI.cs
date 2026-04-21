using UnityEngine;
using System.Collections.Generic;

public class ShowShopUI : MonoBehaviour
{
    [Header("UI 및 프롬프트 설정")]
    [Tooltip("실제 화면에 띄울 상점 UI 캔버스 패널")]
    public GameObject shopUI;

    [Tooltip("미리 배치해둔 3D 'F' 버튼 오브젝트")]
    public GameObject interactionPrompt;

    [Header("자동 아웃라인 설정")]
    [Tooltip("아웃라인을 적용할 실제 3D 모델 (여기에 오브젝트를 넣으세요!)")]
    public GameObject targetOutlineObject;

    [Tooltip("상점/정비소에 씌울 아웃라인 레이어 이름")]
    public string outlineLayerName = "Outline_Interactable";

    private int _outlineLayerIndex;
    private Dictionary<GameObject, int> _originalLayers = new Dictionary<GameObject, int>();

    private bool _isPlayerInRange = false;

    private EnhancementUIManager enhancementUIManager;

    private void Start()
    {
        if (shopUI != null) shopUI.SetActive(false);
        if (interactionPrompt != null) interactionPrompt.SetActive(false);

        _outlineLayerIndex = LayerMask.NameToLayer(outlineLayerName);
        if (_outlineLayerIndex == -1)
        {
            Debug.LogError($"[경고] '{outlineLayerName}' 레이어가 존재하지 않습니다!");
        }

        GameObject outlineTarget = targetOutlineObject != null ? targetOutlineObject : gameObject;

        Renderer[] renderers = outlineTarget.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            if (r is ParticleSystemRenderer) continue;
            _originalLayers.Add(r.gameObject, r.gameObject.layer);
        }

        if (enhancementUIManager == null)
        {
            enhancementUIManager = FindAnyObjectByType<EnhancementUIManager>();
        }
    }

    private void Update()
    {
        if (_isPlayerInRange)
        {
            if (!shopUI.activeSelf && Input.GetKeyDown(KeyCode.F))
            {
                OpenShop();
            }
        }

        if (shopUI != null && shopUI.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            if (enhancementUIManager.resultPanel.activeSelf) return;
            CloseShop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = true;

            if (!shopUI.activeSelf && interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }

            ApplyOutline();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = false;

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }

            RemoveOutline();
            CloseShop();
        }
    }

    private void OpenShop()
    {
        if (shopUI != null) shopUI.SetActive(true);

        if (interactionPrompt != null) interactionPrompt.SetActive(false);
    }

    private void CloseShop()
    {
        if (shopUI != null) shopUI.SetActive(false);

        if (_isPlayerInRange && interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
        }
    }

    private void ApplyOutline()
    {
        if (_outlineLayerIndex == -1) return;

        foreach (var kvp in _originalLayers)
        {
            if (kvp.Key != null)
            {
                kvp.Key.layer = _outlineLayerIndex;
            }
        }
    }

    private void RemoveOutline()
    {
        foreach (var kvp in _originalLayers)
        {
            if (kvp.Key != null)
            {
                kvp.Key.layer = kvp.Value;
            }
        }
    }
}
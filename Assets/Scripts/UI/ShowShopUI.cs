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

    private void Start()
    {
        // 1. 초기 UI 및 프롬프트 숨기기
        if (shopUI != null) shopUI.SetActive(false);
        if (interactionPrompt != null) interactionPrompt.SetActive(false);

        // 2. 아웃라인 레이어 인덱스 가져오기
        _outlineLayerIndex = LayerMask.NameToLayer(outlineLayerName);
        if (_outlineLayerIndex == -1)
        {
            Debug.LogError($"[경고] '{outlineLayerName}' 레이어가 존재하지 않습니다!");
        }

        // 3. 타겟 오브젝트 지정 (비워뒀다면 자기 자신을 타겟으로 잡음)
        GameObject outlineTarget = targetOutlineObject != null ? targetOutlineObject : gameObject;

        // 4. 타겟의 원래 머티리얼 레이어들을 미리 백업해둡니다.
        Renderer[] renderers = outlineTarget.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            if (r is ParticleSystemRenderer) continue;
            _originalLayers.Add(r.gameObject, r.gameObject.layer);
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
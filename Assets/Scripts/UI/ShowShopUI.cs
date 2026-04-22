using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(-50)]
public class ShowShopUI : MonoBehaviour
{
    [Header("UI 및 프롬프트 설정")]
    [Tooltip("실제 화면에 띄울 상점 UI 캔버스 패널")]
    public GameObject shopUI;

    [Tooltip("미리 배치해둔 3D 'F' 버튼 오브젝트")]
    public GameObject interactionPrompt;

    [Header("방어막 팝업 리스트")]
    [Tooltip("이 리스트에 넣은 팝업창(GameObject)이 하나라도 켜져 있으면, ESC를 눌러도 전체 창이 꺼지지 않습니다.")]
    public List<GameObject> popupsToBlockClose;

    [Header("자동 아웃라인 설정")]
    public GameObject targetOutlineObject;

    [Tooltip("상점/정비소에 씌울 아웃라인 레이어 이름")]
    public string outlineLayerName = "Outline_Interactable";

    private int _outlineLayerIndex;
    private Dictionary<GameObject, int> _originalLayers = new Dictionary<GameObject, int>();

    private bool _isPlayerInRange = false;

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
            foreach (GameObject popup in popupsToBlockClose)
            {
                // 단 하나라도 켜져 있는 팝업창을 발견했다면?
                if (popup != null && popup.activeSelf)
                {
                    return; // 상점 전체를 끄지 않고 무시! (방어 성공)
                }
            }

            // 모든 팝업이 꺼져있을 때만 상점을 닫습니다.
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

        if (GameManager.Instance != null) GameManager.Instance.isInteractUIOpen = true;
    }

    private void CloseShop()
    {
        if (shopUI != null) shopUI.SetActive(false);

        if (_isPlayerInRange && interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
        }

        if (GameManager.Instance != null) GameManager.Instance.isInteractUIOpen = false;
    }

    private void OnDisable()
    {
        // 안전장치
        if (GameManager.Instance != null) GameManager.Instance.isInteractUIOpen = false;
    }

    private void ApplyOutline()
    {
        if (_outlineLayerIndex == -1) return;

        foreach (var kvp in _originalLayers)
        {
            if (kvp.Key != null) kvp.Key.layer = _outlineLayerIndex;
        }
    }

    private void RemoveOutline()
    {
        foreach (var kvp in _originalLayers)
        {
            if (kvp.Key != null) kvp.Key.layer = kvp.Value;
        }
    }
}
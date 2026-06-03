using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class TowerFloorSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    public GameObject select_Bg;
    public GameObject unSelect_Bg;
    public Image floorIcon;
    public TextMeshProUGUI floorNameText;
    public GameObject rotateLight;

    [HideInInspector] public TowerFloorSO myData;

    private Action<TowerFloorSlot> onClick;

    public void Initialize(TowerFloorSO data, Action<TowerFloorSlot> callback)
    {
        myData = data;
        onClick = callback;

        if (floorNameText != null)
            floorNameText.text = data.floorName;

        RefreshState();
        SetSelectedState(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (myData == null)
        {
            Debug.LogWarning("TowerFloorSlot에 myData가 없습니다.");
            return;
        }

        if (TowerManager.Instance == null)
        {
            Debug.LogWarning("TowerManager.Instance가 없습니다. 로비씬에 TowerManager를 배치하세요.");
            return;
        }

        bool unlocked = TowerManager.Instance.IsFloorUnlocked(myData.floor);

        if (!unlocked)
            return;

        onClick?.Invoke(this);
    }

    public void RefreshState()
    {
        bool unlocked =
            TowerManager.Instance.IsFloorUnlocked(myData.floor);

        bool cleared =
            TowerManager.Instance.HighestClearedFloor >= myData.floor;

        CanvasGroup cg = GetComponent<CanvasGroup>();

        if (cg == null)
            cg = gameObject.AddComponent<CanvasGroup>();

        cg.alpha = unlocked ? 1f : 0.5f;

        if (floorIcon != null)
        {
            floorIcon.sprite = myData.clearIcon;

            if (cleared)
            {
                floorIcon.color = Color.white;
            }
            else
            {
                floorIcon.color = new Color(
                    0.35f,
                    0.35f,
                    0.35f,
                    1f);
            }
        }

        // 클리어한 층만 회전 이펙트 ON
        if (rotateLight != null)
        {
            rotateLight.SetActive(cleared);
        }
    }

    public void SetSelectedState(bool selected)
    {
        if (select_Bg != null)
            select_Bg.SetActive(selected);

        if (unSelect_Bg != null)
            unSelect_Bg.SetActive(!selected);
    }
}
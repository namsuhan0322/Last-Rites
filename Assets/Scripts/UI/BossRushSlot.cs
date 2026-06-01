using UnityEngine;
using UnityEngine.EventSystems;

public class BossRushSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("상태 오브젝트")]
    public GameObject activeObj;
    public GameObject inactiveObj;

    private System.Action onSlotClicked;

    public void Initialize(System.Action onClickCallback)
    {
        onSlotClicked = onClickCallback;
        SetSelectedState(false);
    }

    public void SetSelectedState(bool isSelected)
    {
        if (activeObj != null) activeObj.SetActive(isSelected);
        if (inactiveObj != null) inactiveObj.SetActive(!isSelected);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onSlotClicked?.Invoke();
    }
}
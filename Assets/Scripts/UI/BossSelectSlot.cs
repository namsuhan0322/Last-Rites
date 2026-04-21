using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class BossSelectSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    public GameObject Select_Bg;
    public GameObject UnSelect_Bg;
    public Image Boss_Icon;
    public TextMeshProUGUI BossNameText;

    public BossStageSO myData { get; private set; }
    private System.Action<BossSelectSlot> onSelectedCallback;

    public void Initialize(BossStageSO data, System.Action<BossSelectSlot> callback)
    {
        myData = data;
        onSelectedCallback = callback;

        if (Boss_Icon != null) Boss_Icon.sprite = data.bossIcon;
        if (BossNameText != null) BossNameText.text = data.bossName;

        SetSelectedState(false);
    }

    public void SetSelectedState(bool isSelected)
    {
        if (Select_Bg != null) Select_Bg.SetActive(isSelected);
        if (UnSelect_Bg != null) UnSelect_Bg.SetActive(!isSelected);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onSelectedCallback?.Invoke(this);
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerFloorItem : MonoBehaviour
{
    public int floor;

    [Header("UI")]
    public Button button;
    public TMP_Text floorText;
    public TMP_Text stateText;

    [Header("Visual")]
    public GameObject selectedGlow;
    public GameObject lockIcon;
    public GameObject clearedIcon;

    TowerLobbyUI owner;

    public void Init(TowerLobbyUI ui)
    {
        owner = ui;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => owner.SelectFloor(floor));
    }

    public void Refresh()
    {
        bool unlocked = TowerManager.Instance.IsFloorUnlocked(floor);
        bool cleared = TowerManager.Instance.HighestClearedFloor >= floor;

        floorText.text = $"{floor}층";

        if (cleared)
            stateText.text = "클리어";
        else if (unlocked)
            stateText.text = "입장 가능";
        else
            stateText.text = "잠김";

        button.interactable = unlocked;

        if (lockIcon != null)
            lockIcon.SetActive(!unlocked);

        if (clearedIcon != null)
            clearedIcon.SetActive(cleared);

        if (selectedGlow != null)
            selectedGlow.SetActive(false);
    }

    public void SetSelected(bool selected)
    {
        if (selectedGlow != null)
            selectedGlow.SetActive(selected);
    }
}

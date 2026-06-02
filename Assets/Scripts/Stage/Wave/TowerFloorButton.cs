using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerFloorButton : MonoBehaviour
{
    public int floor;

    public Button button;
    public GameObject lockIcon;
    public TMP_Text floorText;
    public TMP_Text stateText;

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        bool unlocked = TowerManager.Instance.IsFloorUnlocked(floor);
        bool cleared = TowerManager.Instance.HighestClearedFloor >= floor;

        button.interactable = unlocked;

        if (lockIcon != null)
            lockIcon.SetActive(!unlocked);

        if (floorText != null)
            floorText.text = $"{floor}층";

        if (stateText != null)
        {
            if (cleared)
                stateText.text = "클리어";
            else if (unlocked)
                stateText.text = "입장 가능";
            else
                stateText.text = "잠김";
        }
    }

    public void OnClickEnter()
    {
        if (!TowerManager.Instance.IsFloorUnlocked(floor))
            return;

        TowerManager.Instance.SelectFloor(floor);

        ScenesManager.Instance.LoadScene("TowerScene");
    }
}
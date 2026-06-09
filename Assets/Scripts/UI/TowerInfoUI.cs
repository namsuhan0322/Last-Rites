using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerInfoUI : MonoBehaviour
{
    [Header("Info")]
    public TMP_Text floorText;
    public Image rewardIcon;

    [Header("Buttons")]
    public Button nextFloorButton;
    public Button exitButton;

    [Header("Next Button Visual")]
    public Image nextSelectImage;
    public Image nextUnSelectImage;

    private Color selectOriginalColor;
    private Color unSelectOriginalColor;

    private Image nextButtonImage;
    private Color nextButtonOriginalColor;

    void Start()
    {
        if (nextSelectImage != null)
            selectOriginalColor = nextSelectImage.color;

        if (nextUnSelectImage != null)
            unSelectOriginalColor = nextUnSelectImage.color;

        DisableNextFloorButton();

        if (nextFloorButton != null)
            nextFloorButton.onClick.AddListener(GoNextFloor);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitToLobby);

        Refresh();
    }
    public void Refresh()
    {
        if (TowerManager.Instance == null)
            return;

        int floor = TowerManager.Instance.selectedFloor;

        if (floorText != null)
            floorText.text = $"{floor}Ãþ";

        TowerFloorSO data = TowerManager.Instance.GetSelectedFloorData();

        if (data == null)
        {
            Debug.LogWarning($"{floor}Ãþ SO ¾øÀ½");
            return;
        }

        if (rewardIcon != null)
        {
            rewardIcon.sprite = data.clearIcon;
            rewardIcon.color = Color.white;
        }
    }

    public void DisableNextFloorButton()
    {
        Debug.Log("¹öÆ° ºñÈ°¼ºÈ­");

        if (nextFloorButton != null)
            nextFloorButton.interactable = false;

        if (nextSelectImage != null)
            nextSelectImage.color = Color.gray;

        if (nextUnSelectImage != null)
            nextUnSelectImage.color = Color.gray;
    }

    public void EnableNextFloorButton()
    {
        if (nextFloorButton != null)
            nextFloorButton.interactable = true;

        if (nextSelectImage != null)
            nextSelectImage.color = selectOriginalColor;

        if (nextUnSelectImage != null)
            nextUnSelectImage.color = unSelectOriginalColor;
    }

    void GoNextFloor()
    {
        int nextFloor = TowerManager.Instance.selectedFloor + 1;

        if (!TowerManager.Instance.IsFloorUnlocked(nextFloor))
            return;

        TowerManager.Instance.SelectFloor(nextFloor);

        if (ScenesManager.Instance != null)
            ScenesManager.Instance.LoadScene("TowerScene");
    }

    void ExitToLobby()
    {
        if (ScenesManager.Instance != null)
            ScenesManager.Instance.LoadLobbyScene();
    }
}
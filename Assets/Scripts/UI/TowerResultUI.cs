using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TowerResultUI : MonoBehaviour
{

    public Button nextFloorButton;
    public Button exitButton;

    private void Start()
    {

        nextFloorButton.onClick.AddListener(OnNextFloor);
        exitButton.onClick.AddListener(OnExit);
    }

    void OnNextFloor()
    {
        int nextFloor =
            TowerManager.Instance.selectedFloor + 1;

        if (!TowerManager.Instance.IsFloorUnlocked(nextFloor))
            return;

        TowerManager.Instance.SelectFloor(nextFloor);

        if (ScenesManager.Instance != null)
            ScenesManager.Instance.LoadScene("TowerScene");
        else
            SceneManager.LoadScene("TowerScene");
    }

    void OnExit()
    {
        if (ScenesManager.Instance != null)
            ScenesManager.Instance.LoadLobbyScene();
        else
            SceneManager.LoadScene("LobbyScene");
    }
}

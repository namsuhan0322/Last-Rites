using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public void OnClickGameStart()
    {
        if (ScenesManager.Instance != null)
        {
            ScenesManager.Instance.LoadGameScene();
        }
    }

    public void OnClickQuit()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }
}
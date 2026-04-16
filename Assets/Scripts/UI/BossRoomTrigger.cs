using UnityEngine;

public class BossRoomTrigger : MonoBehaviour
{
    public BossHealthUI bossHealthUI;

    public GameObject doorObj;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) ActivateBossRoom();
    }

    public void ActivateBossRoom()
    {
        if (bossHealthUI != null) bossHealthUI.ShowBossUI();
        if (doorObj != null) doorObj.SetActive(true);

        gameObject.SetActive(false);
    }

    public void ResetRoom()
    {
        gameObject.SetActive(true);

        if (bossHealthUI != null) bossHealthUI.HideBossUI();
        if (doorObj != null) doorObj.SetActive(false);
    }
}
using UnityEngine;
using Cinemachine;

public class BossRoomTrigger : MonoBehaviour
{
    public BossHealthUI bossHealthUI;
    public GameObject doorObj;

    [Header("보스 재생성(초기화) 설정")]
    public GameObject bossPrefab;       // 프로젝트 창에 있는 보스 원본 프리팹
    public Transform bossSpawnPoint;    // 보스가 생성될 위치 (빈 오브젝트 권장)
    public GameObject currentBoss;      // 현재 맵에 살아있는 보스

    [Header("테마2 보스 카메라 설정")]
    public CinemachineVirtualCamera bossCamera;

    private PlayerController _player;

    private void Start()
    {
        if (currentBoss == null)
        {
            currentBoss = GameObject.FindWithTag("Boss");
        }

        if (bossCamera != null) bossCamera.Priority = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _player = other.GetComponent<PlayerController>();
            if (_player != null) _player.SetBossModeOutline(true);
            if (bossHealthUI != null) bossHealthUI.ShowBossUI();
            if (doorObj != null) doorObj.SetActive(true);
            if (bossCamera != null) bossCamera.Priority = 20;

            gameObject.SetActive(false);
        }
    }

    public void ResetRoom()
    {
        gameObject.SetActive(true);
        if (bossHealthUI != null) bossHealthUI.HideBossUI();
        if (doorObj != null) doorObj.SetActive(false);

        if (bossCamera != null) bossCamera.Priority = 0;
        if (_player != null) _player.SetBossModeOutline(false);
        if (currentBoss != null)
        {
            Destroy(currentBoss);
        }

        if (bossPrefab != null && bossSpawnPoint != null)
        {
            GameObject newBossObj = Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);
            currentBoss = newBossObj;

            if (bossHealthUI != null)
            {
                Actor newBossActor = newBossObj.GetComponent<Actor>();
                bossHealthUI.UpdateBossReference(newBossActor);
            }
        }
    }

    public void OnBossDefeated()
    {
        if (bossCamera != null) bossCamera.Priority = 0;
        if (bossHealthUI != null) bossHealthUI.HideBossUI();
        if (doorObj != null) doorObj.SetActive(false);
        if (_player != null) _player.SetBossModeOutline(false);
    }
}
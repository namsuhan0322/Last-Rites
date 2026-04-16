using UnityEngine;

public class BossRoomTrigger : MonoBehaviour
{
    public BossHealthUI bossHealthUI;
    public GameObject doorObj;

    [Header("보스 재생성(초기화) 설정")]
    public GameObject bossPrefab;       // 프로젝트 창에 있는 보스 원본 프리팹
    public Transform bossSpawnPoint;    // 보스가 생성될 위치 (빈 오브젝트 권장)
    public GameObject currentBoss;      // 현재 맵에 살아있는 보스

    private void Start()
    {
        // 처음 시작할 때 맵에 배치되어 있는 보스 연결 (안 되어있을 경우)
        if (currentBoss == null)
        {
            currentBoss = GameObject.FindWithTag("Boss"); // 보스에 Boss 태그가 없다면 직접 인스펙터에서 넣어주세요!
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (bossHealthUI != null) bossHealthUI.ShowBossUI();
            if (doorObj != null) doorObj.SetActive(true);

            gameObject.SetActive(false);
        }
    }

    // GameManager의 리스폰 루틴에서 호출됩니다.
    public void ResetRoom()
    {
        gameObject.SetActive(true);
        if (bossHealthUI != null) bossHealthUI.HideBossUI();
        if (doorObj != null) doorObj.SetActive(false);
        if (currentBoss != null)
        {
            Destroy(currentBoss); // 꼬여버린 낡은 보스 삭제
        }

        if (bossPrefab != null && bossSpawnPoint != null)
        {
            // 완전 새삥 보스 스폰!
            currentBoss = Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);

            // 새로 태어난 보스의 체력을 UI에 다시 연결해줍니다.
            if (bossHealthUI != null)
            {
                bossHealthUI.UpdateBossReference(currentBoss.GetComponent<Actor>());
            }
        }
    }
}
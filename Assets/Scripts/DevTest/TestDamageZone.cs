using UnityEngine;

/// <summary>
/// [역할] 테스트용: 닿은 플레이어(태그 Player)에게 1초에 한 번 5~44 사이 랜덤 데미지.
/// [이유] SuHanTestScene에서 쓰는 테스트 전용 스크립트라 DevTest 폴더로 옮겼다(GUID 그대로라 씬 연결 유지).
/// [근거] 명세 5장 D11, 결정 R19, R35
/// </summary>
public class TestDamageZone : MonoBehaviour
{
    [Header("데미지 설정")]
    [SerializeField] private int minDamage = 5;
    [SerializeField] private int maxDamage = 45;

    [Header("쿨타임 (연타 방지용)")]
    [SerializeField] private float hitCooldown = 1.0f;
    private float _lastHitTime;

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time < _lastHitTime + hitCooldown) return;

        if (other.CompareTag("Player"))
        {
            Actor targetActor = other.GetComponent<Actor>();

            if (targetActor != null && !targetActor.IsDead)
            {
                int damage = Random.Range(minDamage, maxDamage);

                Debug.Log($"[Test] 플레이어에게 {damage}의 데미지를 줍니다!");

                targetActor.TakeDamage(damage);

                _lastHitTime = Time.time;
            }
        }
    }
}
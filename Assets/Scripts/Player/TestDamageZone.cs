using UnityEngine;

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
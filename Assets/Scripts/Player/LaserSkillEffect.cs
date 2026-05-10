using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))] // 이 스크립트를 넣으면 BoxCollider가 자동으로 추가됩니다.
public class LaserSkillEffect : MonoBehaviour
{
    [Header("레이저 스킬 설정")]
    [Tooltip("레이저가 발사되고 몇 초 뒤에 데미지가 들어갈 것인가?")]
    public float damageDelay = 0.1f;
    [Tooltip("이펙트 유지 시간")]
    public float lifeTime = 1.5f;

    private int _damage;
    private LayerMask _enemyLayer;
    private BoxCollider _boxCollider;

    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();
        _boxCollider.isTrigger = true; // 물리 충돌 방지용 트리거 설정
    }

    public void Initialize(int damage, LayerMask enemyLayer)
    {
        _damage = damage;
        _enemyLayer = enemyLayer;

        StartCoroutine(DealDamageCoroutine());
        Destroy(gameObject, lifeTime);
    }

    private IEnumerator DealDamageCoroutine()
    {
        yield return new WaitForSeconds(damageDelay);

        // 1. BoxCollider의 중심점(월드 좌표) 계산
        Vector3 center = transform.TransformPoint(_boxCollider.center);

        // 2. BoxCollider의 절반 크기(Half Extents) 계산 + 오브젝트 스케일 반영
        Vector3 halfExtents = Vector3.Scale(_boxCollider.size, transform.lossyScale) * 0.5f;

        // 3. 사각형(Box) 형태의 광역 판정 실행!
        Collider[] hitEnemies = Physics.OverlapBox(center, halfExtents, transform.rotation, _enemyLayer);

        foreach (Collider enemyCollider in hitEnemies)
        {
            Actor enemy = enemyCollider.GetComponentInParent<Actor>();
            if (enemy != null && !enemy.IsDead)
            {
                enemy.TakeDamage(_damage);
            }
        }
    }

    // 유니티 에디터에서 레이저 판정 범위를 빨간색 박스로 직관적으로 볼 수 있게 해줍니다.
    private void OnDrawGizmosSelected()
    {
        if (_boxCollider == null) _boxCollider = GetComponent<BoxCollider>();
        if (_boxCollider == null) return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // 반투명한 빨간색
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.DrawCube(_boxCollider.center, _boxCollider.size);
    }
}
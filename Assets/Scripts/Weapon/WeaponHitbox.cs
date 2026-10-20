using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// [역할] 근접 무기(대검·창·쌍검)의 공격 판정. 판정 구간(켜기~끄기) 동안 매 프레임, 칼날 위 점들이
///        지난 프레임 위치 → 지금 위치로 쓸고 간 캡슐 안의 적을 찾아 한 번씩 데미지·타격음·타격 이펙트를 준다.
///        무기 박스 콜라이더는 칼날 모양(크기·중심)만 읽고 항상 꺼 둔다.
/// [이유] 물리 트리거 방식은 플레이어 몸이 적 트리거에 닿아도 맞았고(보스 뼈 콜라이더), 0.02초 물리 간격 사이로 칼끝이 건너뛸 수 있었다.
///        보스·약점은 한 번 휘두를 때 보스에게 한 번만 들어가게 한다(AttackHitResolver — 범위 공격·투사체와 같은 규칙). 적 레이어만 검사해 아군 AI는 맞지 않는다.
/// [근거] 공격 판정 명세 3장, 결정 H2~H6, H11 / 리팩토링 명세 3.5·결정 R15
/// </summary>
public class WeaponHitbox : MonoBehaviour
{
    [Header("칼날 모양으로 쓸 박스 (크기·중심만 읽고 판정에는 쓰지 않음, 항상 꺼 둠)")]
    [SerializeField] private BoxCollider _greatSwordCol; // 대검
    [SerializeField] private BoxCollider _spearCol;      // 창
    [SerializeField] private BoxCollider _dualBladeRCol; // 쌍검(우)
    [SerializeField] private BoxCollider _dualBladeLCol; // 쌍검(좌)

    private const int MaxHitsPerQuery = 32;            // 한 번 검사에서 받을 콜라이더 수 (보스 뼈 콜라이더 20~28개)
    private const float EffectTowardCamera = 0.5f;     // 타격 이펙트를 카메라 쪽으로 0.5m (예전과 같음)
    private const float DebugLineSeconds = 0.3f;       // 판정 선을 화면에 남겨 두는 시간 (에디터 기즈모)

    // 지금 무기의 칼날 (쌍검은 2개)
    private class Blade
    {
        public BoxCollider Box;
        public BladeLayout Layout;
        public Vector3[] Previous;   // 지난 프레임의 칼날 점 월드 위치
    }

    private readonly List<Blade> _blades = new List<Blade>();
    private readonly AttackHitResolver _resolver = new AttackHitResolver();   // 맞히기 규칙 (칼·범위·투사체 공통, H11)
    private readonly Collider[] _hitBuffer = new Collider[MaxHitsPerQuery];

    private int _damage;
    private LayerMask _targetLayer;
    private bool _isAttackActive = false;
    private WeaponType _currentWeaponType;

    public bool IsActive => _isAttackActive;              // 테스트 표시용
    public int HitCountThisSwing => _resolver.HitCount;   // 테스트 표시용: 이번 휘두르기에서 데미지를 준 수

    private void Awake()
    {
        DisableAll();
    }

    public void SetupColliders(WeaponType type)
    {
        _blades.Clear();
        DisableAll();

        _currentWeaponType = type;

        switch (type)
        {
            case WeaponType.GreatSword:
                AddBlade(_greatSwordCol);
                break;

            case WeaponType.DualBlade:
                AddBlade(_dualBladeRCol);
                AddBlade(_dualBladeLCol);
                break;

            case WeaponType.Spear:
                AddBlade(_spearCol);
                break;
        }
    }

    private void AddBlade(BoxCollider box)
    {
        if (box == null) return;
        Vector3 size = box.size;
        Vector3 center = box.center;
        BladeLayout layout = BladeLayout.FromBox(size.x, size.y, size.z, center.x, center.y, center.z);
        _blades.Add(new Blade { Box = box, Layout = layout, Previous = new Vector3[layout.Points.Length] });
    }

    // 공격 시 켜기: 이번 휘두르기 시작. 칼날 점 위치를 기억하고, 그 자리에 이미 겹친 적을 한 번 검사한다 (H4)
    public void EnableHitbox(int damage, LayerMask targetLayer)
    {
        _isAttackActive = true;
        _damage = damage;
        _targetLayer = targetLayer;
        _resolver.BeginAttack();

        foreach (Blade blade in _blades)
        {
            for (int i = 0; i < blade.Previous.Length; i++)
                blade.Previous[i] = WorldPoint(blade, i);
        }
        Sweep();
    }

    // 공격 종료 시 끄기
    public void DisableHitbox()
    {
        _isAttackActive = false;
    }

    // 애니메이션이 이번 프레임 자세를 잡은 뒤에 검사한다 (Animator는 Update와 LateUpdate 사이에 움직임)
    private void LateUpdate()
    {
        if (_isAttackActive) Sweep();
    }

    // 칼날 점마다 지난 위치 → 지금 위치 캡슐 안의 적 (트리거 포함). 지난 = 지금이면 그 자리 겹침 검사가 된다
    private void Sweep()
    {
        foreach (Blade blade in _blades)
        {
            float radius = WorldRadius(blade);
            for (int i = 0; i < blade.Previous.Length; i++)
            {
                Vector3 now = WorldPoint(blade, i);
                int count = Physics.OverlapCapsuleNonAlloc(blade.Previous[i], now, radius, _hitBuffer,
                    _targetLayer, QueryTriggerInteraction.Collide);
                for (int h = 0; h < count; h++) TryHit(_hitBuffer[h], now);

#if UNITY_EDITOR
                Debug.DrawLine(blade.Previous[i], now, Color.yellow, DebugLineSeconds);
#endif
                blade.Previous[i] = now;
            }
        }
    }

    private void TryHit(Collider other, Vector3 bladePoint)
    {
        Actor target = _resolver.TryHit(other, _damage);   // 대상 찾기·약점 묶기·보스 한 번 (H2, H11)
        if (target == null) return;

        PlayHitSound(target.transform.position);
        PlayHitEffect(other, bladePoint);
    }

    // 맞은 콜라이더에서 칼날 점에 가장 가까운 지점, 카메라 쪽으로 조금
    private static void PlayHitEffect(Collider other, Vector3 bladePoint)
    {
        Vector3 effectPos = other.ClosestPoint(bladePoint);
        if (Camera.main != null)
        {
            Vector3 dirToCamera = (Camera.main.transform.position - effectPos).normalized;
            effectPos += dirToCamera * EffectTowardCamera;
        }

        EffectManager.Instance.PlayEffect("Hit", effectPos, Quaternion.identity);
    }

    private static Vector3 WorldPoint(Blade blade, int index)
    {
        (float x, float y, float z) = blade.Layout.Points[index];
        return blade.Box.transform.TransformPoint(new Vector3(x, y, z));
    }

    // 로컬 반지름 × 칼날 축이 아닌 두 축의 크기 배율 중 큰 쪽
    private static float WorldRadius(Blade blade)
    {
        Vector3 s = blade.Box.transform.lossyScale;
        float a = Mathf.Abs(s.x), b = Mathf.Abs(s.y), c = Mathf.Abs(s.z);
        float scale = blade.Layout.Axis == 0 ? Mathf.Max(b, c)
                    : blade.Layout.Axis == 1 ? Mathf.Max(a, c)
                    : Mathf.Max(a, b);
        return blade.Layout.Radius * scale;
    }

    private void DisableAll()
    {
        if (_greatSwordCol) _greatSwordCol.enabled = false;
        if (_spearCol) _spearCol.enabled = false;
        if (_dualBladeRCol) _dualBladeRCol.enabled = false;
        if (_dualBladeLCol) _dualBladeLCol.enabled = false;
    }

    private void PlayHitSound(Vector3 hitPosition)
    {
        if (SFXManager.Instance == null) return;

        switch (_currentWeaponType)
        {
            case WeaponType.GreatSword:
                SFXManager.Instance.PlaySFX("GS_002", hitPosition);
                break;

            case WeaponType.DualBlade:
                SFXManager.Instance.PlaySFX("DB_002", hitPosition);
                break;

            case WeaponType.Spear:
                SFXManager.Instance.PlaySFX("SP_002", hitPosition);
                break;
        }
    }

    // 에디터 기즈모: 판정 구간 동안 칼날 구 (테스트 씬·실제 씬 모두, Gizmos를 켜면 보임)
    private void OnDrawGizmos()
    {
        if (!_isAttackActive) return;
        Gizmos.color = new Color(1f, 0.85f, 0f, 0.6f);
        foreach (Blade blade in _blades)
        {
            float radius = WorldRadius(blade);
            for (int i = 0; i < blade.Previous.Length; i++)
                Gizmos.DrawWireSphere(WorldPoint(blade, i), radius);
        }
    }
}

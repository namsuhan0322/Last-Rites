using UnityEngine;

/// <summary>
/// [역할] 테스트 씬의 연습용 보스: 시작할 때 자식 약점(진짜 WeakPoint)을 이 더미 몸통의 약점으로 연결하고,
///        약점 HP가 바뀔 때마다 Console에 `[DevTest][약점]`으로 남긴다.
/// [이유] "한 번 휘두를 때 보스에게 한 번만, 약점 HP는 깎임"(H2)을 실제 보스 없이 확인하기 위해.
/// [근거] 공격 판정 명세 3.5, 결정 H9
/// </summary>
[RequireComponent(typeof(DevTestDummyTarget))]
public class DevTestDummyBoss : MonoBehaviour
{
    [Tooltip("약점 HP. 테스트 중 부서지지 않도록 크게 잡는다.")]
    [SerializeField] private int _weakPointHP = 100000;

    private WeakPoint _weakPoint;
    private int _lastHP;

    private void Start()
    {
        _weakPoint = GetComponentInChildren<WeakPoint>(true);
        if (_weakPoint == null) return;

        _weakPoint.Init(_weakPointHP, GetComponent<DevTestDummyTarget>());
        _lastHP = _weakPoint.CurrentHP;
        _weakPoint.OnHPChanged += HandleWeakPointHP;
    }

    private void OnDestroy()
    {
        if (_weakPoint != null) _weakPoint.OnHPChanged -= HandleWeakPointHP;
    }

    private void HandleWeakPointHP(int current, int max)
    {
        Debug.Log($"[DevTest][약점] {Time.time:F2}초 {_weakPoint.name} HP {current}/{max} ({current - _lastHP})");
        _lastHP = current;
    }
}

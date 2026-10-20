using System.Collections.Generic;

/// <summary>
/// [역할] 새로 닿은 대상에 대한 판정 결과: 건너뜀 / 맞힘 / 약점만 맞힘(보스로 넘기지 않음).
/// [이유] 판정 결과를 이름으로 나눠, 보스·약점 규칙을 엔진 없이 테스트한다.
/// [근거] 공격 판정 명세 3.2, 결정 H2·H8
/// </summary>
public enum SwingHitResult
{
    Skip,
    Hit,
    HitWithoutForward
}

/// <summary>
/// [역할] 한 번 휘두르는 동안(켜기~끄기) 맞은 대상을 기억하고, 새로 닿은 대상마다 결과를 정한다.
///        보스 HP는 한 번 휘두를 때 한 번만 깎이고(몸통이든 약점 경유든 먼저 닿은 쪽), 닿은 약점은 모두 약점 HP가 깎인다.
/// [이유] 몸통과 약점을 함께 베면 보스가 두 번 맞던 문제(피드백, 명세 2.2 ①)를 막는다. 대상은 숫자 ID로만 다룬다.
/// [근거] 공격 판정 명세 3.2, 결정 H1·H2·H8
/// </summary>
public class SwingHitRegistry
{
    private readonly HashSet<int> _hitTargets = new HashSet<int>();      // 이번에 맞은 대상 (몸통·약점·일반 적)
    private readonly HashSet<int> _damagedOwners = new HashSet<int>();   // 이번에 데미지를 받은 주인 (보스·일반 적 자신)

    public void BeginSwing()
    {
        _hitTargets.Clear();
        _damagedOwners.Clear();
    }

    /// targetId: 닿은 대상. ownerId: 약점이면 주인 보스(없으면 targetId), 아니면 targetId
    public SwingHitResult Register(int targetId, int ownerId, bool isWeakPoint)
    {
        if (_hitTargets.Contains(targetId)) return SwingHitResult.Skip;
        _hitTargets.Add(targetId);

        if (isWeakPoint && ownerId != targetId)
        {
            // 약점: 보스가 이번에 이미 데미지를 받았으면 약점 HP만
            if (_damagedOwners.Contains(ownerId)) return SwingHitResult.HitWithoutForward;
            _damagedOwners.Add(ownerId);
            return SwingHitResult.Hit;
        }

        // 일반 적·보스 몸통(주인 없는 약점 포함): 약점 경유로 이미 받았으면 건너뜀
        if (_damagedOwners.Contains(targetId)) return SwingHitResult.Skip;
        _damagedOwners.Add(targetId);
        return SwingHitResult.Hit;
    }
}

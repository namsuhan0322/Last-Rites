using UnityEngine;

/// <summary>
/// [역할] 플레이어 공격 하나(칼 한 번 휘두르기, 범위 공격 한 번 터짐, 투사체 하나) 동안 맞은 콜라이더를 받아
///        대상마다 한 번, 약점이 있는 보스에게도 한 번만 데미지를 준다(약점 HP는 깎임). 데미지를 준 대상을 돌려준다.
/// [이유] 칼·범위 공격·투사체가 같은 규칙을 한 곳에서 쓰게 하려고. 범위 공격·투사체는 맞은 콜라이더 자신의 Actor만 찾고
///        같은 대상을 거르지 않아 늑대 보스(몸통+약점)는 두 번 맞고 드래곤(뼈 콜라이더에 Actor 없음) 몸통은 못 맞혔다.
///        판정 규칙은 엔진 밖에서 테스트하는 SwingHitRegistry가 정하고, 이 클래스는 콜라이더 → Actor 연결과 데미지 주기만 한다.
/// [근거] 공격 판정 명세 3.2·3.6, 결정 H2, H10, H11
/// </summary>
public class AttackHitResolver
{
    private readonly SwingHitRegistry _registry = new SwingHitRegistry();

    public int HitCount { get; private set; }   // 이번 공격에서 데미지를 준 수

    // 공격 하나 시작: 맞힌 기록을 비운다
    public void BeginAttack()
    {
        _registry.BeginSwing();
        HitCount = 0;
    }

    // 맞은 콜라이더 하나를 처리한다. 데미지를 준 대상을 돌려주고, 건너뛰면 null
    public Actor TryHit(Collider other, int damage)
    {
        Actor target = other.GetComponentInParent<Actor>();   // 보스 뼈 콜라이더처럼 Actor가 부모에 있어도 찾는다 (Q9)
        if (target == null || target.IsDead) return null;
        if (target is PlayerStats) return null;   // 플레이어 자신 (적 레이어만 검사하지만 한 번 더 막는다)

        // 약점이면 주인 보스와 묶어 판정 (H2). 주인이 없으면 일반 적처럼
        WeakPoint weakPoint = target as WeakPoint;
        Actor owner = weakPoint != null && weakPoint.Owner != null ? weakPoint.Owner : target;

        SwingHitResult result = _registry.Register(target.GetInstanceID(), owner.GetInstanceID(), weakPoint != null);
        if (result == SwingHitResult.Skip) return null;

        if (result == SwingHitResult.HitWithoutForward) weakPoint.TakeDamageWithoutForward(damage);
        else target.TakeDamage(damage);

        HitCount++;
        return target;
    }
}

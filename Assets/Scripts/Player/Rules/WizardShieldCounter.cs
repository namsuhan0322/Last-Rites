/// <summary>
/// [역할] 마법사 E 보호막의 남은 방어 횟수.
/// [이유] 보호막 판정을 엔진 없이 테스트하기 위해. 보호막 오브젝트 생성·제거와 로그는 WizardCaster(부품)가 한다.
/// [근거] 명세 3.2 WizardShieldCounter, 이상한 동작 Q4 (0 데미지도 소모, 부활·무기 교체로 꺼지지 않음 — 유지)
/// </summary>
public class WizardShieldCounter
{
    public bool IsActive { get; private set; }
    public int RemainingHits { get; private set; }

    public void Activate(int maxHits)
    {
        IsActive = true;
        RemainingHits = maxHits;
    }

    /// 켜져 있으면 1회 소모하고 true. 데미지 양은 보지 않는다.
    /// 0이 되어도 여기서 끄지 않는다 — 깨짐 처리(로그·오브젝트 제거)는 부르는 쪽이 Break()로 한다.
    public bool TryConsumeHit()
    {
        if (!IsActive) return false;
        RemainingHits--;
        return true;
    }

    public void Break() => IsActive = false;
}

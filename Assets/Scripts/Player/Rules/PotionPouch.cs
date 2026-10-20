/// <summary>
/// [역할] 포션 개수, 포션 쿨타임, 사용 조건, 회복량.
/// [이유] PlayerController.UsePotion 의 규칙을 엔진 없이 테스트하기 위해.
/// [근거] 명세 3.2 PotionPouch
/// </summary>
public class PotionPouch
{
    public int Count { get; private set; }
    public float CooldownRemaining { get; private set; }

    /// 조건: 포션 데이터 있음 + 1개 이상 + HP가 가득 차지 않음 + 쿨 끝 (지금 코드의 검사 4개와 같음)
    public bool CanUse(bool hasPotionData, bool isFullHP)
        => hasPotionData && Count > 0 && !isFullHP && CooldownRemaining <= 0f;

    /// 회복량 = round(최대HP × 회복%/100). 계산 순서는 지금 코드와 같다 (int/100f → int×float).
    public static int HealAmount(int maxHP, int healPercent)
    {
        float ratio = healPercent / 100f;
        return RuleMath.RoundToInt(maxHP * ratio);
    }

    public void TakeOne() => Count--;
    public void StartCooldown(float cooldown) => CooldownRemaining = cooldown;
    public void Refill(int maxCount) => Count = maxCount;
    public void ResetCooldown() => CooldownRemaining = 0f;

    /// 쿨을 줄이고, 이번에 끝났으면 true (예전 "포션 쿨타임 끝" 로그용, 로그는 3단계에서 지움 R34). 0보다 클 때만 줄인다.
    public bool Tick(float dt)
    {
        if (CooldownRemaining <= 0) return false;
        CooldownRemaining -= dt;
        return CooldownRemaining <= 0;
    }
}

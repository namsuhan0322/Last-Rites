/// <summary>
/// [역할] 전투 상태 ON/OFF. 적을 보면 ON + 타이머를 채우고, 못 보면 타이머를 줄이다가 0 이하가 된 다음 번에 OFF.
/// [이유] PlayerController.CheckEnemyInSight 의 판정 부분을 엔진 없이 테스트하기 위해 (감지 자체는 2b에서 분리).
/// [근거] 명세 3.2 CombatTimer
/// </summary>
public class CombatTimer
{
    public bool InCombat { get; private set; }
    public float Remaining { get; private set; }

    public void Update(bool enemyFound, float combatCooldown, float dt)
    {
        if (enemyFound)
        {
            InCombat = true;
            Remaining = combatCooldown;
        }
        else if (Remaining > 0)
        {
            Remaining -= dt;    // 이번에 0 이하가 돼도 OFF는 다음 번 (지금 동작)
        }
        else
        {
            InCombat = false;
        }
    }

    /// 튜토리얼 전투 잠금: 즉시 OFF, 타이머도 0
    public void ForceOff()
    {
        InCombat = false;
        Remaining = 0f;
    }

    /// 부활: OFF만 하고 타이머는 그대로 둔다 (지금 Revive 동작)
    public void TurnOffKeepTimer() => InCombat = false;
}

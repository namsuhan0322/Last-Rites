/// <summary>
/// [역할] 이번 프레임에 할 연결: 없음 / 평타 시작(다음 타) / 선입력 스킬·회피 실행.
/// [이유] 상태 클래스가 판단 결과대로만 움직이게 해, 판단을 엔진 없이 테스트한다.
/// [근거] 조작감 명세 3.3, 결정 C9
/// </summary>
public enum ChainDecision
{
    None,
    StartAttack,
    RunBuffered
}

/// <summary>
/// [역할] 평타·스킬 상태에서 이번 프레임에 다음 행동으로 넘어갈지 판단한다 (조작감 규칙 표의 "평타"·"스킬" 줄).
/// [이유] 우선순위(누른 스킬·회피가 누르고 있는 평타보다 먼저)와 끊는 시점 규칙을 한곳에 모아 테스트한다.
/// [근거] 조작감 명세 3.2, 결정 C3·C4·C5·C7②·C9
/// </summary>
public static class ActionChainRule
{
    /// normalizedTime: 지금 애니 진행도(0~1)
    /// skillCancelFrom: 선입력 스킬·회피를 실행해도 되는 진행도 (평타 상태 0 = 언제든, 스킬 상태 0.4)
    /// attackChainAt: 평타로 넘어가는 진행도 (타격 끝. 이벤트가 없으면 상태가 정한 기본값)
    public static ChainDecision Decide(float normalizedTime, float skillCancelFrom, float attackChainAt,
        BufferedActionKind buffered, bool attackHeld)
    {
        // 누른 스킬·회피가 있으면 그것만 기다린다 (누르고 있는 평타보다 먼저, C7②)
        if (buffered == BufferedActionKind.Skill || buffered == BufferedActionKind.Dodge)
            return normalizedTime >= skillCancelFrom ? ChainDecision.RunBuffered : ChainDecision.None;

        if (buffered == BufferedActionKind.Attack || attackHeld)
            return normalizedTime >= attackChainAt ? ChainDecision.StartAttack : ChainDecision.None;

        return ChainDecision.None;
    }
}

/// <summary>
/// [역할] 튜토리얼 잠금 3종: 전체 정지(연출 중), 스킬 잠금, 전투 잠금. 튜토리얼이 없는 씬이면 모두 false.
/// [이유] 같은 null 검사·조건이 세 곳에 흩어져 있었다. 값은 부를 때마다 TutorialSystem에서 실시간으로 읽는다 —
///        한 프레임 안에서도 Q 사용 → SkillTutorial 알림 → 튜토리얼 값 변경이 일어날 수 있어서 (R23).
/// [근거] 명세 3.4, 결정 R23
/// </summary>
public class TutorialGate
{
    private readonly PlayerController _owner;

    public TutorialGate(PlayerController owner)
    {
        _owner = owner;
    }

    private TutorialSystem Tutorial => _owner.tutorialSystem;

    /// 튜토리얼 연출 중: 플레이어 전체 정지 (경로 초기화·이동값 0 후 그 프레임 처리 중단, 쿨타임도 멈춤)
    public bool IsFrozen => Tutorial != null && Tutorial.tutorialPlaying;

    /// 스킬 키(Q/W/E/R/V) 막힘
    public bool SkillsLocked => Tutorial != null && !Tutorial.canUseSkills;

    /// 전투 감지 꺼짐 (전투 OFF + 무기 넣기)
    public bool CombatLocked => Tutorial != null && !Tutorial.canPlayerCombat;
}

using UnityEngine;

/// <summary>
/// [역할] 플레이어 애니메이터에서 쓰는 이름을 한곳에 모은다: 파라미터(해시), 스킬 트리거 이름, 상태 이름·태그, 레이어 번호.
/// [이유] 같은 문자열이 상태 클래스·컨트롤러·부품 여기저기에 흩어져 오타가 나도 알 수 없었다.
/// [근거] 명세 3.4, 결정 R12 (파라미터는 해시 = 문자열 호출과 결과 같음 / 상태 이름은 IsName 비교 방식 때문에 문자열 그대로)
/// </summary>
public static class PlayerAnimParams
{
    // ── 파라미터 (SetFloat / SetBool / SetTrigger / ResetTrigger) ──
    public static readonly int Move = Animator.StringToHash("Move");
    public static readonly int AttackSpd = Animator.StringToHash("AttackSpd");
    public static readonly int InCombat = Animator.StringToHash("InCombat");
    public static readonly int HitPower = Animator.StringToHash("HitPower");
    public static readonly int Attack = Animator.StringToHash("Attack");
    public static readonly int Roll = Animator.StringToHash("Roll");
    public static readonly int IsHit = Animator.StringToHash("IsHit");
    public static readonly int IsStun = Animator.StringToHash("IsStun");
    public static readonly int IsDead = Animator.StringToHash("IsDead");

    // ── 스킬 트리거 이름 (CurrentSkillAnim에 문자열로 저장됨) ──
    public const string SkillQ = "Skill_Q";
    public const string SkillW = "Skill_W";
    public const string SkillE = "Skill_E";
    public const string SkillR = "Skill_R";
    public const string SkillV = "Skill_V";
    public static readonly string[] SkillTriggers = { SkillQ, SkillW, SkillE, SkillR, SkillV };

    // ── 상태 이름 (IsName / CrossFade / Play) ──
    public const string StateIdleMove = "Idle/Move";
    public const string StateRoll = "Roll";
    public const string StateAttack1 = "Attack1";
    public const string StateAttack2 = "Attack2";
    public const string StateAttack3 = "Attack3";
    public const string StateUpperEmpty = "Empty";             // 상체 레이어
    public const string StateUpperSheathe = "SheatheHips";     // 상체 레이어: 칼 넣기 애니
    public const string StateUpperUnsheathe = "UnsheatheHips"; // 상체 레이어: 칼 뽑기 애니
    public const string StateUpperCombat = "Combat (1)";       // 상체 레이어: 무기 든 자세

    // ── 태그 ──
    public const string TagAttack = "Attack";
    public const string TagSkill = "Skill";
    public const string TagRoll = "Roll";

    // ── 레이어 ──
    public const int BaseLayer = 0;
    public const int UpperBodyLayer = 1;
}

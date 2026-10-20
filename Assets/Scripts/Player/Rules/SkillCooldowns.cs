using System;

/// <summary>
/// [역할] 스킬 5칸, 회피, 공통 스킬 딜레이, 회피 후 공격 딜레이의 남은 시간을 관리한다.
/// [이유] PlayerController가 타이머마다 같은 감소·초기화 코드를 반복하던 것을 한 곳으로 모은다.
/// [근거] 명세 3.2 SkillCooldowns, 결정 R7. 포션 쿨타임은 PotionPouch가 따로 관리한다.
/// </summary>
public class SkillCooldowns
{
    private readonly float[] _skills = new float[5];    // SkillSlot 값(0~4)으로 접근

    public float Dash { get; private set; }
    public float GlobalSkill { get; set; }              // 스킬 상태에 들어갈 때 globalSkillDelay로 설정됨
    public float PostRollAttack { get; set; }           // 지금 코드에서는 0 말고 넣는 곳이 없음 (이상한 동작 Q3, 유지)

    public float Get(SkillSlot slot) => _skills[(int)slot];
    public bool IsReady(SkillSlot slot) => _skills[(int)slot] <= 0f;
    public void Start(SkillSlot slot, float duration) => _skills[(int)slot] = duration;
    public void StartDash(float duration) => Dash = duration;

    /// 회피·공통 딜레이·회피 후 딜레이를 줄인다. 0보다 클 때만 빼므로 0 아래로 살짝 내려갈 수 있다 (지금과 같음).
    public void TickGeneral(float dt)
    {
        if (Dash > 0) Dash -= dt;
        if (GlobalSkill > 0) GlobalSkill -= dt;
        if (PostRollAttack > 0) PostRollAttack -= dt;
    }

    /// 스킬 5칸을 Q→W→E→R→V 순서로 줄이고, 이번에 0 이하가 된 칸을 알린다 (알림을 받는 곳이 없으면 생략, R34).
    public void TickSkills(float dt, Action<SkillSlot> onFinished = null)
    {
        for (int i = 0; i < _skills.Length; i++)
        {
            if (_skills[i] > 0)
            {
                _skills[i] -= dt;
                if (_skills[i] <= 0) onFinished?.Invoke((SkillSlot)i);
            }
        }
    }

    /// 무기 교체: 스킬 5칸만 0 (지금 ChangeWeapon과 같음)
    public void ResetSkills() => Array.Clear(_skills, 0, _skills.Length);

    /// 부활: 스킬 5칸 + 회피 + 공통 딜레이 + 회피 후 딜레이 모두 0 (지금 Revive와 같음)
    public void ResetAll()
    {
        ResetSkills();
        Dash = 0f;
        GlobalSkill = 0f;
        PostRollAttack = 0f;
    }
}

using System.Collections;
using UnityEngine;

/// <summary>
/// [역할] 무기 R 스킬 버프의 부모. 무기 SO의 R_Skill_Logic에 넣으면 R 사용 시 Execute(R값)가 불린다.
/// [이유] 쌍검·창·검방 버프가 똑같이 쓰던 "지속 시간 기다렸다 끝내기" 코루틴을 부모에 하나 둔다.
/// [근거] 명세 3.5, 기획서 '무기별 특수 스킬(R) 및 버프' 1장(BuffSkill_SO 구조)
/// </summary>
public abstract class BuffSkill_SO : ScriptableObject
{
    [Header("버프 기본 정보")]
    public string skillName;
    public string description;

    // R 스킬 발동 시 호출될 함수 (무기 SO에서 R_Val 값을 넘겨받습니다)
    public abstract void Execute(PlayerController player, float rVal);

    // 버프 지속시간이 끝나거나 조건이 달성되어 종료될 때 호출할 함수
    public abstract void EndBuff(PlayerController player);

    /// 지속 시간만큼 기다린 뒤 EndBuff (쌍검·창·검방 공통. 대검은 HP 소모 반복이 있어 따로)
    protected IEnumerator RunForDuration(PlayerController player, float duration)
    {
        yield return new WaitForSeconds(duration);
        EndBuff(player);
    }
}

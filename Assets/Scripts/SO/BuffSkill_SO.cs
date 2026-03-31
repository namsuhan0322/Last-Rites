using UnityEngine;

public abstract class BuffSkill_SO : ScriptableObject
{
    [Header("버프 기본 정보")]
    public string skillName;
    public string description;

    // R 스킬 발동 시 호출될 함수 (무기 SO에서 R_Val 값을 넘겨받습니다)
    public abstract void Execute(PlayerController player, float rVal);

    // 버프 지속시간이 끝나거나 조건이 달성되어 종료될 때 호출할 함수
    public abstract void EndBuff(PlayerController player);
}
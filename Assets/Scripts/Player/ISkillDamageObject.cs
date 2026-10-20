using UnityEngine;

/// <summary>
/// [역할] 플레이어가 생성해서 데미지를 맡기는 오브젝트(범위 공격·레이저·블랙홀·창 투사체·마법탄)의 공통 약속.
/// [이유] 생성하는 쪽이 GetComponent를 스크립트 종류마다 차례로 시도하던 것을 한 번으로 줄인다.
///        U3 확인: 해당 프리팹 8개는 모두 이 스크립트가 하나씩만 붙어 있어 찾는 결과가 같다.
/// [근거] 명세 3.5, 결정 R16, R33
/// </summary>
public interface ISkillDamageObject
{
    void Initialize(int damage, LayerMask enemyLayer);
}

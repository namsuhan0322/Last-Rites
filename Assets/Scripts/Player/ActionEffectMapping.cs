using UnityEngine;

/// <summary>
/// [역할] 애니메이션 이벤트 암호(actionCode) 하나와 그때 켤 파티클 묶음 (인스펙터 목록의 한 줄).
/// [이유] EnableActionEffect("암호")가 무엇을 켤지 코드 대신 인스펙터에서 정하게 한다.
/// [근거] 명세 3.3 PlayerActionEffects
/// </summary>
[System.Serializable]
public class ActionEffectMapping
{
    [Tooltip("애니메이션 이벤트에 적을 암호")]
    public string actionCode;

    [Tooltip("동시에 켜질 파티클들 (W처럼 2개면 Size를 2로 늘려서 넣으세요)")]
    public ParticleSystem[] particles;
}
using UnityEngine;

[System.Serializable]
public class ActionEffectMapping
{
    [Tooltip("애니메이션 이벤트에 적을 암호")]
    public string actionCode;

    [Tooltip("동시에 켜질 파티클들 (W처럼 2개면 Size를 2로 늘려서 넣으세요)")]
    public ParticleSystem[] particles;
}
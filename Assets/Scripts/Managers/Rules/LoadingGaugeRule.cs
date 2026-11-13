using System;

/// <summary>
/// [역할] 로딩 게이지 규칙(엔진 없음): 실제 로딩 진행도 → 게이지(0~90%), 로딩이 끝난 뒤 연출(지금 위치 → 100%),
///        게이지가 내려가지 않게 막기, 가득 찼는지.
/// [이유] 예전에는 진행도를 0~99%로 바꾸고 연출은 90%에서 다시 시작해 게이지가 99% → 90%로 내려갔다(커밋 ddb73d81).
///        기준을 한 곳에 모으고 엔진 밖에서 테스트한다.
/// [근거] 로딩 게이지 명세 2.2·3.1, 결정 L1~L3
/// </summary>
public static class LoadingGaugeRule
{
    public const float LoadEnd = 0.9f;          // allowSceneActivation = false면 Unity 진행도가 여기서 멈춤
    public const float RealShare = 0.9f;        // 실제 로딩이 차지하는 게이지 몫 (0~90%, 결정 L1)
    public const float CreepSpeed = 0.05f;      // 연출 속도: 초당 5% → 90에서 100%까지 2초 (예전 속도 그대로, L3)
    public const float FullHoldSeconds = 0.3f;  // 100%를 보여 주는 시간: UI가 Lerp로 따라와 100% 글자까지 약 0.15초 (L3)

    /// 실제 진행도 → 게이지. 지금 게이지보다 작아지지 않음
    public static float FromLoad(float loadProgress, float current)
    {
        float ratio = Math.Max(0f, Math.Min(1f, loadProgress / LoadEnd));
        return Math.Max(current, ratio * RealShare);
    }

    /// 연출 한 걸음: 지금 위치에서 흐른 시간 × 속도만큼 올림, 1에서 멈춤
    public static float Creep(float current, float deltaTime) => Math.Min(1f, current + deltaTime * CreepSpeed);

    /// 게이지가 가득 찼는지
    public static bool IsFull(float gauge) => gauge >= 1f;
}

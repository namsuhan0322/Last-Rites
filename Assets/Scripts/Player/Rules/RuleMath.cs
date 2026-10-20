using System;

/// <summary>
/// [역할] 규칙 클래스들이 함께 쓰는 반올림.
/// [이유] 엔진 없이 테스트하려면 Mathf를 쓸 수 없다. Mathf.RoundToInt(f)는 내부에서 (int)Math.Round(f)라서
///        똑같이 쓰면 결과(짝수 쪽 반올림: 2.5 → 2)가 같다.
/// [근거] 결정 R8
/// </summary>
public static class RuleMath
{
    public static int RoundToInt(float value) => (int)Math.Round(value);
}

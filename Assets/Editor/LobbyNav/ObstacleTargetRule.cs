using System;
using System.Collections.Generic;

/// <summary>
/// [역할] 투명 벽 장애물 대상 규칙(엔진 없음): 상자가 서 있는지(벽) 기울어져 있는지(밟고 오르는 경사로) 판단한다.
/// [이유] 렌더러가 꺼진 상자에는 투명 벽뿐 아니라 계단 경사로(약 45° 기운 Cube)도 있어, 경사로까지 파내면 계단 NavMesh가 사라졌다.
///        상자는 어느 축을 세워도 같은 모양이라 세 축 중 하나라도 거의 수직이면 서 있는 상자로 본다.
/// [근거] 로비 이동 막힘 명세 3.3, 결정 N11
/// </summary>
public static class ObstacleTargetRule
{
    /// 세 축이 세계 위쪽과 이루는 각도(0~180°) 중 하나라도 maxTiltDegrees 안이면(뒤집힌 축은 180°에서 뺀 값) 서 있는 상자
    public static bool IsUpright(IReadOnlyList<float> axisAnglesToUp, float maxTiltDegrees)
    {
        for (int i = 0; i < axisAnglesToUp.Count; i++)
        {
            float angle = axisAnglesToUp[i];
            float tilt = Math.Min(angle, 180f - angle);
            if (tilt <= maxTiltDegrees) return true;
        }
        return false;
    }
}

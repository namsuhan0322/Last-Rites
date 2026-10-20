using System;

/// <summary>
/// [역할] 무기 박스(로컬 크기·중심)에서 칼날 모양을 계산한다: 칼날 축(가장 긴 축), 구 반지름(나머지 두 축 중 큰 쪽의 절반),
///        칼날 위 점(로컬). 양 끝에서 반지름만큼 안쪽에 첫·끝 점을 두고 사이는 반지름 이하 간격으로 채운다.
/// [이유] 판정 크기를 지금 무기 박스와 같게 유지하면서, 구들이 겹쳐 칼날 위에 빈틈이 없게 한다.
/// [근거] 공격 판정 명세 3.3, 결정 H3·H6
/// </summary>
public sealed class BladeLayout
{
    public int Axis { get; }                                // 0 = x, 1 = y, 2 = z
    public float Radius { get; }                            // 로컬 단위
    public (float X, float Y, float Z)[] Points { get; }    // 로컬 좌표

    private BladeLayout(int axis, float radius, (float X, float Y, float Z)[] points)
    {
        Axis = axis;
        Radius = radius;
        Points = points;
    }

    public static BladeLayout FromBox(float sizeX, float sizeY, float sizeZ, float centerX, float centerY, float centerZ)
    {
        float[] size = { sizeX, sizeY, sizeZ };

        int axis = 0;
        for (int i = 1; i < 3; i++)
        {
            if (size[i] > size[axis]) axis = i;   // 같으면 앞 축 우선
        }

        float width = 0f;
        for (int i = 0; i < 3; i++)
        {
            if (i != axis && size[i] > width) width = size[i];
        }
        float radius = width * 0.5f;

        float length = size[axis];
        float span = length - 2f * radius;   // 첫 점 ~ 끝 점 거리
        int count = span <= 0f || radius <= 0f ? 1 : (int)Math.Ceiling(span / radius) + 1;

        var points = new (float X, float Y, float Z)[count];
        for (int i = 0; i < count; i++)
        {
            float offset = count == 1 ? 0f : -length * 0.5f + radius + span * i / (count - 1);
            float[] p = { centerX, centerY, centerZ };
            p[axis] += offset;
            points[i] = (p[0], p[1], p[2]);
        }
        return new BladeLayout(axis, radius, points);
    }
}

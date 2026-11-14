using System.Collections.Generic;

/// <summary>
/// [역할] 클릭 목적지 규칙(엔진 없음): 클릭 광선이 닿은 곳들 중 걸을 수 있는(근처에 NavMesh가 있는) 가장 가까운 곳을 고른다.
/// [이유] 로비 벽·난간·소품도 Ground 레이어라 광선이 바닥보다 벽에 먼저 닿으면 이동하지 못했다(사용자는 벽을 Ignore Raycast로 바꿔 피함).
///        처음 닿은 곳이 걸을 수 있으면 예전과 같은 곳을 고르고, 아니면 그 뒤의 바닥을 고른다.
/// [근거] 로비 이동 막힘 명세 2.3·3.1, 결정 N3·N4
/// </summary>
public static class ClickTargetRule
{
    /// 걸을 수 있는 곳 중 거리가 가장 짧은 번호(거리가 같으면 앞 번호), 없으면 -1. 입력 순서는 섞여 있어도 된다(Unity가 거리순으로 주지 않음)
    public static int PickFirstWalkable(IReadOnlyList<float> distances, IReadOnlyList<bool> walkable)
    {
        int best = -1;
        for (int i = 0; i < distances.Count; i++)
        {
            if (!walkable[i]) continue;
            if (best < 0 || distances[i] < distances[best]) best = i;
        }
        return best;
    }
}

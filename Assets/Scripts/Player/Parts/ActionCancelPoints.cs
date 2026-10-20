using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// [역할] 애니메이터 기본 레이어에서 지금 가장 크게 재생 중인 클립의 "타격이 끝나는 시점"(0~1)을 알려 준다.
///        클립마다 한 번만 계산해 기억한다.
/// [이유] 순수 규칙 CancelPointRule에 넘길 이벤트 목록을 엔진(AnimationClip.events)에서 꺼내는 일을 맡는다.
/// [근거] 조작감 명세 3.1, 결정 C3·C5
/// </summary>
public class ActionCancelPoints
{
    private readonly PlayerController _owner;
    private readonly Dictionary<AnimationClip, float> _hitEndByClip = new Dictionary<AnimationClip, float>();   // 타격 이벤트가 없으면 NaN
    private readonly List<AnimatorClipInfo> _clipInfos = new List<AnimatorClipInfo>();
    private readonly List<(string Name, float Time)> _events = new List<(string Name, float Time)>();

    public ActionCancelPoints(PlayerController owner)
    {
        _owner = owner;
    }

    /// 지금 클립의 타격 끝 시점. 클립이 없거나 타격 이벤트가 없으면 fallback
    public float CurrentHitEnd(float fallback)
    {
        _owner.Anim.GetCurrentAnimatorClipInfo(PlayerAnimParams.BaseLayer, _clipInfos);

        AnimationClip clip = null;
        float bestWeight = -1f;
        foreach (AnimatorClipInfo info in _clipInfos)
        {
            if (info.weight > bestWeight)
            {
                bestWeight = info.weight;
                clip = info.clip;
            }
        }
        if (clip == null) return fallback;

        if (!_hitEndByClip.TryGetValue(clip, out float hitEnd))
        {
            hitEnd = Compute(clip);
            _hitEndByClip[clip] = hitEnd;
        }
        return float.IsNaN(hitEnd) ? fallback : hitEnd;
    }

    // 이벤트 시간(초) ÷ 클립 길이 = 0~1 진행도. FBX 가져오기 설정의 이벤트도 clip.events에 들어 있다 (검증 기록 W2)
    private float Compute(AnimationClip clip)
    {
        _events.Clear();
        if (clip.length > 0f)
        {
            foreach (AnimationEvent evt in clip.events)
                _events.Add((evt.functionName, evt.time / clip.length));
        }
        return CancelPointRule.HitEnd(_events, float.NaN);
    }
}

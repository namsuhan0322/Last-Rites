using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// [역할] 이동 상태: 우클릭 지점으로 NavMesh 이동(누르고 있으면 계속 갱신), 좌클릭 → 공격, 회피·스킬 입력.
///        우클릭 광선이 벽·난간 등에 먼저 닿으면 그 뒤의 걸을 수 있는 바닥을 목적지로 고른다.
/// [이유] 이동만의 처리(클릭 지점 갱신·도착 판정)만 남기고, 평타 시작은 PlayerState.TryStartBasicAttack을 쓴다.
///        로비 벽·난간·소품도 Ground 레이어라, 처음 닿은 곳만 보면 벽이 가린 바닥을 클릭해도 움직이지 못했다.
/// [근거] 명세 3.4, 결정 R14(타이밍·거리 숫자는 이름 있는 상수) / 로비 이동 막힘 명세 3.2, 결정 N3
/// </summary>
public class PlayerMoveState : PlayerState
{
    private const float StoppingDistance = 0.5f;    // 목적지 앞 0.5m에서 멈춤
    private const float ArriveDistance = 0.1f;      // 남은 거리 0.1 이하면 대기로
    private const float ClickRayDistance = 100f;    // 마우스 클릭 레이캐스트 거리
    private const float NavSampleRadius = 1.0f;     // 클릭 지점 주변 1m 안의 NavMesh로 보정
    private const float ClickEffectHeight = 0.1f;   // 클릭 이펙트는 바닥 위 0.1m
    private const int ClickHitBufferSize = 16;      // 광선 하나가 지나는 Ground 콜라이더(바닥·벽·난간·소품)는 이보다 훨씬 적음

    // 클릭 광선이 닿은 곳들 — 매 프레임 새로 잡지 않도록 미리 만든 칸 (결정 N3)
    private readonly RaycastHit[] _clickHits = new RaycastHit[ClickHitBufferSize];
    private readonly float[] _clickDistances = new float[ClickHitBufferSize];
    private readonly bool[] _clickWalkable = new bool[ClickHitBufferSize];
    private readonly Vector3[] _clickNavPoints = new Vector3[ClickHitBufferSize];

    public PlayerMoveState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        _player.Stats.SetInvincible(false);
        _player.Agent.stoppingDistance = StoppingDistance;
        _player.ForceDisableAllActionEffects();
        SetDestinationToMouse();
    }

    public override void HandleInput()
    {
        if (PlayerInputReader.MoveHeld) SetDestinationToMouse();
        TryStartBasicAttack();
        _player.TryRunBufferedSkillOrDodge();
    }

    public override void LogicUpdate()
    {
        _player.Motor.RotateTowardsMovement();
        _player.Motor.UpdateMoveAnimation();

        if (!_player.Agent.pathPending)
        {
            if (_player.Agent.remainingDistance <= ArriveDistance)
            {
                _stateMachine.ChangeState(_player.IdleState);
            }
        }
    }

    public override void PhysicsUpdate()
    {
        _player.Motor.MoveWithNavMesh();
    }

    public override void Exit()
    {
        _player.Agent.ResetPath();
        _player.Agent.velocity = Vector3.zero;
    }

    private void SetDestinationToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(PlayerInputReader.MousePosition);
        int count = Physics.RaycastNonAlloc(ray, _clickHits, ClickRayDistance, _player.GroundLayer);

        // 닿은 곳마다 1m 안에 NavMesh가 있는지 — 벽 중간처럼 근처에 바닥이 없는 곳은 건너뛰고 그 뒤 바닥을 쓴다 (결정 N3)
        for (int i = 0; i < count; i++)
        {
            _clickDistances[i] = _clickHits[i].distance;
            _clickWalkable[i] = NavMesh.SamplePosition(_clickHits[i].point, out NavMeshHit navHit, NavSampleRadius, NavMesh.AllAreas);
            _clickNavPoints[i] = navHit.position;
        }

        int pick = ClickTargetRule.PickFirstWalkable(
            new ArraySegment<float>(_clickDistances, 0, count),
            new ArraySegment<bool>(_clickWalkable, 0, count));
        if (pick < 0) return;

        Vector3 target = _clickNavPoints[pick];
        _player.Agent.SetDestination(target);

        // 누른 순간에만 클릭 이펙트 (누르고 있는 동안은 목적지만 갱신)
        if (PlayerInputReader.MovePressed)
            EffectManager.Instance.PlayEffect("ClickMousePoint", target + Vector3.up * ClickEffectHeight, Quaternion.identity);
    }
}

using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// [역할] 이동 상태: 우클릭 지점으로 NavMesh 이동(누르고 있으면 계속 갱신), 좌클릭 → 공격, 회피·스킬 입력.
/// [이유] 이동만의 처리(클릭 지점 갱신·도착 판정)만 남기고, 평타 시작은 PlayerState.TryStartBasicAttack을 쓴다.
/// [근거] 명세 3.4, 결정 R14(타이밍·거리 숫자는 이름 있는 상수)
/// </summary>
public class PlayerMoveState : PlayerState
{
    private const float StoppingDistance = 0.5f;    // 목적지 앞 0.5m에서 멈춤
    private const float ArriveDistance = 0.1f;      // 남은 거리 0.1 이하면 대기로
    private const float ClickRayDistance = 100f;    // 마우스 클릭 레이캐스트 거리
    private const float NavSampleRadius = 1.0f;     // 클릭 지점 주변 1m 안의 NavMesh로 보정
    private const float ClickEffectHeight = 0.1f;   // 클릭 이펙트는 바닥 위 0.1m

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
        if (Physics.Raycast(ray, out RaycastHit hit, ClickRayDistance, _player.GroundLayer))
        {
            if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, NavSampleRadius, NavMesh.AllAreas))
            {
                _player.Agent.SetDestination(navHit.position);

                // 누른 순간에만 클릭 이펙트 (누르고 있는 동안은 목적지만 갱신)
                if (PlayerInputReader.MovePressed)
                    EffectManager.Instance.PlayEffect("ClickMousePoint", navHit.position + Vector3.up * ClickEffectHeight, Quaternion.identity);
            }
        }
    }
}

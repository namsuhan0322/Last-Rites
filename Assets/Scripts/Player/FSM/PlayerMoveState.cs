using UnityEngine;

public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        _player.Stats.SetInvincible(false);
        _player.Agent.stoppingDistance = 0f;
        _player.ForceDisableAllActionEffects();
        SetDestinationToMouse();
    }

    public override void HandleInput()
    {
        if (Input.GetMouseButton(1)) SetDestinationToMouse();
        if (Input.GetMouseButtonDown(0))
        {
            if (_player.InCombat)
            {
                _stateMachine.ChangeState(_player.AttackState);
            }
        }

        if (_player.CheckSkillAndDashInput()) return;
    }

    public override void LogicUpdate()
    {
        _player.RotateTowardsMovement();
        _player.UpdateMoveAnimation();

        if (!_player.Agent.pathPending)
        {
            if (_player.Agent.remainingDistance <= 0.1f)
            {
                _stateMachine.ChangeState(_player.IdleState);
            }
        }
    }

    public override void PhysicsUpdate()
    {
        _player.MoveWithNavMesh();
    }

    public override void Exit()
    {
        _player.Agent.ResetPath();
        _player.Agent.velocity = Vector3.zero;
    }

    private void SetDestinationToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, _player.GroundLayer))
        {
            if (UnityEngine.AI.NavMesh.SamplePosition(hit.point, out UnityEngine.AI.NavMeshHit navHit, 1.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                _player.Agent.SetDestination(navHit.position);

                if (Input.GetMouseButtonDown(1))
                    EffectManager.Instance.PlayEffect("ClickMousePoint", navHit.position + Vector3.up * 0.1f, Quaternion.identity);
            }
        }
    }
}
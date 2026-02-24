using UnityEngine;

public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        _player.Stats.SetInvincible(false);
        _player.Agent.stoppingDistance = 0f;
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
        if (Input.GetKeyDown(KeyCode.Space))
            if (_player.Stats.CurrentStamina >= _player.Stats.DashCost) _stateMachine.ChangeState(_player.RollState);

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (_player.TryUseSkill(KeyCode.Q, "Skill_Q", _player.CurrentWeapon.Q_Dmg, _player.CurrentWeapon.Q_Cool))
                _stateMachine.ChangeState(_player.SkillState);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (_player.TryUseSkill(KeyCode.W, "Skill_W", _player.CurrentWeapon.W_Dmg, _player.CurrentWeapon.W_Cool))
                _stateMachine.ChangeState(_player.SkillState);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_player.TryUseSkill(KeyCode.E, "Skill_E", _player.CurrentWeapon.E_Dmg, _player.CurrentWeapon.E_Cool))
                _stateMachine.ChangeState(_player.SkillState);
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (_player.TryUseSkill(KeyCode.V, "Skill_V", _player.CurrentWeapon.V_Dmg, _player.CurrentWeapon.V_Cool))
                _stateMachine.ChangeState(_player.SkillState);
        }
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
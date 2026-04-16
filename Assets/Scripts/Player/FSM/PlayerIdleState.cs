using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        _player.Anim.SetFloat("Move", 0f);
        _player.Agent.ResetPath();
    }

    public override void HandleInput()
    {
        if (Input.GetMouseButton(1)) // 우클릭 -> 이동
        {
            _stateMachine.ChangeState(_player.MoveState);
        }
        if (Input.GetMouseButtonDown(0)) // 좌클릭 -> 공격
        {
            if (_player.InCombat && _player.postRollAttackTimer <= 0f)
            {
                _stateMachine.ChangeState(_player.AttackState);
            }
        }

        if (_player.CheckSkillAndDashInput()) return;
    }

    public override void LogicUpdate()
    {

    }

    public override void PhysicsUpdate()
    {
        _player.StopAndApplyGravity();
    }
}
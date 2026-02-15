using UnityEngine;

public class PlayerAttackState : PlayerState
{
    private bool _nextComboBuffered;

    public PlayerAttackState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        _player.Agent.ResetPath();
        _player.Anim.SetFloat("Move", 0f);

        _nextComboBuffered = false;

        _player.Anim.ResetTrigger("Attack");
        _player.Anim.SetTrigger("Attack");

        float atkSpd = _player.CurrentWeapon != null ? _player.CurrentWeapon.Atk_Spd : 1f;
        _player.Anim.SetFloat("AttackSpd", atkSpd);
    }

    public override void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _nextComboBuffered = true;
        }

        if (Input.GetMouseButtonDown(1))
        {
            _player.Anim.ResetTrigger("Attack");
            _stateMachine.ChangeState(_player.MoveState);
        }
    }

    public override void LogicUpdate()
    {
        _player.StopAndApplyGravity();

        AnimatorStateInfo stateInfo = _player.Anim.GetCurrentAnimatorStateInfo(1);

        if (stateInfo.IsTag("Attack"))
        {
            if (stateInfo.normalizedTime >= 0.95f)
            {
                if (_nextComboBuffered)
                {
                    _stateMachine.ChangeState(_player.AttackState);
                }
                else
                {
                    _stateMachine.ChangeState(_player.IdleState);
                }
            }
        }
        else if (_player.StateMachine.CurrentState == this && stateInfo.normalizedTime > 1.0f)
        {
            _stateMachine.ChangeState(_player.IdleState);
        }
    }

    public override void Exit()
    {
        _player.Anim.ResetTrigger("Attack");
    }
}
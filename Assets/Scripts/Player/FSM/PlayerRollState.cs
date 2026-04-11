using UnityEngine;

public class PlayerRollState : PlayerState
{
    private float _stateTimer;

    public PlayerRollState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        _player.Stats.UseStamina(_player.Stats.DashCost);
        _player.Stats.SetInvincible(true);
        _player.TogglePlayerOutline(false);
        _stateTimer = 0f;
        _player.Anim.applyRootMotion = true;

        _player.RotateToMouseImmediate();
        _player.ForceDisableAllActionEffects();

        _player.Anim.SetTrigger("Roll");
        _player.Agent.ResetPath();
    }

    public override void LogicUpdate()
    {
        _stateTimer += Time.deltaTime;

        AnimatorStateInfo stateInfo = _player.Anim.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Roll") || stateInfo.IsTag("Roll"))
        {
            if (stateInfo.normalizedTime >= 0.9f)
            {
                if (Input.GetMouseButton(1))
                {
                    _player.Stats.SetInvincible(false);
                    _stateMachine.ChangeState(_player.MoveState);
                }
                else
                {
                    _player.Stats.SetInvincibleForSeconds(0.5f);
                    _stateMachine.ChangeState(_player.IdleState);
                }
            }
        }
        else
        {
            if (_stateTimer >= 1.5f)
            {
                _stateMachine.ChangeState(_player.IdleState);
            }
        }
    }

    public override void PhysicsUpdate() { }

    public override void Exit()
    {
        _player.Anim.applyRootMotion = false;
        _player.Agent.velocity = Vector3.zero;
        _player.TogglePlayerOutline(true);

        _player.ResetDashTimer();
    }
}
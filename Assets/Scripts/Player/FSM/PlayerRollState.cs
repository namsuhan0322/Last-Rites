using UnityEngine;

public class PlayerRollState : PlayerState
{
    private bool _attackBuffered;
    private float _stateTimer;

    public PlayerRollState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        _player.ResetDashTimer();

        _player.Stats.SetInvincible(true);
        _stateTimer = 0f;
        _player.Anim.applyRootMotion = true;

        _player.RotateToMouseImmediate();
        _player.ForceDisableAllActionEffects();

        _player.Anim.SetTrigger("Roll");
        _player.Agent.ResetPath();

        _attackBuffered = false;
    }

    public override void HandleInput()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
        {
            _attackBuffered = true;
        }
    }

    public override void LogicUpdate()
    {
        _stateTimer += Time.deltaTime;

        AnimatorStateInfo stateInfo = _player.Anim.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Roll") || stateInfo.IsTag("Roll"))
        {
            float normalizedTime = stateInfo.normalizedTime;
            if (normalizedTime >= 0.85f && _attackBuffered)
            {
                _player.Stats.SetInvincible(false);
                _stateMachine.ChangeState(_player.AttackState);
                return;
            }

            if (stateInfo.normalizedTime >= 0.65f)
            {
                _player.Stats.SetInvincible(false);

                if (Input.GetMouseButton(1))
                {
                    _stateMachine.ChangeState(_player.MoveState);
                    return;
                }
                else if (stateInfo.normalizedTime >= 0.9f)
                {
                    _player.Stats.SetInvincibleForSeconds(0.5f);
                    _stateMachine.ChangeState(_player.IdleState);
                    return;
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
        _attackBuffered = false;
        _player.postRollAttackTimer = 0f;
    }
}
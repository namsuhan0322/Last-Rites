using UnityEngine;

public class PlayerStunState : PlayerState
{
    private float _stunDuration = 3.0f;
    private float _stateTimer;

    public PlayerStunState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        _player.Agent.ResetPath();
        _player.Agent.velocity = Vector3.zero;
        _player.Anim.SetLayerWeight(1, 0f);
        _player.Anim.SetTrigger("IsStun");

        _stateTimer = 0f;

        _player.ForceDisableAllActionEffects();

        // 스턴 이펙트나 사운드 재생
    }

    public override void LogicUpdate()
    {
        _player.StopAndApplyGravity();
        _stateTimer += Time.deltaTime;

        if (_stateTimer >= _stunDuration)
        {
            if (!_player.Stats.IsDead)
            {
                _stateMachine.ChangeState(_player.IdleState);
            }
        }
    }

    public override void Exit()
    {
        _player.Anim.ResetTrigger("IsStun");
    }
}
using UnityEngine;

public class PlayerRollState : PlayerState
{
    private float _rollDuration;
    private float _stateTimer;

    public PlayerRollState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        _stateTimer = 0f;
        _player.Anim.SetLayerWeight(1, 0f);
        _player.Anim.applyRootMotion = true;
        _player.Anim.SetTrigger("Roll");
        _player.Agent.ResetPath();
        _rollDuration = 1.0f;
    }

    public override void LogicUpdate()
    {
        _stateTimer += Time.deltaTime;

        _player.Anim.SetLayerWeight(1, 0f);

        AnimatorStateInfo stateInfo = _player.Anim.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Roll") || stateInfo.IsTag("Roll"))
        {
            if (_rollDuration == 1.0f && stateInfo.length > 0)
            {
                _rollDuration = stateInfo.length;
            }
        }

        // 시간 종료 체크
        if (_stateTimer >= _rollDuration)
        {
            _stateMachine.ChangeState(_player.IdleState);
        }
    }

    public override void PhysicsUpdate() { }

    public override void Exit()
    {
        _player.Anim.SetLayerWeight(1, 1f);
        _player.Anim.applyRootMotion = false;
        _player.Agent.velocity = Vector3.zero;
    }
}
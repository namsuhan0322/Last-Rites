using UnityEngine;

public class PlayerRollState : PlayerState
{
    private Vector3 _rollDir;
    private float _rollSpeed;
    private float _rollDuration;
    private float _stateTimer;

    public PlayerRollState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        // 초기화 & 상체 끄기
        _stateTimer = 0f;
        _player.Anim.SetLayerWeight(1, 0f); // 상체 끄기
        _player.Anim.SetTrigger("Roll");
        _player.Agent.ResetPath();

        // 방향 설정
        if (_player.Agent.velocity.sqrMagnitude > 0.1f)
            _rollDir = _player.Agent.velocity.normalized;
        else
            _rollDir = _player.transform.forward;

        _rollSpeed = _player.Stats.DashSpeed;

        _rollDuration = 0.8f;
    }

    public override void LogicUpdate()
    {
        _stateTimer += Time.deltaTime;

        AnimatorStateInfo stateInfo = _player.Anim.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("Roll") || stateInfo.IsTag("Roll"))
        {
            if (_rollDuration > 2.0f && stateInfo.length > 0)
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

    public override void PhysicsUpdate()
    {
        Vector3 move = _rollDir * _rollSpeed;
        move.y += -9.81f;

        _player.CC.Move(move * Time.deltaTime);
    }

    public override void Exit()
    {
        _player.Anim.SetLayerWeight(1, 1f);
        _player.Agent.velocity = Vector3.zero;
    }
}
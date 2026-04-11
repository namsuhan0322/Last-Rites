using UnityEngine;

public class PlayerRollState : PlayerState
{
    private float _rollDuration;
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
        _player.ForceDisableAllAttackEffects();

        _player.Anim.SetTrigger("Roll");
        _player.Agent.ResetPath();
        _rollDuration = 1.0f;
    }

    public override void LogicUpdate()
    {
        _stateTimer += Time.deltaTime;

        AnimatorStateInfo stateInfo = _player.Anim.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Roll") || stateInfo.IsTag("Roll"))
        {
            if (_rollDuration == 1.0f && stateInfo.length > 0)
            {
                _rollDuration = stateInfo.length;
            }
        }

        if (_stateTimer >= _rollDuration)
        {
            // 구르기가 끝났을 때의 행동에 따른 무적 처리
            if (Input.GetMouseButton(1))
            {
                // 바로 이동하려 한다면 0.5초 보너스 없이 무적 즉시 해제
                _player.Stats.SetInvincible(false);
                _stateMachine.ChangeState(_player.MoveState);
            }
            else
            {
                // 가만히 대기 상태로 간다면 0.5초 보너스 무적 부여
                _player.Stats.SetInvincibleForSeconds(0.5f);
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
    }
}
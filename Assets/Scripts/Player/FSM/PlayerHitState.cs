using UnityEngine;

public class PlayerHitState : PlayerState
{
    private float _hitSeverity;
    private float _stateTimer;
    private float _stunDuration;

    public PlayerHitState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public void SetSeverity(float severity)
    {
        _hitSeverity = severity;
    }

    public override void Enter()
    {
        _player.Agent.ResetPath();
        _player.Agent.velocity = Vector3.zero;
        _player.Anim.SetLayerWeight(1, 0f);
        _player.Anim.SetFloat("HitPower", _hitSeverity);
        _player.Anim.SetTrigger("IsHit");

        // 강도에 따른 경직(Stun) 시간 설정
        // 0.0f (약함): 0.5초
        // 0.5f (중간): 0.8초
        // 1.0f (강함/크리티컬): 1.2초
        if (_hitSeverity >= 1.0f)
            _stunDuration = 1.2f;
        else if (_hitSeverity >= 0.5f)
            _stunDuration = 0.8f;
        else
            _stunDuration = 0.5f;

        // 다구리 방지용
        _player.Stats.SetInvincibleForSeconds(_stunDuration + 0.2f);
        _stateTimer = 0f;
    }

    public override void LogicUpdate()
    {
        _stateTimer += Time.deltaTime;

        _player.StopAndApplyGravity();

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
        _player.Anim.ResetTrigger("IsHit");
    }
}
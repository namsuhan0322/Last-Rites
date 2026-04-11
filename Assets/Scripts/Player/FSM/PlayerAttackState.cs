using UnityEngine;

public class PlayerAttackState : PlayerState
{
    private bool _nextComboBuffered;
    private float _stateTimer;

    public PlayerAttackState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        _player.Stats.SetInvincible(false);
        _player.TogglePlayerOutline(false);

        // 칼 뽑는 애니메이션 이벤트가 스킵되었을 때를 대비한 강제 동기화
        if (_player.VisualManager != null) 
            _player.VisualManager.DrawWeapon();

        _player.Agent.ResetPath();
        _player.Anim.SetFloat("Move", 0f);

        // 상체 레이어 끄기 (공격은 전신이므로)
        _player.Anim.SetLayerWeight(1, 0f);

        _nextComboBuffered = false; 
        
        _player.RotateToMouseImmediate();

        _player.Anim.ResetTrigger("Attack");
        _player.Anim.SetTrigger("Attack");

        float atkSpd = _player.CurrentWeapon != null ? _player.CurrentWeapon.Atk_Spd : 1f;
        _player.Anim.SetFloat("AttackSpd", atkSpd);

        _stateTimer = 0f;
    }

    public override void HandleInput()
    {
        if (Input.GetMouseButtonDown(0)) _nextComboBuffered = true;
        if (Input.GetMouseButtonDown(1))
        {
            _player.Anim.ResetTrigger("Attack");
            _stateMachine.ChangeState(_player.MoveState);
        }
        if (Input.GetKeyDown(KeyCode.Space))
            if (_player.Stats.CurrentStamina >= _player.Stats.DashCost) _stateMachine.ChangeState(_player.RollState);
    }

    public override void LogicUpdate()
    {
        _player.StopAndApplyGravity();
        _stateTimer += Time.deltaTime;

        if (_player.Anim.IsInTransition(0)) return;

        AnimatorStateInfo stateInfo = _player.Anim.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsTag("Attack"))
        {
            if (_stateTimer < 0.1f) return;

            float normalizedTime = stateInfo.normalizedTime;

            // [공격 중] 후딜레이 캔슬 (60% 이상 진행 시 이동 허용)
            if (normalizedTime >= 0.6f)
            {
                if (Input.GetMouseButtonDown(1) || Input.GetMouseButton(1))
                {
                    _stateMachine.ChangeState(_player.MoveState);
                    return;
                }
            }

            // [공격 중] 콤보 연결 또는 종료 (95% 이상 진행 시)
            if (normalizedTime >= 0.95f)
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
        else
        {
            if (_stateTimer > 0.5f)
            {
                _stateMachine.ChangeState(_player.IdleState);
            }
        }
    }

    public override void Exit()
    {
        _player.Anim.ResetTrigger("Attack");
        _player.TogglePlayerOutline(true);

        _player.ForceDisableAllAttackEffects();
    }
}
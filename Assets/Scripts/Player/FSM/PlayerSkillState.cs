using UnityEngine;

public class PlayerSkillState : PlayerState
{
    private float _stateTimer;
    private bool _attackBuffered;
    public PlayerSkillState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        _player.Stats.SetInvincible(false); // 스킬 쓸 때 무적 해제

        if (_player.VisualManager != null)
            _player.VisualManager.DrawWeapon();

        _player.Agent.ResetPath();
        _player.Anim.SetFloat("Move", 0f);
        _player.Anim.SetLayerWeight(1, 0f); // 상체 레이어 끄기

        _player.RotateToMouseImmediate();

        _player.Anim.ResetTrigger(_player.CurrentSkillAnim);
        _player.Anim.SetTrigger(_player.CurrentSkillAnim);

        float baseAtkSpd = _player.CurrentWeapon != null ? _player.CurrentWeapon.Atk_Spd : 1f;
        _player.Anim.SetFloat("AttackSpd", baseAtkSpd * _player.AtkSpeedModifier);

        _stateTimer = 0f;
        _attackBuffered = false;

        _player.globalSkillTimer = _player.globalSkillDelay;
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
        _player.StopAndApplyGravity();
        _stateTimer += Time.deltaTime;

        if (_player.Anim.IsInTransition(0)) return;

        AnimatorStateInfo stateInfo = _player.Anim.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsTag("Skill"))
        {
            float normalizedTime = stateInfo.normalizedTime;

            if (normalizedTime >= 0.4f)
            {
                if (_player.CheckSkillAndDashInput())
                {
                    return;
                }

                if (Input.GetMouseButton(1) || Input.GetMouseButtonDown(1))
                {
                    _player.Anim.CrossFade("Idle/Move", 0.1f);
                    _stateMachine.ChangeState(_player.MoveState);
                    return;
                }
            }

            if (normalizedTime >= 0.55f && _attackBuffered)
            {
                _stateMachine.ChangeState(_player.AttackState);
                return;
            }

            if (normalizedTime >= 0.8f)
            {
                _player.Anim.CrossFade("Idle/Move", 0.1f);
                _stateMachine.ChangeState(_player.IdleState);
            }
        }
        else
        {
            if (_stateTimer > 0.5f) _stateMachine.ChangeState(_player.IdleState);
        }
    }

    public override void Exit()
    {
        _player.Anim.ResetTrigger(_player.CurrentSkillAnim);
        _player.DisableWeaponCollider();

        _attackBuffered = false;
    }
}
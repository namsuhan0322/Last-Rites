using UnityEngine;

public class PlayerSkillState : PlayerState
{
    private float _stateTimer;

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

        float atkSpd = _player.CurrentWeapon != null ? _player.CurrentWeapon.Atk_Spd : 1f;
        _player.Anim.SetFloat("AttackSpd", atkSpd);

        _stateTimer = 0f;
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

            // 스킬 애니메이션이 95% 끝났으면 대기 상태로 복귀
            if (normalizedTime >= 0.95f)
            {
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
    }
}
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

        _player.globalSkillTimer = _player.globalSkillDelay;
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

            if (normalizedTime >= 0.6f)
            {
                // 이 시점부터 다른 스킬이나 회피(Space) 키가 눌리면 즉시 그 상태로 넘어갑니다!
                if (_player.CheckSkillAndDashInput())
                {
                    return; // 성공적으로 다른 스킬/회피가 나갔다면 여기서 멈춤
                }
            }

            // 아무것도 안 누르고 가만히 있으면 95%에서 대기 상태로 복귀
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
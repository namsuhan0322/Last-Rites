using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        _player.Anim.SetFloat("Move", 0f);
        _player.Agent.ResetPath();
    }

    public override void HandleInput()
    {
        if (Input.GetMouseButtonDown(1)) // 우클릭 -> 이동
        {
            _stateMachine.ChangeState(_player.MoveState);
        }
        if (Input.GetMouseButtonDown(0)) // 좌클릭 -> 공격
        {
            if (_player.InCombat)
            {
                _stateMachine.ChangeState(_player.AttackState);
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
            if (_player.Stats.CurrentStamina >= _player.Stats.DashCost) _stateMachine.ChangeState(_player.RollState);

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (_player.TryUseSkill(KeyCode.Q, "Skill_Q", _player.CurrentWeapon.Q_Dmg, _player.CurrentWeapon.Q_Cool))
                _stateMachine.ChangeState(_player.SkillState);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (_player.TryUseSkill(KeyCode.W, "Skill_W", _player.CurrentWeapon.W_Dmg, _player.CurrentWeapon.W_Cool))
                _stateMachine.ChangeState(_player.SkillState);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_player.TryUseSkill(KeyCode.E, "Skill_E", _player.CurrentWeapon.E_Dmg, _player.CurrentWeapon.E_Cool))
                _stateMachine.ChangeState(_player.SkillState);
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (_player.TryUseSkill(KeyCode.V, "Skill_V", _player.CurrentWeapon.V_Dmg, _player.CurrentWeapon.V_Cool))
                _stateMachine.ChangeState(_player.SkillState);
        }
    }

    public override void LogicUpdate()
    {

    }

    public override void PhysicsUpdate()
    {
        _player.StopAndApplyGravity();
    }
}
using UnityEngine;

public class PlayerDeadState : PlayerState
{
    public PlayerDeadState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        _player.Anim.SetLayerWeight(1, 0f);

        _player.Anim.SetTrigger("IsDead");
        _player.Agent.ResetPath();
        _player.Agent.velocity = Vector3.zero;
        _player.Agent.isStopped = true;
        _player.ForceDisableAllActionEffects();

        if (_player.CC != null) 
            _player.CC.enabled = false;

        if (_player.RB != null)
        {
            _player.RB.linearVelocity = Vector3.zero;
            _player.RB.angularVelocity = Vector3.zero;
            _player.RB.useGravity = false;
            _player.RB.isKinematic = true;
        }

        GameManager.Instance.GameOver();
    }
}
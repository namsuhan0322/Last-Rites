using UnityEngine;

/// <summary>
/// [역할] 사망 상태: 멈추고, 충돌·물리를 끄고, GameManager에 게임 오버를 알린다 (부활은 GameManager → PlayerController.Revive).
/// [이유] 사망만의 처리(충돌·물리 끄기, 게임 오버 알림)만 남기고, 멈춤은 PlayerState.StopForReaction을 쓴다.
/// [근거] 명세 3.4, 조작감 C7
/// </summary>
public class PlayerDeadState : PlayerState
{
    public PlayerDeadState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override bool IsFullBodyAction => true;
    public override bool BlocksInputBuffer => true;   // 조작감 C7⑤
    public override bool IsIncapacitated => true;

    public override void Enter()
    {
        StopForReaction();
        _player.Anim.SetTrigger(PlayerAnimParams.IsDead);
        _player.Agent.isStopped = true;

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

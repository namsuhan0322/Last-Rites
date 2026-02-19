using UnityEngine;

public class PlayerRollState : PlayerState
{
    private float _rollDuration;
    private float _stateTimer;

    public PlayerRollState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        _stateTimer = 0f;
        _player.Anim.applyRootMotion = true;

        RotateToMouseImmediate();

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
            if (Input.GetMouseButton(1)) _stateMachine.ChangeState(_player.MoveState);
            else _stateMachine.ChangeState(_player.IdleState);
        }
    }

    private void RotateToMouseImmediate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, _player.GroundLayer))
        {
            Vector3 targetPoint = hit.point;
            targetPoint.y = _player.transform.position.y;

            Vector3 dir = (targetPoint - _player.transform.position).normalized;

            if (dir != Vector3.zero)
            {
                _player.transform.rotation = Quaternion.LookRotation(dir);
            }
        }
    }

    public override void PhysicsUpdate() { }

    public override void Exit()
    {
        _player.Anim.applyRootMotion = false;
        _player.Agent.velocity = Vector3.zero;
    }
}
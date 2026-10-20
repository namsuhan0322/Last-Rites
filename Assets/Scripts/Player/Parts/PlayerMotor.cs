using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// [역할] 플레이어 이동: NavMesh 이동, 중력, 이동 방향 회전, 마우스 방향 즉시 회전, 이동 애니 값,
///        구르기 루트모션, 낙하 복구.
/// [이유] PlayerController에 흩어져 있던 이동 코드를 한곳에 모은다.
/// [근거] 명세 3.3 PlayerMotor, 결정 R4, R27(설정값은 쓸 때마다 PlayerController에서 읽음)
/// </summary>
public class PlayerMotor
{
    private const float Gravity = -9.81f;               // 지금 코드의 _gravity
    private const float GroundedStickVelocity = -2f;    // 땅에 닿아 있으면 아래로 살짝 눌러 둔다
    private const float FallRecoverHeight = -2f;        // 이 높이 아래로 떨어지면 NavMesh 위로 복구
    private const float FallRecoverSampleUp = 10f;      // 복구할 때 현재 위치 위 10m에서
    private const float FallRecoverSampleRadius = 20f;  // 반경 20 안의 NavMesh를 찾는다
    private const float MinRotateSpeedSqr = 0.1f;       // 이동 방향 회전을 시작하는 최소 속도(제곱)
    private const float MoveAnimStopSpeed = 0.1f;       // 이 속도 미만이면 이동 애니 값 0
    private const float ArriveMargin = 0.1f;            // 남은 거리가 정지 거리 + 0.1 이하(도착 직전)여도 0
    private const float MouseRotateMinDistance = 0.5f;  // 마우스가 이 거리 안이면 회전하지 않음

    private readonly PlayerController _owner;
    private float _verticalVelocity;

    public PlayerMotor(PlayerController owner)
    {
        _owner = owner;
    }

    private Transform Tr => _owner.transform;
    private NavMeshAgent Agent => _owner.Agent;
    private CharacterController CC => _owner.CC;
    private Animator Anim => _owner.Anim;

    public void MoveWithNavMesh()
    {
        Vector3 worldDeltaPosition = Agent.desiredVelocity;

        ApplyGravityAndMove(worldDeltaPosition);

        Agent.nextPosition = Tr.position;
    }

    public void StopAndApplyGravity()
    {
        Agent.velocity = Vector3.zero;
        ApplyGravityAndMove(Vector3.zero);
        Agent.nextPosition = Tr.position;
    }

    private void ApplyGravityAndMove(Vector3 motionVelocity)
    {
        if (CC.isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = GroundedStickVelocity;
        }
        _verticalVelocity += Gravity * Time.deltaTime;

        Vector3 finalMove = motionVelocity + Vector3.up * _verticalVelocity;

        CC.Move(finalMove * Time.deltaTime);
    }

    public void RotateTowardsMovement()
    {
        if (Agent.desiredVelocity.sqrMagnitude > MinRotateSpeedSqr)
        {
            Vector3 lookDirection = Agent.desiredVelocity;
            lookDirection.y = 0;

            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                Tr.rotation = Quaternion.Slerp(Tr.rotation, targetRotation, Time.deltaTime * _owner.RotateSpeed);
            }
        }
    }

    public void UpdateMoveAnimation()
    {
        if (Anim == null) return;

        float speed = Agent.desiredVelocity.magnitude;

        if (speed < MoveAnimStopSpeed || (Agent.remainingDistance <= Agent.stoppingDistance + ArriveMargin))
        {
            speed = 0f;
        }

        Anim.SetFloat(PlayerAnimParams.Move, speed, _owner.AnimationSmoothTime, Time.deltaTime);
    }

    public void RotateToMouseImmediate()
    {
        Ray ray = Camera.main.ScreenPointToRay(PlayerInputReader.MousePosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, Tr.position.y, 0));
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 targetPoint = ray.GetPoint(rayDistance);
            if (Vector3.Distance(Tr.position, targetPoint) > MouseRotateMinDistance)
            {
                Vector3 dir = (targetPoint - Tr.position).normalized;
                dir.y = 0;

                if (dir != Vector3.zero)
                {
                    Tr.rotation = Quaternion.LookRotation(dir);
                }
            }
        }
    }

    /// 구르기 중 루트모션 이동 (PlayerController.OnAnimatorMove에서 구르기 상태일 때만 부름)
    public void ApplyRollRootMotion()
    {
        Vector3 velocity = Anim.deltaPosition;
        velocity.y = _verticalVelocity * Time.deltaTime;

        CC.Move(velocity);

        Tr.rotation = Anim.rootRotation;
        Agent.nextPosition = Tr.position;
    }

    /// LateUpdate: NavMeshAgent 위치를 실제 위치에 맞추고, 바닥 아래로 떨어졌으면 NavMesh 위로 복구
    public void SyncAgentAndRecoverFall()
    {
        if (Agent != null && Agent.enabled && Agent.isOnNavMesh)
        {
            Agent.nextPosition = Tr.position;
        }

        if (Tr.position.y < FallRecoverHeight)
        {
            _verticalVelocity = 0f;

            if (NavMesh.SamplePosition(Tr.position + (Vector3.up * FallRecoverSampleUp), out NavMeshHit hit, FallRecoverSampleRadius, NavMesh.AllAreas))
            {
                if (CC != null) CC.enabled = false;
                Tr.position = hit.position;
                if (CC != null) CC.enabled = true;
            }
        }
    }
}

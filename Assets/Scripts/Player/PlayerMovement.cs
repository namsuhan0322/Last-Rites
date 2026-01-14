using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : MonoBehaviour
{
    #region 레퍼런스
    private NavMeshAgent agent;
    private CharacterController cc;
    private PlayerStats stats;

    [Header("스탯")]
    [SerializeField] private float _rotateSpeed = 10f;

    private float _gravity = -9.81f;
    private float _verticalVelocity;

    #endregion

    #region 초기화
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        cc = GetComponent<CharacterController>();
        stats = GetComponent<PlayerStats>();

        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.speed = stats.MoveSpeed;
    }

    #endregion

    #region 업데이트
    void Update()
    {
        UpdateMovement();
        UpdateRotation();
    }

    #endregion

    #region 움직임
    public void MoveTo(Vector3 destination)
    {
        agent.SetDestination(destination);
    }

    private void UpdateMovement()
    {
        Vector3 worldDeltaPosition = agent.desiredVelocity;

        if (cc.isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = -2f;
        }
        _verticalVelocity += _gravity * Time.deltaTime;

        Vector3 finalMove = worldDeltaPosition + Vector3.up * _verticalVelocity;
        cc.Move(finalMove * Time.deltaTime);

        agent.nextPosition = transform.position;
    }

    private void UpdateRotation()
    {
        if (agent.desiredVelocity.sqrMagnitude > 0.1f)
        {
            Vector3 lookDirection = agent.desiredVelocity;
            lookDirection.y = 0;

            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotateSpeed);
            }
        }
    }

    #endregion
}
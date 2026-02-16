using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    #region State Machine
    public PlayerStateMachine StateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerAttackState AttackState { get; private set; }
    public PlayerRollState RollState { get; private set; }
    public PlayerHitState HitState { get; private set; }
    public PlayerDeadState DeadState { get; private set; }

    #endregion

    #region Components & Settings
    public NavMeshAgent Agent { get; private set; }
    public CharacterController CC { get; private set; }
    public PlayerStats Stats { get; private set; }
    public Animator Anim { get; private set; }

    [Header("스탯")]
    [SerializeField] public float RotateSpeed = 10f;
    [SerializeField] public float AnimationSmoothTime = 0.1f;

    private float _gravity = -9.81f;
    private float _verticalVelocity;

    public WeaponSO CurrentWeapon;
    [HideInInspector] public int CurrentComboStep = 0;
    [HideInInspector] public float LastAttackTime = 0;

    [Header("Input")]
    public LayerMask GroundLayer;

    [Header("전투 감지 센서")]
    public float DetectionRadius = 8.0f;   
    [Range(0, 360)]
    public float ViewAngle = 120.0f;     
    public LayerMask EnemyLayer;           
    public float CombatCooldown = 5.0f;    

    private float _combatTimer;
    private bool _inCombat;

    public bool InCombat => _inCombat;

    #endregion

    private void Awake()
    {
        StateMachine = new PlayerStateMachine();
        IdleState = new PlayerIdleState(this, StateMachine);
        MoveState = new PlayerMoveState(this, StateMachine);
        AttackState = new PlayerAttackState(this, StateMachine);
        RollState = new PlayerRollState(this, StateMachine);
        HitState = new PlayerHitState(this, StateMachine);
        DeadState = new PlayerDeadState(this, StateMachine);

        Agent = GetComponent<NavMeshAgent>();
        CC = GetComponent<CharacterController>();
        Stats = GetComponent<PlayerStats>();
        Anim = GetComponent<Animator>();

        Stats.OnHit += HandleHit;
        Stats.OnDeath += HandleDeath;
    }

    private void Start()
    {
        Agent.updatePosition = false;
        Agent.updateRotation = false;
        Agent.speed = Stats.MoveSpeed;

        StateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        StateMachine.CurrentState.HandleInput();
        StateMachine.CurrentState.LogicUpdate();

        CheckEnemyInSight();
    }

    private void FixedUpdate()
    {
        StateMachine.CurrentState.PhysicsUpdate();
    }

    private void OnDestroy()
    {
        if (Stats != null)
        {
            Stats.OnHit -= HandleHit;
            Stats.OnDeath -= HandleDeath;
        }
    }

    public void MoveWithNavMesh()
    {
        Vector3 worldDeltaPosition = Agent.desiredVelocity;

        ApplyGravityAndMove(worldDeltaPosition);

        Agent.nextPosition = transform.position;
    }

    public void StopAndApplyGravity()
    {
        Agent.velocity = Vector3.zero;
        ApplyGravityAndMove(Vector3.zero);
        Agent.nextPosition = transform.position;
    }

    private void ApplyGravityAndMove(Vector3 motionVelocity)
    {
        if (CC.isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = -2f;
        }
        _verticalVelocity += _gravity * Time.deltaTime;

        Vector3 finalMove = motionVelocity + Vector3.up * _verticalVelocity;

        CC.Move(finalMove * Time.deltaTime);
    }

    public void RotateTowardsMovement()
    {
        if (Agent.desiredVelocity.sqrMagnitude > 0.1f)
        {
            Vector3 lookDirection = Agent.desiredVelocity;
            lookDirection.y = 0;

            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * RotateSpeed);
            }
        }
    }

    public void UpdateMoveAnimation()
    {
        if (Anim == null) return;

        float speed = Agent.desiredVelocity.magnitude;

        if (speed < 0.1f || (Agent.remainingDistance <= Agent.stoppingDistance + 0.1f))
        {
            speed = 0f;
        }

        Anim.SetFloat("Move", speed, AnimationSmoothTime, Time.deltaTime);
    }

    private void OnAnimatorMove()
    {
        if (StateMachine.CurrentState is PlayerRollState)
        {
            Vector3 velocity = Anim.deltaPosition;
            velocity.y = _verticalVelocity * Time.deltaTime;

            CC.Move(velocity);

            transform.rotation = Anim.rootRotation;
            Agent.nextPosition = transform.position;
        }
    }

    private void HandleHit(float severity)
    {
        HitState.SetSeverity(severity);
        StateMachine.ChangeState(HitState);
    }

    private void HandleDeath()
    {
        // 이미 죽은 상태라면 무시
        if (StateMachine.CurrentState == DeadState) return;

        // 상태 강제 전환
        StateMachine.ChangeState(DeadState);
    }

    #region 적 탐지
    private void CheckEnemyInSight()
    {
        // 주변의 적(Collider)들을 모두 찾음
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, DetectionRadius, EnemyLayer);

        bool enemyFound = false;

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < ViewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, LayerMask.GetMask("Default")))
                {
                    enemyFound = true;
                    break;
                }
            }
        }

        if (enemyFound)
        {
            _inCombat = true;
            _combatTimer = CombatCooldown;
        }
        else
        {
            if (_combatTimer > 0)
            {
                _combatTimer -= Time.deltaTime;
            }
            else
            {
                _inCombat = false;
            }
        }

        Anim.SetBool("InCombat", _inCombat);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, DetectionRadius);

        Vector3 viewAngleA = DirFromAngle(-ViewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(ViewAngle / 2, false);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * DetectionRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * DetectionRadius);
    }

    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    #endregion
}
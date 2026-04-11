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
    public PlayerStunState StunState { get; private set; }
    public PlayerSkillState SkillState { get; private set; }

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
    public WeaponHitbox Hitbox;
    [HideInInspector] public int CurrentComboStep = 0;

    public WeaponVisualManager VisualManager;

    [Tooltip("R스킬 쓰면 무기 강화 관련 이펙트")]
    public GameObject weaponEffect;

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
    public TutorialSystem tutorialSystem;
    SkillTutorial skillTutorial;
    public bool InCombat => _inCombat;

    [Header("스킬 관리")]
    [HideInInspector] public string CurrentSkillAnim;   // 어떤 스킬 애니메이션을 틀지
    [HideInInspector] public int CurrentSkillDamage;    // 현재 스킬 데미지가 얼마인지
    [HideInInspector] public float CurrentSkillVal;

    [Header("무기 특수 버프 플래그")]
    [HideInInspector] public bool HasRBuff = false;         // 대검 (결정타/슈퍼아머)
    [HideInInspector] public bool HasTwinBuff = false;      // 쌍검 (스태미나 감소)
    [HideInInspector] public bool HasSpearBuff = false;     // 창 (방어력 관통)
    [HideInInspector] public bool HasShieldBuff = false;    // 검방 (피해 70% 감소/반격)

    public float Q_Timer { get; private set; }
    public float W_Timer { get; private set; }
    public float E_Timer { get; private set; }
    public float R_Timer { get; private set; }
    public float V_Timer { get; private set; }

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
        StunState = new PlayerStunState(this, StateMachine);
        SkillState = new PlayerSkillState(this, StateMachine);

        Agent = GetComponent<NavMeshAgent>();
        CC = GetComponent<CharacterController>();
        Stats = GetComponent<PlayerStats>();
        Anim = GetComponent<Animator>();
        skillTutorial = FindFirstObjectByType<SkillTutorial>();

        Stats.OnHit += HandleHit;
        Stats.OnDeath += HandleDeath;
        Stats.OnStun += HandleStun;
    }

    private void Start()
    {
        Agent.updatePosition = false;
        Agent.updateRotation = false;
        Agent.speed = Stats.MoveSpeed;

        if (CurrentWeapon != null && Hitbox != null)
        {
            if (Hitbox != null) Hitbox.SetupColliders(CurrentWeapon.weaponType);
            if (VisualManager != null) VisualManager.SetupVisuals(CurrentWeapon.weaponType);
            if (CurrentWeapon.weaponAnimator != null) Anim.runtimeAnimatorController = CurrentWeapon.weaponAnimator;
        }

        StateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        if (tutorialSystem != null && tutorialSystem.tutorialPlaying)
        {
            Agent.ResetPath();
            Anim.SetFloat("Move", 0f);
            return;
        }
        StateMachine.CurrentState.HandleInput();
        StateMachine.CurrentState.LogicUpdate();

        CheckEnemyInSight();
        ManageUpperBodyWeight();
        UpdateSkillCooldowns();
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
            Stats.OnStun -= HandleStun;
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
        if (StateMachine.CurrentState == StunState || StateMachine.CurrentState == DeadState) return;
        if (HasRBuff) return;

        if (severity <= 0f) return;

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

    private void HandleStun()
    {
        // 이미 죽었거나 이미 스턴 상태면 무시
        if (StateMachine.CurrentState == DeadState || StateMachine.CurrentState == StunState) return;
        if (HasRBuff) return;

        Debug.Log("플레이어가 스턴에 걸림!");
        StateMachine.ChangeState(StunState);
    }

    private void ManageUpperBodyWeight()
    {
        // 전신을 써야 하는 상태(공격, 회피 등)에서는 상체 레이어를 즉시 끕니다.
        if (StateMachine.CurrentState == AttackState ||
            StateMachine.CurrentState == RollState ||
            StateMachine.CurrentState == SkillState)
        {
            Anim.SetLayerWeight(1, 0f);
            return;
        }

        AnimatorStateInfo upperState = Anim.GetCurrentAnimatorStateInfo(1);

        bool isEmpty = upperState.IsName("Empty");
        bool isDrawing = upperState.IsName("SheatheHips");
        bool isStowing = upperState.IsName("UnsheatheHips");

        // 이벤트 캔슬 대비용 안전장치
        if (isEmpty && !_inCombat)
        {
            OnWeaponStowed();
        }
        else if (upperState.IsName("Combat (1)") && _inCombat)
        {
            OnWeaponDrawn();
        }

        // 전투 중이거나, 칼 뽑기/넣기 중이면 레이어 켜기
        if (_inCombat || isDrawing || isStowing)
        {
            Anim.SetLayerWeight(1, Mathf.Lerp(Anim.GetLayerWeight(1), 1f, Time.deltaTime * 10f));
        }
        else
        {
            Anim.SetLayerWeight(1, Mathf.Lerp(Anim.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
        }
    }

    public void RotateToMouseImmediate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, GroundLayer))
        {
            Vector3 targetPoint = hit.point;
            targetPoint.y = transform.position.y; // 캐릭터가 위아래로 기울어지는 것 방지

            Vector3 dir = (targetPoint - transform.position).normalized;

            if (dir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(dir);
            }
        }
    }

    public bool CheckSkillAndDashInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Stats.CurrentStamina >= Stats.DashCost)
            {
                StateMachine.ChangeState(RollState);
                return true;
            }
        }

        if (InCombat)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (TryUseSkill(KeyCode.Q, "Skill_Q", CurrentWeapon.Q_Dmg, CurrentWeapon.Q_Cool))
                {
                    StateMachine.ChangeState(SkillState);

                    skillTutorial?.OnPlayerUsedQSkill();  

                    return true;
                }
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                if (TryUseSkill(KeyCode.W, "Skill_W", CurrentWeapon.W_Dmg, CurrentWeapon.W_Cool))
                { StateMachine.ChangeState(SkillState); return true; }
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (TryUseSkill(KeyCode.E, "Skill_E", CurrentWeapon.E_Dmg, CurrentWeapon.E_Cool))
                { StateMachine.ChangeState(SkillState); return true; }
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (TryUse_RSkill(KeyCode.R, "Skill_R", CurrentWeapon.R_Val, CurrentWeapon.R_Cool))
                { StateMachine.ChangeState(SkillState); return true; }
            }
            if (Input.GetKeyDown(KeyCode.V))
            {
                if (TryUseSkill(KeyCode.V, "Skill_V", CurrentWeapon.V_Dmg, CurrentWeapon.V_Cool))
                { StateMachine.ChangeState(SkillState); return true; }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.W) ||
                Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.V))
            {
                Debug.Log("주변에 적이 없어 스킬을 사용할 수 없습니다!");
            }
        }

        return false;
    }

    #region 공격 판정
    public void EnableWeaponCollider()
    {
        if (Hitbox == null || CurrentWeapon == null) return;

        int damageToDeal = CurrentWeapon.Combo_1;

        AnimatorStateInfo stateInfo = Anim.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("Attack1"))
        {
            damageToDeal = CurrentWeapon.Combo_1;
        }
        else if (stateInfo.IsName("Attack2"))
        {
            damageToDeal = CurrentWeapon.Combo_2;
        }
        else if (stateInfo.IsName("Attack3"))
        {
            damageToDeal = CurrentWeapon.Combo_3;
        }
        else if (stateInfo.IsTag("Skill"))
        {
            damageToDeal = CurrentSkillDamage;
        }

        if (HasRBuff)
        {
            // 데미지를 CurrentSkillVal(R_Val) 배율만큼 곱해줍니다.
            damageToDeal = Mathf.RoundToInt(damageToDeal * CurrentSkillVal);

            // 공격을 시작했으므로 버프를 끕니다! (허공에 쳐도 날아감)
            HasRBuff = false;

            Debug.Log($"[R 스킬 효과 적용!] 데미지 {damageToDeal}로 뻥튀기 됨! 슈퍼아머 해제.");
        }

        // 히트박스 켜면서 결정된 데미지 전달
        Hitbox.EnableHitbox(damageToDeal);
    }

    // 공격 종료 시 호출
    public void DisableWeaponCollider()
    {
        if (Hitbox != null)
        {
            Hitbox.DisableHitbox();
        }
    }

    // 특정 공격 모션에서 이펙트 끄기
    public void DisableREffect()
    {
        if (weaponEffect != null) weaponEffect.SetActive(false);
    }
    #endregion

    #region 스킬 관련
    private void UpdateSkillCooldowns()
    {
        if (Q_Timer > 0)
        {
            Q_Timer -= Time.deltaTime;
            if (Q_Timer <= 0) Debug.Log("Q 스킬 쿨타임이 끝났습니다!");
        }

        if (W_Timer > 0)
        {
            W_Timer -= Time.deltaTime;
            if (W_Timer <= 0) Debug.Log("W 스킬 쿨타임이 끝났습니다!");
        }

        if (E_Timer > 0)
        {
            E_Timer -= Time.deltaTime;
            if (E_Timer <= 0) Debug.Log("E 스킬 쿨타임이 끝났습니다!");
        }

        if (R_Timer > 0)
        {
            R_Timer -= Time.deltaTime;
            if (R_Timer <= 0) Debug.Log("R 스킬 쿨타임이 끝났습니다!");
        }

        if (V_Timer > 0)
        {
            V_Timer -= Time.deltaTime;
            if (V_Timer <= 0) Debug.Log("V 스킬 쿨타임이 끝났습니다!");
        }
    }

    public bool TryUseSkill(KeyCode key, string animName, int damage, float maxCool)
    {
        CurrentSkillAnim = animName;
        CurrentSkillDamage = damage;

        // 쿨타임이 다 돌았다면(0 이하라면) 스킬 사용 승인 및 쿨타임 초기화
        switch (key)
        {
            case KeyCode.Q: if (Q_Timer <= 0) { Q_Timer = maxCool; return true; } break;
            case KeyCode.W: if (W_Timer <= 0) { W_Timer = maxCool; return true; } break;
            case KeyCode.E: if (E_Timer <= 0) { E_Timer = maxCool; return true; } break;
            case KeyCode.V: if (V_Timer <= 0) { V_Timer = maxCool; return true; } break;
        }

        Debug.Log($"{key} 스킬 쿨타임 중입니다!");
        return false;
    }

    public bool TryUse_RSkill(KeyCode key, string animName, float val, float maxCool)
    {
        CurrentSkillAnim = animName;
        CurrentSkillVal = val;

        switch (key)
        {
            case KeyCode.R:
                if (R_Timer <= 0)
                {
                    R_Timer = maxCool;

                    if (CurrentWeapon != null && CurrentWeapon.R_Skill_Logic != null)
                    {
                        CurrentWeapon.R_Skill_Logic.Execute(this, val);
                    }
                    else
                    {
                        Debug.LogWarning("경고: 현재 무기에 R 스킬(BuffSkill_SO)이 할당되지 않았습니다!");
                    }

                    return true;
                }
                break;
        }

        Debug.Log($"{key} 스킬 쿨타임 중입니다!");
        return false;
    }

    #endregion

    #region 무기 장착, 해제 이벤트 
    // 칼집에서 칼을 뽑는 애니메이션 도중 손이 자루에 닿을 때 호출
    public void OnWeaponDrawn()
    {
        if (VisualManager != null)
            VisualManager.DrawWeapon();

        tutorialSystem?.OnPlayerWeaponDraw();

    }

    // 칼을 칼집에 넣는 애니메이션 도중 손에서 자루를 놓을 때 호출
    public void OnWeaponStowed()
    {
        if (VisualManager != null)
        {
            VisualManager.StowWeapon();
        }
    }
    #endregion

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

    #region 아웃라인 제어
    public void TogglePlayerOutline(bool isOn)
    {
        int targetLayer = isOn ? LayerMask.NameToLayer("Player") : LayerMask.NameToLayer("Default");

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            if (r is ParticleSystemRenderer) continue;
            r.gameObject.layer = targetLayer;
        }
    }

    #endregion

    #region 충돌 및 트리거 감지
    private void OnTriggerEnter(Collider other)
    {
        if ((EnemyLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            if (StateMachine.CurrentState == MoveState)
            {
                StopMovementAndIdle();
            }
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if ((EnemyLayer.value & (1 << hit.gameObject.layer)) > 0)
        {
            if (StateMachine.CurrentState == MoveState)
            {
                StopMovementAndIdle();
            }
        }
    }

    private void StopMovementAndIdle()
    {
        Agent.ResetPath();
        Agent.velocity = Vector3.zero;
        StateMachine.ChangeState(IdleState);
    }

    #endregion
}
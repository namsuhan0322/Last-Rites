using System;
using System.Collections.Generic;
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
    public Rigidbody RB { get; private set; }

    [Header("스탯")]
    [SerializeField] public float RotateSpeed = 10f;
    [SerializeField] public float AnimationSmoothTime = 0.1f;

    private float _gravity = -9.81f;
    private float _verticalVelocity;

    public WeaponSO CurrentWeapon;
    public WeaponHitbox Hitbox;
    [HideInInspector] public int CurrentComboStep = 0;

    public WeaponVisualManager VisualManager;

    [Header("Input")]
    public LayerMask GroundLayer;

    [Header("이동 및 회피")]
    [Tooltip("회피 후 다음 회피를 할 수 있을 때까지의 최소 지연 시간")]
    public float dashCooldown = 0.2f;
    private float _dashTimer = 0f;
    [Tooltip("회피 후 평타 공격을 할 수 있을 때까지의 지연 시간")]
    public float postRollAttackDelay = 0.3f;
    [HideInInspector] public float postRollAttackTimer = 0f;

    [Header("전투 감지 센서")]
    public float DetectionRadius = 8.0f;   
    [Range(0, 360)]
    public float ViewAngle = 120.0f;     
    public LayerMask EnemyLayer;           
    public float CombatCooldown = 5.0f;

    [Header("Effect && Pos")]
    public GameObject HealEffect;
    public GameObject GreateSwordEffect;
    public Transform bodyEffectPos;
    private GameObject currentRSkillInstance;

    [Header("카메라 스크린 이펙트")]
    public ScreenBloodController screenBloodEffect;
    public GameObject screenFireEffect;

    [Header("아이템 관리")]
    public ShopPotionSO currentPotionData;          // 장착된 포션 SO
    public int currentPotionCount;                  // 현재 인벤토리에 남은 포션 개수
    private GameObject currentHealInstance;
    private Coroutine _healEffectCoroutine;

    public float potionCooldown = 5.0f;
    public float Potion_Timer { get; private set; }

    public event Action<int> OnPotionCountChanged;

    [Header("Action Effects (현재 무기의 액션 이펙트 모음)")]
    public List<ActionEffectMapping> weaponEffects = new List<ActionEffectMapping>();
    private Dictionary<string, ParticleSystem[]> effectDictionary = new Dictionary<string, ParticleSystem[]>();

    private float _combatTimer;
    private bool _inCombat;
    public TutorialSystem tutorialSystem;
    SkillTutorial skillTutorial;
    public bool InCombat => _inCombat;

    [Header("스킬 관리")]
    [HideInInspector] public float AtkSpeedModifier = 1.0f;
    [HideInInspector] public string CurrentSkillAnim;   // 어떤 스킬 애니메이션을 틀지
    [HideInInspector] public int CurrentSkillDamage;    // 현재 스킬 데미지가 얼마인지
    [HideInInspector] public float CurrentSkillVal;
    [Tooltip("스킬 종료 후 다음 스킬을 쓸 수 있을 때까지의 대기 시간")]
    public float globalSkillDelay = 0.5f;
    [HideInInspector] public float globalSkillTimer = 0f;

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
        RB = GetComponent<Rigidbody>();
        skillTutorial = FindFirstObjectByType<SkillTutorial>();

        Stats.OnHit += HandleHit;
        Stats.OnDeath += HandleDeath;
        Stats.OnStun += HandleStun;
        Stats.OnHPChanged += CheckDangerState;

        foreach (var mapping in weaponEffects)
        {
            if (!effectDictionary.ContainsKey(mapping.actionCode))
            {
                effectDictionary.Add(mapping.actionCode, mapping.particles);
            }
        }
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

        if (currentPotionData != null)
        {
            currentPotionCount = currentPotionData.Max_Count;
            OnPotionCountChanged?.Invoke(currentPotionCount);
        }
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
        CheckItemInput();
    }

    private void LateUpdate()
    {
        if (Agent != null && Agent.enabled && Agent.isOnNavMesh)
        {
            Agent.nextPosition = transform.position;
        }

        if (transform.position.y < -2f)
        {
            _verticalVelocity = 0f;

            if (NavMesh.SamplePosition(transform.position + (Vector3.up * 10f), out NavMeshHit hit, 20f, NavMesh.AllAreas))
            {
                if (CC != null) CC.enabled = false;
                transform.position = hit.position;
                if (CC != null) CC.enabled = true;
            }
        }
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
            Stats.OnHPChanged -= CheckDangerState;
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
        if (screenBloodEffect != null)
        {
            screenBloodEffect.PlayHitEffect(1.5f); // 1.5초 동안 피 튀김
        }

        if (StateMachine.CurrentState == StunState || StateMachine.CurrentState == DeadState) return;
        if (HasRBuff) return;

        if (severity <= 0f) return;

        HitState.SetSeverity(severity);
        StateMachine.ChangeState(HitState);
    }

    private void CheckDangerState(int currentHP, int maxHP)
    {
        if (screenBloodEffect == null) return;

        float hpPercent = (float)currentHP / maxHP;

        // 체력이 20% 이하이고 살아있으면 Danger 모드 ON, 아니면 OFF
        bool isDanger = (hpPercent <= 0.2f && currentHP > 0);
        screenBloodEffect.SetDangerMode(isDanger);
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
        if (StateMachine.CurrentState == DeadState ||
            StateMachine.CurrentState == StunState ||
            StateMachine.CurrentState == HitState ||
            StateMachine.CurrentState == AttackState ||
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
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 targetPoint = ray.GetPoint(rayDistance);
            if (Vector3.Distance(transform.position, targetPoint) > 0.5f)
            {
                Vector3 dir = (targetPoint - transform.position).normalized;
                dir.y = 0;

                if (dir != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(dir);
                }
            }
        }
    }

    public bool CheckSkillAndDashInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Stats.CurrentStamina >= Stats.DashCost)
            {
                if (_dashTimer <= 0)
                {
                    StateMachine.ChangeState(RollState);
                    return true;
                }
                else
                {
                    Debug.Log($"<color=orange>[회피 불가] 쿨타임 중입니다! 남은 시간: {_dashTimer:F2}초</color>");
                }
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

    public void ResetDashTimer()
    {
        _dashTimer = dashCooldown;
    }

    #region 공격 판정
    public void EnableWeaponCollider()
    {
        if (StateMachine.CurrentState != AttackState 
            && StateMachine.CurrentState != SkillState) return;

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
            // 데미지를 CurrentSkillVal 배율만큼 곱해줍니다.
            damageToDeal = Mathf.RoundToInt(damageToDeal * CurrentSkillVal);
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

    // 특정 공격 모션에서 이펙트 끄기 (애니메이션 이벤트용)
    public void DisableREffect()
    {
        if (HasRBuff) return;

        if (currentRSkillInstance != null)
        {
            currentRSkillInstance.SetActive(false);
        }

        if (screenFireEffect != null) screenFireEffect.SetActive(false);
    }

    public void EnableREffect()
    {
        if (GreateSwordEffect == null || bodyEffectPos == null)
        {
            Debug.LogWarning("[PlayerController] R스킬 아우라 프리팹 또는 생성 위치가 할당되지 않았습니다!");
            return;
        }

        currentRSkillInstance = Instantiate(GreateSwordEffect, bodyEffectPos.position, Quaternion.identity, bodyEffectPos);

        currentRSkillInstance.SetActive(true);

        if (screenFireEffect != null) screenFireEffect.SetActive(true);
    }

    #endregion

    #region 스킬 관련
    private void UpdateSkillCooldowns()
    {
        if (_dashTimer > 0) _dashTimer -= Time.deltaTime;
        if (globalSkillTimer > 0) globalSkillTimer -= Time.deltaTime;
        if (postRollAttackTimer > 0) postRollAttackTimer -= Time.deltaTime;

        if (Potion_Timer > 0)
        {
            Potion_Timer -= Time.deltaTime;
            if (Potion_Timer <= 0) Debug.Log("포션 쿨타임이 끝났습니다!");
        }

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
        if (globalSkillTimer > 0f)
        {
            Debug.Log($"스킬 전환 딜레이 중입니다! ({globalSkillTimer:F1}초 남음)");
            return false;
        }

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

    #region 액션 이펙트 제어 (Refactored)
    public void EnableActionEffect(string actionCode)
    {
        if (StateMachine.CurrentState != AttackState && StateMachine.CurrentState != SkillState) return;

        if (effectDictionary.TryGetValue(actionCode, out ParticleSystem[] particles))
        {
            foreach (var p in particles)
            {
                PlayParticle(p);
            }
        }
        else
        {
            Debug.LogWarning($"[Effect] '{actionCode}' 이펙트가 리스트에 등록되지 않았습니다!");
        }
    }

    public void DisableActionEffect(string actionCode)
    {
        if (effectDictionary.TryGetValue(actionCode, out ParticleSystem[] particles))
        {
            foreach (var p in particles)
            {
                StopParticle(p);
            }
        }
    }

    public void ForceDisableAllActionEffects()
    {
        foreach (var particles in effectDictionary.Values)
        {
            foreach (var p in particles)
            {
                if (p != null)
                {
                    p.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    p.gameObject.SetActive(false);
                }
            }
        }
    }

    private void PlayParticle(ParticleSystem effect)
    {
        if (effect == null) return;
        if (!effect.gameObject.activeSelf) effect.gameObject.SetActive(true);
        effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var mainModule = effect.main;
        mainModule.simulationSpeed = Anim.speed;
        effect.Play(true);
    }

    private void StopParticle(ParticleSystem effect)
    {
        if (effect != null)
        {
            effect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    #endregion

    #region 아이템 사용
    private void CheckItemInput()
    {
        if (StateMachine.CurrentState == DeadState || StateMachine.CurrentState == StunState) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UsePotion();
        }
    }

    private void UsePotion()
    {
        if (currentPotionData == null) return;
        if (currentPotionCount <= 0) return;
        if (Stats.IsFullHP) return;
        if (Potion_Timer > 0) return;

        float percentRatio = currentPotionData.Heal_Percent / 100f;
        int healAmount = Mathf.RoundToInt(Stats.MaxHP * percentRatio);

        Stats.Heal(healAmount);
        currentPotionCount--;
        OnPotionCountChanged?.Invoke(currentPotionCount);
        Potion_Timer = potionCooldown;

        PlayHealEffect();
    }

    private void PlayHealEffect()
    {
        if (HealEffect == null || bodyEffectPos == null) return;

        if (currentHealInstance == null)
            currentHealInstance = Instantiate(HealEffect, bodyEffectPos.position, Quaternion.identity, bodyEffectPos);

        currentHealInstance.SetActive(false);
        currentHealInstance.SetActive(true);

        ParticleSystem[] particles = currentHealInstance.GetComponentsInChildren<ParticleSystem>();
        foreach (var p in particles)
        {
            p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            p.Play(true);
        }

        if (_healEffectCoroutine != null) StopCoroutine(_healEffectCoroutine);

        _healEffectCoroutine = StartCoroutine(DisableHealEffectAfterDelay(2.0f));
    }

    private System.Collections.IEnumerator DisableHealEffectAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (currentHealInstance != null)
        {
            currentHealInstance.SetActive(false);
            _healEffectCoroutine = null;
        }
    }

    #endregion

    #region 부활 처리 (리스폰)
    public void Revive(Transform spawnPoint)
    {
        if (CC != null) CC.enabled = false;
        if (RB != null)
        {
            RB.isKinematic = false;
            RB.useGravity = true;
        }

        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }

        if (CC != null) CC.enabled = true;

        Stats.InitActor(Stats.MaxHP);
        if (currentPotionData != null)
        {
            currentPotionCount = currentPotionData.Max_Count;
            OnPotionCountChanged?.Invoke(currentPotionCount);
        }

        Anim.ResetTrigger("IsDead");
        Anim.ResetTrigger("IsHit");
        Anim.ResetTrigger("IsStun");
        Anim.ResetTrigger("Attack");
        Anim.ResetTrigger("Roll");
        Anim.ResetTrigger("Skill_Q");
        Anim.ResetTrigger("Skill_W");
        Anim.ResetTrigger("Skill_E");
        Anim.ResetTrigger("Skill_R");
        Anim.ResetTrigger("Skill_V");

        Anim.SetBool("InCombat", false);
        _inCombat = false;
        Anim.SetFloat("Move", 0f);
        Anim.SetFloat("HitPower", 0f);

        Anim.Rebind();
        Anim.Update(0f);

        Anim.SetLayerWeight(1, 0f);
        Anim.Play("Idle", 0, 0f);

        StateMachine.ChangeState(IdleState);
        ForceDisableAllActionEffects();
    }

    #endregion
}
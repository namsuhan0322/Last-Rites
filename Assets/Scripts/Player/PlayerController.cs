using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

/// <summary>
/// [역할] 플레이어 허브: 인스펙터 설정·참조, 상태 머신, Unity 콜백, 애니메이션 이벤트 창구,
///        바깥 공개 함수(Revive·ChangeWeapon·SetBossModeOutline·TryBlockDamageWithShield), 상태 프로퍼티.
///        실제 일은 규칙(Player/Rules)과 부품(Player/Parts)에 맡긴다.
/// [이유] 1,283줄에 모든 기능이 섞여 있던 것을 나눴다. 인스펙터 필드와 이벤트 함수는
///        프리팹·씬·애니메이션이 이름으로 찾으므로 여기 그대로 둔다.
/// [근거] 명세 3.1, 결정 R4, R9, R27, 조작감 명세 3.1
/// </summary>
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

    #region 규칙 (Player/Rules — 엔진 없이 테스트됨, 명세 3.2)
    private readonly SkillCooldowns _cooldowns = new SkillCooldowns();
    private readonly CombatTimer _combat = new CombatTimer();
    private readonly WizardShieldCounter _shield = new WizardShieldCounter();
    private readonly PotionPouch _potion = new PotionPouch();
    private readonly ActionInputBuffer _inputBuffer = new ActionInputBuffer();   // 선입력 한 칸 (조작감 C5·C7)
    #endregion

    #region 부품 (Player/Parts — 엔진 쪽 일반 클래스, 명세 3.3)
    public PlayerMotor Motor { get; private set; }
    private PlayerActionEffects _effects;
    private WizardCaster _wizard;
    private PlayerAttack _attack;
    private PlayerWeaponLoadout _loadout;
    private CombatSensor _sensor;
    private TutorialGate _tutorial;
    private PlayerSkillInput _skillInput;
    private PlayerUpperBody _upperBody;
    public ActionCancelPoints CancelPoints { get; private set; }   // 지금 클립의 타격 끝 시점 (조작감 C3·C5)
    #endregion

    #region Components & Settings
    public NavMeshAgent Agent { get; private set; }
    public CharacterController CC { get; private set; }
    public PlayerStats Stats { get; private set; }
    public Animator Anim { get; private set; }
    public Rigidbody RB { get; private set; }

    [Header("무기 데이터베이스")]
    public WeaponDatabaseSO weaponDatabase;

    [Header("스탯")]
    [SerializeField] public float RotateSpeed = 10f;
    [SerializeField] public float AnimationSmoothTime = 0.1f;

    public WeaponSO CurrentWeapon;
    public WeaponHitbox Hitbox;

    public WeaponVisualManager VisualManager;

    [Header("Input")]
    public LayerMask GroundLayer;

    [Header("조작감 (로스트아크식 선입력·후딜 캔슬)")]
    [Tooltip("평타 콤보가 다음 타로 넘어가는 시점. 0 = 이번 타격이 끝나는 순간(가장 빠름), 1 = 예전처럼 공격 애니 95%")]
    [Range(0f, 1f)] public float comboChainDelay = 0.4f;   // 조작감 결정 C5, C11(테스트 후 추가: 체크박스 → 슬라이더, 0.4는 사용자가 Play 중 맞춘 값)
    public ActionInputBuffer InputBuffer => _inputBuffer;

    [Header("이동 및 회피")]
    [Tooltip("회피 후 다음 회피를 할 수 있을 때까지의 최소 지연 시간")]
    public float dashCooldown = 10f;
    public float DashTimer => _cooldowns.Dash;
    [Tooltip("회피 후 평타 공격을 할 수 있을 때까지의 지연 시간")]
    public float postRollAttackDelay = 0.3f;
    // 이름은 바깥 코드 호환을 위해 그대로 (결정 R7). 지금 코드에서는 0 말고 넣는 곳이 없음 (Q3)
    public float postRollAttackTimer { get => _cooldowns.PostRollAttack; set => _cooldowns.PostRollAttack = value; }

    [Header("전투 감지 센서")]
    public float DetectionRadius = 8.0f;   
    [Range(0, 360)]
    public float ViewAngle = 120.0f;     
    public LayerMask EnemyLayer;           
    public float CombatCooldown = 5.0f;

    [Header("데코레이터 무기")]
    public ISword CurrentDecoratedWeapon;

    [Header("Effect && Pos")]
    public GameObject HealEffect;
    public Transform bodyEffectPos;

    [Header("무기별 특수(R) 이펙트 매핑")]
    public List<WeaponEffectMapping> weaponSpecialEffects = new List<WeaponEffectMapping>();

    [Header("낙하형(AoE) 스킬")]
    public GameObject excaliburSkillPrefab;
    public float spawnDistance = 5f;

    [Header("카메라 스크린 이펙트")]
    public ScreenBloodController screenBloodEffect;

    [Header("아이템 관리")]
    public ShopPotionSO currentPotionData;          // 장착된 포션 SO
    public int currentPotionCount => _potion.Count; // 현재 인벤토리에 남은 포션 개수

    public float potionCooldown = 5.0f;
    public float Potion_Timer => _potion.CooldownRemaining;

    public event Action<int> OnPotionCountChanged;

    [Header("무기 별 투사체")]
    public GameObject spearBuffProjectilePrefab;
    public GameObject wizardVfxProjectilePrefab;

    [Header("Action Effects (현재 무기의 액션 이펙트 모음)")]
    public List<ActionEffectMapping> weaponEffects = new List<ActionEffectMapping>();

    public TutorialSystem tutorialSystem;
    SkillTutorial skillTutorial;
    public bool InCombat => _combat.InCombat;

    [Header("스킬 관리")]
    [HideInInspector] public float AtkSpeedModifier = 1.0f;
    [HideInInspector] public string CurrentSkillAnim;   // 어떤 스킬 애니메이션을 틀지
    [HideInInspector] public int CurrentSkillDamage;    // 현재 스킬 데미지가 얼마인지
    [HideInInspector] public float CurrentSkillVal;
    [Tooltip("스킬 종료 후 다음 스킬을 쓸 수 있을 때까지의 대기 시간")]
    public float globalSkillDelay = 0.5f;
    public float globalSkillTimer { get => _cooldowns.GlobalSkill; set => _cooldowns.GlobalSkill = value; }

    [Header("무기 특수 버프 플래그")]
    [FormerlySerializedAs("HasRBuff")]                      // 프리팹에 저장된 옛 이름과 연결 (값은 false, 결정 R17)
    [HideInInspector] public bool HasGreatSwordBuff = false; // 대검 R (슈퍼아머 + 데미지 × R값)
    [HideInInspector] public bool HasTwinBuff = false;      // 쌍검 (스태미나 감소)
    [HideInInspector] public bool HasSpearBuff = false;     // 창 (방어력 관통)
    [HideInInspector] public bool HasShieldBuff = false;    // 검방 (피해 70% 감소/반격)

    [Header("마법사 스킬 파티클 프리팹 (Q, W, E, V)")]
    public GameObject wizardSkillQPrefab;
    public GameObject wizardSkillWPrefab;
    public GameObject wizardSkillEPrefab;
    public GameObject wizardSkillVPrefab;

    public bool isWizardShieldActive => _shield.IsActive;
    public int wizardShieldRemainingHits => _shield.RemainingHits;
    [Header("마법사 보호막 세팅")]
    public int wizardShieldMaxHits = 3;

    [SerializeField] private Transform wizardQSpawnPoint;
    [SerializeField] private Transform wizardWSpawnPoint;
    [SerializeField] private Transform wizardESpawnPoint;
    [SerializeField] private Transform wizardVSpawnPoint;

    // WizardCaster가 읽는 통로 (필드는 인스펙터·씬 덮어쓰기 유지를 위해 그대로 둔다, 결정 R27)
    internal Transform WizardQSpawnPoint => wizardQSpawnPoint;
    internal Transform WizardWSpawnPoint => wizardWSpawnPoint;
    internal Transform WizardESpawnPoint => wizardESpawnPoint;
    internal Transform WizardVSpawnPoint => wizardVSpawnPoint;

    public float Q_Timer => _cooldowns.Get(SkillSlot.Q);
    public float W_Timer => _cooldowns.Get(SkillSlot.W);
    public float E_Timer => _cooldowns.Get(SkillSlot.E);
    public float R_Timer => _cooldowns.Get(SkillSlot.R);
    public float V_Timer => _cooldowns.Get(SkillSlot.V);
    public float GetSkillTimer(SkillSlot slot) => _cooldowns.Get(slot);   // UICooldown 반복문용 (R31)

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

        _tutorial = new TutorialGate(this);
        Motor = new PlayerMotor(this);
        _effects = new PlayerActionEffects(this);
        _wizard = new WizardCaster(this, _shield);
        _attack = new PlayerAttack(this, _wizard);
        _loadout = new PlayerWeaponLoadout(this);
        _sensor = new CombatSensor(this, _combat, _tutorial);
        _skillInput = new PlayerSkillInput(this, _tutorial, skillTutorial, _inputBuffer);
        CancelPoints = new ActionCancelPoints(this);
        _upperBody = new PlayerUpperBody(this);
    }

    private void Start()
    {
        Agent.updatePosition = false;
        Agent.updateRotation = false;
        Agent.speed = Stats.MoveSpeed;

        _loadout.LoadEquipped();

        StateMachine.Initialize(IdleState);

        if (currentPotionData != null)
        {
            _potion.Refill(currentPotionData.Max_Count);
            OnPotionCountChanged?.Invoke(currentPotionCount);
        }
    }

    private void Update()
    {
        if (_tutorial.IsFrozen)
        {
            Agent.ResetPath();
            Anim.SetFloat(PlayerAnimParams.Move, 0f);
            return;
        }

        // 선입력 모으기: 상태 처리 전에 이번 프레임에 누른 키를 한 칸에 담는다. 피격·스턴·사망 중에는 모으지 않고 비운다 (C7⑤)
        if (StateMachine.CurrentState.BlocksInputBuffer) _inputBuffer.Clear();
        else _skillInput.Collect();

        StateMachine.CurrentState.HandleInput();
        StateMachine.CurrentState.LogicUpdate();
        StateMachine.CurrentState.PhysicsUpdate();

        _sensor.Tick();
        _upperBody.Tick();
        UpdateSkillCooldowns();
        CheckItemInput();
    }

    private void LateUpdate()
    {
        Motor.SyncAgentAndRecoverFall();
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

    // 상태 성격 묻기. 상태가 아직 없으면(Start 전) '아니오' — 예전 CurrentState == … 비교와 같은 결과 (R29)
    private bool StateCanDealDamage => StateMachine.CurrentState != null && StateMachine.CurrentState.CanDealDamage;
    private bool StateIsIncapacitated => StateMachine.CurrentState != null && StateMachine.CurrentState.IsIncapacitated;
    private bool StateUsesRootMotion => StateMachine.CurrentState != null && StateMachine.CurrentState.UsesRootMotion;

    private void OnAnimatorMove()
    {
        if (StateUsesRootMotion) Motor.ApplyRollRootMotion();
    }

    private void HandleHit(float severity)
    {
        _effects.PlayHitScreen();   // 1.5초 동안 피 튀김

        if (StateIsIncapacitated) return;   // 스턴·사망 중에는 경직 반응 없음
        if (HasGreatSwordBuff) return;

        if (severity <= 0f) return;

        HitState.SetSeverity(severity);
        StateMachine.ChangeState(HitState);
    }

    // 체력이 20% 이하이고 살아있으면 Danger 모드 ON, 아니면 OFF
    private void CheckDangerState(int currentHP, int maxHP) => _effects.UpdateDangerScreen(currentHP, maxHP);

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
        if (StateIsIncapacitated) return;
        if (HasGreatSwordBuff) return;

        Debug.Log("플레이어가 스턴에 걸림!");
        StateMachine.ChangeState(StunState);
    }

    // 선입력의 회피·스킬 실행 → PlayerSkillInput (상태 4곳이 부름). 상태가 회피·스킬로 바뀌었으면 true (조작감 C7)
    public bool TryRunBufferedSkillOrDodge() => _skillInput.TryRunBuffered();

    public void ResetDashTimer()
    {
        _cooldowns.StartDash(dashCooldown);
    }

    #region 공격 판정
    // 애니메이션 이벤트 창구 → PlayerAttack. 공격·스킬 상태에서만 히트를 켠다
    // AnimationEvent를 받는 이유: 넘어가는 도중에도 이 이벤트를 낸 애니의 상태로 공식을 고르려고 (Q11 고침, 버그 수정 B8)
    public void EnableWeaponCollider(AnimationEvent evt)
    {
        if (!StateCanDealDamage) return;   // 공격·스킬 상태에서만

        // Animator가 낸 이벤트면 그 이벤트의 상태, 아니면(안전장치) 예전처럼 지금 상태
        AnimatorStateInfo hitState = evt != null && evt.isFiredByAnimator
            ? evt.animatorStateInfo
            : Anim.GetCurrentAnimatorStateInfo(PlayerAnimParams.BaseLayer);
        _attack.EnableHit(hitState);
    }

    // 공격 종료 시 호출
    public void DisableWeaponCollider() => _attack.DisableHit();

    // 특정 공격 모션에서 이펙트 끄기 (애니메이션 이벤트용)
    public void DisableREffect()
    {
        if (HasGreatSwordBuff || HasSpearBuff) return;
        _effects.DisableR();
    }

    public void EnableREffect() => _effects.EnableR();

    public void RefreshDecoratedWeapon() => _loadout.RefreshDecorated();
    #endregion

    #region 스킬 관련
    private void UpdateSkillCooldowns()
    {
        float dt = Time.deltaTime;
        // 회피·공통·회피후 → 포션 → Q·W·E·R·V (예전 "쿨타임 끝" 로그 순서. 로그는 3단계에서 지움, R34)
        _cooldowns.TickGeneral(dt);
        _potion.Tick(dt);
        _cooldowns.TickSkills(dt);
    }

    public bool TryUseSkill(KeyCode key, string animName, int damage, float maxCool, int staminaCost)
    {
        // 쿨타임 칸이 있는 키는 Q/W/E/V. 그 밖의 키(R 포함)는 실패 (기록도 안 함, B9)
        bool hasSlot = TryGetSkillSlot(key, out SkillSlot slot) && slot != SkillSlot.R;
        SkillUseResult result = SkillUseRule.Evaluate(true, globalSkillTimer, Stats.CurrentStamina, staminaCost,
            hasSlot && _cooldowns.IsReady(slot));

        if (SkillUseRule.RecordsSkillInfo(result))
        {
            CurrentSkillAnim = animName;
            CurrentSkillDamage = damage;
        }
        if (result != SkillUseResult.Ready) return false;

        _cooldowns.Start(slot, maxCool);
        Stats.UseStamina(staminaCost);
        return true;
    }

    public bool TryUse_RSkill(KeyCode key, string animName, float val, float maxCool, int staminaCost)
    {
        // R은 공통 딜레이를 보지 않는다 (지금 코드). R 키가 아니면 실패 (기록도 안 함, B9)
        bool isR = key == KeyCode.R;
        SkillUseResult result = SkillUseRule.Evaluate(false, 0f, Stats.CurrentStamina, staminaCost,
            isR && _cooldowns.IsReady(SkillSlot.R));

        if (SkillUseRule.RecordsSkillInfo(result))
        {
            CurrentSkillAnim = animName;
            CurrentSkillVal = val;
        }
        if (result != SkillUseResult.Ready) return false;

        _cooldowns.Start(SkillSlot.R, maxCool);
        Stats.UseStamina(staminaCost);
        if (CurrentWeapon != null && CurrentWeapon.R_Skill_Logic != null)
        {
            CurrentWeapon.R_Skill_Logic.Execute(this, val);
        }
        return true;
    }

    private static bool TryGetSkillSlot(KeyCode key, out SkillSlot slot)
    {
        switch (key)
        {
            case KeyCode.Q: slot = SkillSlot.Q; return true;
            case KeyCode.W: slot = SkillSlot.W; return true;
            case KeyCode.E: slot = SkillSlot.E; return true;
            case KeyCode.R: slot = SkillSlot.R; return true;
            case KeyCode.V: slot = SkillSlot.V; return true;
            default: slot = SkillSlot.Q; return false;
        }
    }

    // 창 V 애니메이션 이벤트 창구 → PlayerAttack
    public void UseExcaliburSkill() => _attack.FireExcalibur();
    #endregion

    #region 무기 장착, 해제 이벤트 
    // 칼집에서 칼을 뽑는 애니메이션 도중 손이 자루에 닿을 때 호출
    public void OnWeaponDrawn()
    {
        if (VisualManager != null)
            VisualManager.DrawWeapon();

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
    // 감지는 CombatSensor.Tick (Update에서 부름). 기즈모는 플레이 전에도 그려야 해서 static 함수를 부른다
    private void OnDrawGizmosSelected()
    {
        CombatSensor.DrawGizmos(transform, DetectionRadius, ViewAngle);
    }

    #endregion

    #region 아웃라인 제어
    public void SetBossModeOutline(bool isBossMode)
    {
        int targetLayer = isBossMode ? LayerMask.NameToLayer("PlayerBossOutline") : LayerMask.NameToLayer("Player");

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
        if (!StateCanDealDamage) return;   // 공격·스킬 상태에서만
        _effects.Play(actionCode);
    }

    public void DisableActionEffect(string actionCode) => _effects.Stop(actionCode);

    public void ForceDisableAllActionEffects() => _effects.StopAll();

    #endregion

    #region 아이템 사용
    private void CheckItemInput()
    {
        if (StateIsIncapacitated) return;   // 스턴·사망 중에는 포션 못 씀

        if (PlayerInputReader.PotionPressed)
        {
            UsePotion();
        }
    }

    private void UsePotion()
    {
        // 조건 4가지와 회복량 공식은 PotionPouch (명세 3.2)
        if (!_potion.CanUse(currentPotionData != null, Stats.IsFullHP)) return;

        int healAmount = PotionPouch.HealAmount(Stats.MaxHP, currentPotionData.Heal_Percent);

        Stats.Heal(healAmount);
        _potion.TakeOne();
        OnPotionCountChanged?.Invoke(currentPotionCount);
        _potion.StartCooldown(potionCooldown);

        _effects.PlayHeal();
    }

    #endregion

    #region 부활 처리 (리스폰)
    public void Revive(Vector3 spawnPosition)
    {
        if (CC != null) CC.enabled = false;
        if (RB != null)
        {
            RB.isKinematic = true;
            RB.useGravity = false;
        }

        transform.position = spawnPosition;

        if (CC != null) CC.enabled = true;

        Stats.InitActor(Stats.MaxHP);
        if (currentPotionData != null)
        {
            _potion.Refill(currentPotionData.Max_Count);
            OnPotionCountChanged?.Invoke(currentPotionCount);
        }

        // 스킬 5칸·회피·공통 딜레이·회피 후 딜레이 + 포션 쿨 모두 0 (지금 코드와 같은 범위)
        _cooldowns.ResetAll();
        _potion.ResetCooldown();
        _inputBuffer.Clear();   // 부활 때 선입력 비움 (조작감 C7⑤)
        HasGreatSwordBuff = false;

        DisableREffect();
        DisableWeaponCollider();
        ForceDisableAllActionEffects();

        Anim.ResetTrigger(PlayerAnimParams.IsDead);
        Anim.ResetTrigger(PlayerAnimParams.IsHit);
        Anim.ResetTrigger(PlayerAnimParams.IsStun);
        Anim.ResetTrigger(PlayerAnimParams.Attack);
        Anim.ResetTrigger(PlayerAnimParams.Roll);
        foreach (string skillTrigger in PlayerAnimParams.SkillTriggers) Anim.ResetTrigger(skillTrigger);

        Anim.SetBool(PlayerAnimParams.InCombat, false);
        _combat.TurnOffKeepTimer();
        Anim.SetFloat(PlayerAnimParams.Move, 0f);
        Anim.SetFloat(PlayerAnimParams.HitPower, 0f);

        Anim.Rebind();
        Anim.Update(0f);

        Anim.SetLayerWeight(PlayerAnimParams.UpperBodyLayer, 0f);
        Anim.Play(PlayerAnimParams.StateIdleMove, PlayerAnimParams.BaseLayer, 0f);   // 기본 상태 처음부터 (예전 "Idle"은 없는 이름이라 경고, Q13 고침)

        StateMachine.ChangeState(IdleState);
    }

    #endregion

    #region 무기 교체 저장
    public void ChangeWeapon(WeaponSO newWeapon)
    {
        if (newWeapon == null || CurrentWeapon == newWeapon) return;

        _loadout.Apply(newWeapon);
        _cooldowns.ResetSkills();
        _loadout.SaveEquipped(newWeapon);
    }

    #endregion

    #region 마법사 스킬 (애니메이션 이벤트 창구 → WizardCaster)
    public void FireWizardSkill_Q() => _wizard.FireQ();
    public void FireWizardSkill_W() => _wizard.FireW();
    public void FireWizardSkill_E() => _wizard.FireE();
    public void FireWizardSkill_V() => _wizard.FireV();
    #endregion

    #region 마법사 보호막(E) 방어 (Actor가 부름 → WizardCaster)
    public bool TryBlockDamageWithShield() => _wizard.TryBlockDamage();
    public void BreakWizardShield() => _wizard.BreakShield();
    #endregion

    #region 사운드
    public void PlayWeaponSwingSound(string soundID)
    {
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlaySFX(soundID, transform.position);
        }
    }
    #endregion
}
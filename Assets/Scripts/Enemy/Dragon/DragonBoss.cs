using UnityEngine;
using UnityEngine.AI;
using System.Collections;
public class DragonBoss : Enemy
{
    [Header("2페이즈")]
    [SerializeField] private float phase2HpRate = 0.4f;

    [Header("테스트")]
    public bool testMeteorOnly = false;

    [Header("스킬후 공통 현타시간")]
    public float combatIdleTime = 2f;

    [Header("용 움직임")]
    public float turnSpeed = 80f;
    public float turnDuration = 1.2f;

    [Header("타겟")]
    public float playerDetectRange = 15f;
    public float targetLoseRange = 25f;
    public float faceFinishAngle = 5f;
    public float faceCooldown = 1.2f;

    [Header("포효")]
    public float roarDuration = 2.5f;

    private int currentMoveType = -1;
    private Transform lockedTarget;
    private float faceCooldownTimer = 0f;
    private bool hasRoared = false;


    [Header("전방 깨물기 패턴")]
    public float biteRange = 4f;
    public float biteDuration = 1.5f;
    public float biteCooldown = 2f;
    [SerializeField] private DragonAttackHitbox headHitbox;

    [Header("날개 슬램 패턴")]
    public float wingSlamRange = 7f;
    public float wingSlamDuration = 2f;
    public float wingSlamCooldown = 8f;
    [Header("날개 히트박스")]
    [SerializeField] private DragonAttackHitbox leftWingHitbox;
    [SerializeField] private DragonAttackHitbox rightWingHitbox;

    [Header("꼬리 공격 패턴")]
    public float tailAttackRange = 8f;
    public float tailAttackDuration = 2f;
    public float tailAttackCooldown = 7f;
    public float tailBackAngle = 120f;
    [Header("꼬리 히트박스")]
    [SerializeField] private DragonAttackHitbox leftTailHitbox;
    [SerializeField] private DragonAttackHitbox rightTailHitbox;

    [Header("파이어볼 패턴")]
    public float fireballMinRange = 8f;
    public float fireballMaxRange = 25f;
    public float fireballDuration = 2f;
    public float fireballCooldown = 6f;
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform fireballSpawnPoint;

    [Header("강화 날개 스윙 콤보")]
    public float wingComboDuration = 3.5f;
    public float rightWingStartDelay = 1.4f;
    public float wingComboCooldown = 12f;

    [Header("점프 공격 패턴")]
    public float jumpAttackCooldown = 15f;
    public float jumpUpHeight = 8f;
    public float jumpUpTime = 1f;
    public float skyLoopTime = 1.5f;
    public float warningShowTime = 1f;
    public float fallTime = 0.8f;
    public float jumpDamageRadius = 5f;
    public int jumpDamage = 40;
    public float fallAnimDelay = 0.25f;
    public float afterWarningDelay = 0.5f; //점프 공격 알려주고 난 다음 타임
    [SerializeField] private GameObject jumpWarningPrefab;

    [Header("날개 내려찍기 + 브레스 콤보")]
    public float wingCrushBreathDuration = 5f;
    public float secondCrushDelay = 1.3f;
    public float breathStartDelay = 2.8f;
    public float wingCrushBreathCooldown = 12f;
    [Header("정면 브레스")]
    public float fireBreathDuration = 4.5f;
    public GameObject breathPrefab;
    public Transform breathSpawnPoint;
    [Header("날개 크러쉬 삼각형 판정")]
    [SerializeField] private Transform leftWingCrushPoint;
    [SerializeField] private Transform rightWingCrushPoint;
    public float wingCrushTriangleLength = 8f;
    public float wingCrushTriangleWidth = 6f;
    public int wingCrushDamage = 40;
    [Header("날개 크러쉬 이펙트")]
    [SerializeField] private GameObject leftWingCrushEffectPrefab;
    [SerializeField] private GameObject rightWingCrushEffectPrefab;

    public float wingCrushEffectScale = 2f;
    public float wingCrushEffectSpeed = 0.7f;
    public float wingCrushEffectLifeTime = 3f;

    [Header("날개 스윙 + 물기 콤보")]
    public float wingSlamBiteComboDuration = 6f;
    public float secondWingSlamDelay = 1.3f;
    public float biteAfterWingDelay = 2.8f;
    public float wingSlamBiteComboCooldown = 12f;

    [Header("날개 스윙 + 물기 + 좌우 브레스 콤보")]
    public float wingSlamBiteSideBreathComboDuration = 11f;
    public float sideBreathStartDelay = 7f;
    public float wingSlamBiteSideBreathComboCooldown = 18f;
    [Header("좌우 브레스")]
    public float sideBreathDuration = 4.5f;

    [Header("좌우 점프 이동 콤보 스킬")]
    public float sideJumpDuration = 1.2f;
    public float sideJumpCooldown = 8f;



    [Header("브레스 중 헤드약점")]
    [SerializeField] private WeakPoint headWeakPoint;
    [SerializeField] private int headWeakPointHP = 100;
    [Header("브레스 차징 중 날개 약점")]
    [SerializeField] private WeakPoint leftWingWeakPoint;
    [SerializeField] private WeakPoint rightWingWeakPoint;
    [SerializeField] private int wingWeakPointHP = 100;
    [Header("브레스 차징 패턴")]
    public float breathCastTime = 3.5f;
    public float breathChargeTime = 2.5f;
    public float breathChargeCooldown = 12f;
    public GameObject breathChargePrefab;
    [Header("브레스 차징 실패 시 광역 데미지")]
    public float breathChargeExplosionRadius = 100f;
    public int breathChargeExplosionDamage = 80;
    [Header("브레스 차징 FX")]
    public float breathChargeFxLifeTime = 2f;
    [Header("부위파괴")]
    public float weakPointBreakAnimTime = 3f;
    [Header("1페이즈 브레스 차지 체력 이벤트")]
    [SerializeField] private float breathChargeHp1 = 0.85f;
    [SerializeField] private float breathChargeHp2 = 0.70f;
    [SerializeField] private float breathChargeHp3 = 0.55f;
    [SerializeField] private int normalBreathChargeDamage = 80;
    [SerializeField] private int finalBreathChargeDamage = 150;

    [Header("돌진 패턴")]
    public float chargeCooldown = 10f;
    public float chargeDistance = 10f;
    public float chargeReadyTime = 2.5f;
    public float chargeStartDelay = 0.8f;
    public float chargeDuration = 1.5f;
    public int chargeDamage = 45;
    public float chargeHitRadius = 2.5f;
    public float chargeIndicatorBaseLength = 7f;
    [SerializeField] private GameObject chargeIndicatorPrefab;
    [Tooltip("돌진 준비 중 플레이어를 따라보는 회전 속도")]
    public float chargeTurnSpeed = 360f;
    [Tooltip("돌진 장판 앞뒤 위치 보정")]
    public float chargeIndicatorForwardOffset = 0f;
    [Tooltip("Root Motion 돌진에 추가로 밀어주는 속도")]
    public float chargeExtraMoveSpeed = 0f;

    [Header("2페이즈 메테오 스킬")]
    public float meteorDuration = 5f;
    public float meteorCooldown = 18f;
    public float meteorDamageInterval = 0.4f;
    public float meteorRadius = 18f;
    public float meteorHitRadius = 2.5f;
    public int meteorDamage = 35;
    [SerializeField] private GameObject meteorEffectPrefab;
    public float meteorEffectScale = 1f;
    public float meteorEffectSpeed = 0.6f;
    public float meteorEffectLifeTime = 6f;
    public int meteorSpawnMaxCount = 8;
    [Header("메테오 경고")]
    [SerializeField] private GameObject meteorWarningPrefab;
    public float meteorWarningTime = 1f;
    [Header("메테오 바닥 체크")]
    [SerializeField] private LayerMask meteorGroundLayer;
    [Header("점프 착지 충격파")]
    public int jumpImpactWaveCount = 3;
    public float jumpImpactWaveInterval = 0.4f;
    public float jumpImpactWaveRadius1 = 4f;
    public float jumpImpactWaveRadius2 = 7f;
    public float jumpImpactWaveRadius3 = 10f;
    public float jumpImpactWaveEffectScale1 = 1f;
    public float jumpImpactWaveEffectScale2 = 1.5f;
    public float jumpImpactWaveEffectScale3 = 2f;
    [Header("메테오 콤보 착지 이펙트")]
    public float meteorComboImpactEffectSpeed = 3f;
    [Header("메테오 전용 현자타임")]
    public float meteorRecoveryTime = 6f;

    [Header("2페이즈 오른찍-왼날개-왼찍 콤보")]
    public float phase2MixedComboDuration = 6f;
    public float phase2MixedComboSecondDelay = 1.6f;
    public float phase2MixedComboThirdDelay = 3.2f;
    public float phase2MixedComboCooldown = 14f;

    [Header("2페이즈 양쪽 내려찍기 + 양쪽 윙슬램 콤보")]
    public float phase2CrushSlamComboDuration = 11f;
    public float phase2CrushSlamComboSecondDelay = 2.4f;
    public float phase2CrushSlamComboThirdDelay = 5.2f;
    public float phase2CrushSlamComboFourthDelay = 8.0f;
    public float phase2CrushSlamComboCooldown = 16f;

    [Header("5갈래 회오리 공격")]
    [SerializeField] private GameObject tornadoFiveWayVfxPrefab;
    [SerializeField] private GameObject tornadoHitboxPrefab;
    [SerializeField] private Transform tornadoSpawnPoint;
    public float fiveTornadoDuration = 5f;
    public float fiveTornadoCooldown = 20f;
    public float tornadoProjectileSpeed = 12f;
    public float tornadoProjectileLifeTime = 4f;
    public int tornadoProjectileDamage = 30;
    public float tornadoSpreadAngle = 60f;




    [Header("fbx모음")]
    [Header("날개 내려찍기 이펙트")]
    [SerializeField] private GameObject leftWingSlamEffectPrefab;
    [SerializeField] private GameObject rightWingSlamEffectPrefab;
    [Header("날개 슬램 이펙트 보스 기준 위치")]
    public Vector3 leftWingSlamLocalOffset = new Vector3(-4f, 0f, 2f);
    public Vector3 rightWingSlamLocalOffset = new Vector3(4f, 0f, 2f);
    public float wingSlamEffectScale = 2f;
    public float wingSlamEffectSpeed = 0.5f;
    public float wingSlamEffectLifeTime = 3f;
    [Header("꼬리 공격 이펙트")]
    [SerializeField] private GameObject leftTailAttackEffectPrefab;
    [SerializeField] private GameObject rightTailAttackEffectPrefab;
    [SerializeField] private Transform leftTailAttackEffectPoint;
    [SerializeField] private Transform rightTailAttackEffectPoint;
    public float tailAttackEffectScale = 2f;
    public float tailAttackEffectSpeed = 0.5f;
    public float tailAttackEffectLifeTime = 3f;
    [Header("브레스 차지 폭발 이펙트")]
    [SerializeField] private GameObject breathChargeExplosionEffectPrefab;
    public float breathChargeExplosionEffectScale = 5f;
    public float breathChargeExplosionEffectSpeed = 0.6f;
    public float breathChargeExplosionEffectLifeTime = 4f;
    [Header("점프 공격 이펙트")]
    [SerializeField] private GameObject jumpStartDustEffectPrefab;
    [SerializeField] private GameObject jumpLandImpactEffectPrefab;
    public float jumpStartDustEffectScale = 2f;
    public float jumpStartDustEffectSpeed = 0.6f;
    public float jumpStartDustEffectLifeTime = 3f;
    public float jumpLandImpactEffectScale = 4f;
    public float jumpLandImpactEffectSpeed = 0.6f;
    public float jumpLandImpactEffectLifeTime = 4f;
    [Header("포효 이펙트")]
    [SerializeField] private GameObject roarEffectPrefab;
    public float roarEffectScale = 4f;
    public float roarEffectSpeed = 0.6f;
    public float roarEffectLifeTime = 4f;
    [Header("이펙트 바닥 체크")]
    [SerializeField] private LayerMask groundLayer;
    [Header("메테오 착탄 폭발 이펙트")]
    [SerializeField] private GameObject meteorImpactEffectPrefab;
    public float meteorImpactEffectScale = 1f;
    public float meteorImpactEffectSpeed = 0.6f;
    public float meteorImpactEffectLifeTime = 3f;
    [Header("먼지 이펙트")]
    [SerializeField] private GameObject dustEffectPrefab;
    [SerializeField] private float dustEffectScale = 1f;
    [SerializeField] private float dustEffectLifeTime = 3f;
    [SerializeField] private float dustEffectYOffset = 0.1f;




    //변수들
    public bool HasRoared => hasRoared;
    private float biteCooldownTimer = 0f;
    private Vector3 lockedAttackPosition;
    private float wingSlamCooldownTimer = 0f;
    private float globalAttackRecoveryTimer = 0f;
    private float tailAttackCooldownTimer = 0f;
    private float fireballCooldownTimer = 0f;
    private Vector3 lockedFireballTargetPosition;
    private bool roarRequested = false;
    private float wingComboCooldownTimer = 0f;
    private float jumpAttackCooldownTimer = 0f;
    private float wingCrushBreathCooldownTimer = 0f;
    private float wingSlamBiteComboCooldownTimer = 0f;
    private float wingSlamBiteSideBreathComboCooldownTimer = 0f;
    private GameObject currentBreath;
    private BreathFollowMouth currentBreathFollow;
    private float sideJumpCooldownTimer = 0f;
    private bool isHeadWeakPointBroken = false;
    private float breathChargeCooldownTimer = 0f;
    private GameObject currentBreathCharge;
    private float breathChargeRestartTimer = 0f;
    private bool isBreakingWeakPoint = false;
    private bool isHeadBroken = false;
    private bool isLeftWingBroken = false;
    private bool isRightWingBroken = false;
    private bool cancelBreathCharge = false;
    private bool isPhase2 = false;
    private bool phase2Requested = false;
    private bool firstEncounterRoared = false;
    private bool phase2Roared = false;
    private bool breathCharge1Used = false;
    private bool breathCharge2Used = false;
    private bool breathCharge3Used = false;
    private bool breathChargeEventRequested = false;
    private int currentBreathChargeEventIndex = 0;
    private bool isBTActionPlaying = false;
    private float chargeCooldownTimer = 0f;
    private float meteorCooldownTimer = 0f;
    private float phase2MixedComboCooldownTimer = 0f;
    private float phase2CrushSlamComboCooldownTimer = 0f;
    private bool phase2FirstMeteorRequested = false;
    private float leftWingSlamCooldownTimer = 0f;
    private float rightWingSlamCooldownTimer = 0f;
    private float fiveTornadoCooldownTimer = 0f;
    private bool meteor2SoundPlayed = false;
    private Coroutine chargeSoundRoutine;


    //기본적으로 모든 스킬에 다 쓸거 (마지막 플레이어 위치 저장)
    public void LockAttackPosition()
    {
        Transform target = GetLockedTarget();

        if (target == null)
            return;

        lockedAttackPosition = target.position;
    }

    //공통 현자타임 시간(개별로 나눔)
    public bool CanUseAnyAttack()
    {
        return globalAttackRecoveryTimer <= 0f
            && !isBreakingWeakPoint
            && !isBTActionPlaying;
    }
    //이하 동일
    public void StartGlobalAttackRecovery()
    {
        globalAttackRecoveryTimer = combatIdleTime;
    }

    //마지막 공격 위치
    public Vector3 GetLockedAttackPosition()
    {
        return lockedAttackPosition;
    }
    protected override void Start()
    {
        base.Start();

        agent.updateRotation = false;
        agent.speed = PatrolSpeed;
    }

    protected override void EnemyAIUpdate()
    {
        if (_isDead) return;

        if (globalAttackRecoveryTimer > 0f)
            globalAttackRecoveryTimer -= Time.deltaTime;

        if (faceCooldownTimer > 0f)
            faceCooldownTimer -= Time.deltaTime;

        if (biteCooldownTimer > 0f)
            biteCooldownTimer -= Time.deltaTime;

        if (wingSlamCooldownTimer > 0f)
            wingSlamCooldownTimer -= Time.deltaTime;

        if (tailAttackCooldownTimer > 0f)
            tailAttackCooldownTimer -= Time.deltaTime;

        if (fireballCooldownTimer > 0f)
            fireballCooldownTimer -= Time.deltaTime;

        if (wingComboCooldownTimer > 0f)
            wingComboCooldownTimer -= Time.deltaTime;

        if (jumpAttackCooldownTimer > 0f)
            jumpAttackCooldownTimer -= Time.deltaTime;

        if (wingCrushBreathCooldownTimer > 0f)
            wingCrushBreathCooldownTimer -= Time.deltaTime;

        if (wingSlamBiteComboCooldownTimer > 0f)
            wingSlamBiteComboCooldownTimer -= Time.deltaTime;

        if (wingSlamBiteSideBreathComboCooldownTimer > 0f)
            wingSlamBiteSideBreathComboCooldownTimer -= Time.deltaTime;

        if (sideJumpCooldownTimer > 0f)
            sideJumpCooldownTimer -= Time.deltaTime;

        if (breathChargeCooldownTimer > 0f)
            breathChargeCooldownTimer -= Time.deltaTime;

        if (chargeCooldownTimer > 0f)
            chargeCooldownTimer -= Time.deltaTime;

        if (meteorCooldownTimer > 0f)
            meteorCooldownTimer -= Time.deltaTime;

        if (phase2MixedComboCooldownTimer > 0f)
            phase2MixedComboCooldownTimer -= Time.deltaTime;

        if (phase2CrushSlamComboCooldownTimer > 0f)
            phase2CrushSlamComboCooldownTimer -= Time.deltaTime;

        if (leftWingSlamCooldownTimer > 0f)
            leftWingSlamCooldownTimer -= Time.deltaTime;

        if (rightWingSlamCooldownTimer > 0f)
            rightWingSlamCooldownTimer -= Time.deltaTime;

        if(fiveTornadoCooldownTimer > 0f)
            fiveTornadoCooldownTimer -= Time.deltaTime;

    }

    public void SetMoveType(int type)
    {
        if (currentMoveType == type) return;

        currentMoveType = type;
        animator.SetInteger("MoveType", type);
    }

    public void StopMove()
    {
        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    public void Idle()
    {
        StopDragonWingFlapSound();
        StopDragonBreathSound();

        StopMove();
        SetMoveType(0);
    }
    //순찰 랜덤 포인트 뽑기
    public bool GetRandomPatrolPoint(out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * PatrolRadius;
            randomPos.y = transform.position.y;

            if (Vector3.Distance(transform.position, randomPos) < 3f)
                continue;

            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = transform.position;
        return false;
    }

    public bool HasPlayerInRange()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) return false;

        float dist = Vector3.Distance(transform.position, player.position);

        if (lockedTarget != null)
        {
            if (dist <= targetLoseRange)
                return true;

            lockedTarget = null;
            return false;
        }

        if (dist <= playerDetectRange)
        {
            lockedTarget = player;
            return true;
        }

        return false;
    }

    public bool HasLockedTarget()
    {
        return lockedTarget != null;
    }

    public Transform GetLockedTarget()
    {
        return lockedTarget;
    }

    public bool CanFacePlayer()
    {
        return faceCooldownTimer <= 0f;
    }

    public void StartFaceCooldown()
    {
        faceCooldownTimer = faceCooldown;
    }

    public void SetRoared()
    {
        hasRoared = true;
    }


    //깨물기 패턴 
    public int GetBiteMoveType()
    {
        Transform target = GetLockedTarget();

        if (target == null)
        {
            Debug.Log("GetBiteMoveType 실패: target null");
            return -1;
        }

        float dist = Vector3.Distance(transform.position, target.position);
        Debug.Log($"Bite 거리: {dist}");

        if (dist > biteRange)
        {
            Debug.Log($"Bite 실패: 거리 밖 dist={dist}, range={biteRange}");
            return -1;
        }

        int rand = Random.Range(0, 3);

        if (rand == 0)
            return 5; // 정면 물기

        if (rand == 1)
            return 6; // 왼쪽 물기

        return 7; // 오른쪽 물기
    }
    //꺠물고 난 후 현타시간
    public bool CanBite()
    {
        return biteCooldownTimer <= 0f;
    }

    public void StartBiteCooldown()
    {
        biteCooldownTimer = biteCooldown;
    }



    //꺠물기 범위에 있나
    public bool IsLockedTargetInBiteRange()
    {
        Transform target = GetLockedTarget();
        if (target == null) return false;

        float dist = Vector3.Distance(transform.position, target.position);
        return dist <= biteRange;
    }

    public void PlayBite(int moveType)
    {
        animator.ResetTrigger("BiteFront");
        animator.ResetTrigger("BiteLeft");
        animator.ResetTrigger("BiteRight");

        if (moveType == 5)
            animator.SetTrigger("BiteFront");
        else if (moveType == 6)
            animator.SetTrigger("BiteLeft");
        else if (moveType == 7)
            animator.SetTrigger("BiteRight");
    }

    //날개를 스윙할 수 있나?
    public bool CanWingSlam()
    {
        int moveType = GetWingSlamMoveType();

        if (moveType == 8)
            return leftWingSlamCooldownTimer <= 0f;

        if (moveType == 9)
            return rightWingSlamCooldownTimer <= 0f;

        return false;
    }
    public int GetWingSlamMoveType()
    {
        Transform target = GetLockedTarget();

        if (target == null)
            return -1;

        float dist = Vector3.Distance(transform.position, target.position);

        if (dist > wingSlamRange)
            return -1;

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f)
            return -1;

        float angle = Vector3.SignedAngle(
            transform.forward,
            dir.normalized,
            Vector3.up
        );

        if (angle > 0f)
            return 9; // 오른쪽 날개 내려찍기

        return 8; // 왼쪽 날개 내려찍기
    }

    public void StartWingSlamCooldown(int moveType)
    {
        if (moveType == 8)
            leftWingSlamCooldownTimer = wingSlamCooldown;
        else if (moveType == 9)
            rightWingSlamCooldownTimer = wingSlamCooldown;
    }

    public void PlayWingSlam(int moveType)
    {
        animator.ResetTrigger("LeftWingSlam");
        animator.ResetTrigger("RightWingSlam");

        if (moveType == 8)
            animator.SetTrigger("LeftWingSlam");
        else if (moveType == 9)
            animator.SetTrigger("RightWingSlam");
    }

    public void DisableAllWingHitboxes()
    {
        leftWingHitbox.DisableHitbox();
        rightWingHitbox.DisableHitbox();
    }

    public bool CanWingCombo()
    {
        return wingComboCooldownTimer <= 0f;
    }

    public void StartWingComboCooldown()
    {
        wingComboCooldownTimer = wingComboCooldown;
    }
    public void PlayLeftWingSlam()
    {
        animator.ResetTrigger("LeftWingSlam");
        animator.SetTrigger("LeftWingSlam");
    }

    public void PlayRightWingSlam()
    {
        animator.ResetTrigger("RightWingSlam");
        animator.SetTrigger("RightWingSlam");
    }



    //랜덤으로 깨물기 공격 
    public int GetRandomBiteMoveType()
    {
        int rand = Random.Range(0, 3);

        if (rand == 0) return 5;
        if (rand == 1) return 6;
        return 7;
    }


    //꼬리 공격가능?
    public bool CanTailAttack()
    {
        return tailAttackCooldownTimer <= 0f;
    }

    public void StartTailAttackCooldown()
    {
        tailAttackCooldownTimer = tailAttackCooldown;
    }

    //꼬리 공격 패턴 
    public int GetTailAttackMoveType()
    {
        Transform target = GetLockedTarget();

        if (target == null)
            return -1;

        float dist = Vector3.Distance(transform.position, target.position);

        if (dist > tailAttackRange)
            return -1;

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f)
            return -1;

        float angle = Vector3.SignedAngle(
            transform.forward,
            dir.normalized,
            Vector3.up
        );

        // 뒤쪽이 아니면 꼬리 공격 안 함
        if (Mathf.Abs(angle) < tailBackAngle)
            return -1;

        if (angle < 0f)
            return 10; // 왼쪽 꼬리 공격

        return 11; // 오른쪽 꼬리 공격
    }

    public void PlayTailAttack(int moveType)
    {
        animator.ResetTrigger("LeftTailAttack");
        animator.ResetTrigger("RightTailAttack");

        if (moveType == 10)
            animator.SetTrigger("LeftTailAttack");
        else if (moveType == 11)
            animator.SetTrigger("RightTailAttack");
    }

    public void DisableAllTailHitboxes()
    {
        leftTailHitbox.DisableHitbox();
        rightTailHitbox.DisableHitbox();
    }

    public bool CanFireball()
    {
        return fireballCooldownTimer <= 0f;
    }

    public void StartFireballCooldown()
    {
        fireballCooldownTimer = fireballCooldown;
    }
    //타겟이 파이어볼 범위에 있나?
    public bool IsTargetInFireballRange()
    {
        Transform target = GetLockedTarget();
        if (target == null) return false;

        float dist = Vector3.Distance(transform.position, target.position);

        return dist >= fireballMinRange && dist <= fireballMaxRange;
    }
    //파이어볼 위치 고정
    public void LockFireballTargetPosition()
    {
        Transform target = GetLockedTarget();

        if (target == null) return;

        lockedFireballTargetPosition = target.position;
    }

    public Vector3 GetLockedFireballTargetPosition()
    {
        return lockedFireballTargetPosition;
    }

    public void PlayFireball()
    {
        animator.ResetTrigger("Fireball");
        animator.SetTrigger("Fireball");
    }
    //파이어볼 쏘기
    public void ShootFireball()
    {
        if (fireballPrefab == null || fireballSpawnPoint == null)
            return;

        Vector3 targetPos = GetLockedFireballTargetPosition();

        Vector3 dir = targetPos - fireballSpawnPoint.position;

        if (dir.sqrMagnitude < 0.01f)
            dir = transform.forward;

        PlayDragonFireballShootSound();

        GameObject fireball = Instantiate(
            fireballPrefab,
            fireballSpawnPoint.position,
            Quaternion.LookRotation(dir.normalized)
        );

        DragonFireballProjectile projectile =
            fireball.GetComponent<DragonFireballProjectile>();

        if (projectile != null)
            projectile.Init(dir, this);
    }

    public bool CanJumpAttack()
    {
        return jumpAttackCooldownTimer <= 0f;
    }

    public void StartJumpAttackCooldown()
    {
        jumpAttackCooldownTimer = jumpAttackCooldown;
    }

    public void PlayJumpStart()
    {
        animator.SetTrigger("JumpStart");
    }

    public void PlaySkyLoop()
    {
        animator.ResetTrigger("FlyUp");
        animator.ResetTrigger("SkyLoop");
        animator.SetTrigger("SkyLoop");

        StartDragonWingFlapSound();
    }

    public void PlayJumpFall()
    {
        StopDragonWingFlapSound();
        animator.SetTrigger("JumpFall");
    }

    //점프 위험장판 표시
    public GameObject CreateJumpWarning(Vector3 position)
    {
        if (jumpWarningPrefab == null)
            return null;

        GameObject warning = Instantiate(
            jumpWarningPrefab,
            position + Vector3.up * 0.05f,
            Quaternion.Euler(90f, 0f, 0f)
        );

        float size = jumpDamageRadius * 2f;

        JumpWarningFill fill = warning.GetComponent<JumpWarningFill>();

        if (fill != null)
            fill.Init(size, warningShowTime);
        else
            warning.transform.localScale = new Vector3(size, size, 1f);

        return warning;
    }

    //점프 데미지 주기
    public void DoJumpDamage(Vector3 center)
    {
        Collider[] hits = Physics.OverlapSphere(
            center,
            jumpDamageRadius,
            targetLayer
        );

        foreach (Collider hit in hits)
        {
            Actor target = hit.GetComponentInParent<Actor>();

            if (target == null)
                continue;

            if (target == this)
                continue;

            target.TakeDamage(jumpDamage);
        }
    }

    public bool CanWingCrushBreathCombo()
    {
        return !isPhase2 && wingCrushBreathCooldownTimer <= 0f;
    }

    public void StartWingCrushBreathComboCooldown()
    {
        wingCrushBreathCooldownTimer = wingCrushBreathCooldown;
    }

    public void PlayLeftWingCrush()
    {
        animator.ResetTrigger("LeftWingCrush");
        animator.SetTrigger("LeftWingCrush");
    }

    public void PlayRightWingCrush()
    {
        animator.ResetTrigger("RightWingCrush");
        animator.SetTrigger("RightWingCrush");
    }

    public void PlayFireBreath()
    {
        animator.ResetTrigger("FireBreath");
        animator.SetTrigger("FireBreath");
    }
    private void DoWingCrushTriangleDamage(Transform point)
    {
        if (point == null)
            return;

        Vector3 origin = point.position;

        if (Physics.Raycast(
            point.position,
            Vector3.down,
            out RaycastHit hit,
            50f,
            groundLayer))
        {
            origin = hit.point;
        }

        origin.y += 0.05f;

        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = transform.right;
        right.y = 0f;
        right.Normalize();


        // 디버그 삼각형
        Debug.DrawRay(origin, forward * wingCrushTriangleLength, Color.red, 2f);

        Debug.DrawRay(
            origin,
            (forward * wingCrushTriangleLength) + (right * wingCrushTriangleWidth * 0.5f),
            Color.green,
            2f
        );

        Debug.DrawRay(
            origin,
            (forward * wingCrushTriangleLength) - (right * wingCrushTriangleWidth * 0.5f),
            Color.green,
            2f
        );

        Actor[] actors = FindObjectsByType<Actor>(FindObjectsSortMode.None);

        foreach (Actor actor in actors)
        {
            if (actor == null || actor.IsDead)
                continue;

            if (actor == this)
                continue;

            if (((1 << actor.gameObject.layer) & targetLayer) == 0)
                continue;

            Collider col = actor.GetComponentInChildren<Collider>();
            if (col == null)
                continue;

            Vector3 targetPos = col.bounds.center;
            targetPos.y = origin.y;

            Vector3 toTarget = targetPos - origin;

            float z = Vector3.Dot(toTarget, forward);
            float x = Vector3.Dot(toTarget, right);

            if (z < 0f || z > wingCrushTriangleLength)
                continue;

            float widthAtZ = Mathf.Lerp(
                0f,
                wingCrushTriangleWidth * 0.5f,
                z / wingCrushTriangleLength
            );

            bool isInside =
                Mathf.Abs(x) <= widthAtZ;

            if (isInside)
            {
                Debug.Log("날개 크러쉬 히트");
                actor.TakeDamage(wingCrushDamage);
            }
        }
    }

    //브레스 소환
    public void SpawnBreath()
    {
        if (breathPrefab == null || breathSpawnPoint == null)
            return;

        Vector3 dir = transform.forward;
        dir.y = 0f;
        dir.Normalize();

        GameObject breath = Instantiate(
            breathPrefab,
            breathSpawnPoint.position,
            Quaternion.LookRotation(dir)
        );

        // 파티클을 2초 진행된 상태부터 시작
        ParticleSystem[] particles = breath.GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem ps in particles)
        {
            ps.Simulate(2f, true, true);
            ps.Play();
        }

        DragonBreathDamage damage = breath.GetComponent<DragonBreathDamage>();

        if (damage != null)
            damage.Init(this);
    }


    //좌우 날개스윙 후 물기
    public bool CanWingSlamBiteCombo()
    {
        return !isPhase2 && wingSlamBiteComboCooldownTimer <= 0f;
    }

    public void StartWingSlamBiteComboCooldown()
    {
        wingSlamBiteComboCooldownTimer = wingSlamBiteComboCooldown;
    }

    //좌우로 브레스
    public void SpawnAttachedBreath()
    {
        if (breathPrefab == null || breathSpawnPoint == null)
            return;

        if (currentBreath != null)
            Destroy(currentBreath);

        currentBreath = Instantiate(
            breathPrefab,
            breathSpawnPoint.position,
            Quaternion.identity
        );

        currentBreathFollow = currentBreath.GetComponent<BreathFollowMouth>();

        if (currentBreathFollow == null)
            currentBreathFollow = currentBreath.AddComponent<BreathFollowMouth>();

        currentBreathFollow.Init(
            breathSpawnPoint,
            transform,
            new Vector3(0f, 90f, 0f)
        );

        ParticleSystem[] particles = currentBreath.GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem ps in particles)
        {
            ps.Simulate(2f, true, true);
            ps.Play();
        }

        DragonBreathDamage damage = currentBreath.GetComponent<DragonBreathDamage>();

        if (damage != null)
            damage.Init(this);
    }

    public void StopAttachedBreath()
    {
        if (currentBreath != null)
        {
            Destroy(currentBreath);
            currentBreath = null;
            currentBreathFollow = null;
        }
    }

    public void PlaySideBreath()
    {
        animator.ResetTrigger("SideBreath");
        animator.SetTrigger("SideBreath");
    }

    public bool CanWingSlamBiteSideBreathCombo()
    {
        return !isPhase2 && wingSlamBiteSideBreathComboCooldownTimer <= 0f;
    }

    public void StartWingSlamBiteSideBreathComboCooldown()
    {
        wingSlamBiteSideBreathComboCooldownTimer = wingSlamBiteSideBreathComboCooldown;
    }


    //좌우로 점프 하기
    public bool CanSideJump()
    {
        return !isPhase2 && sideJumpCooldownTimer <= 0f;
    }

    public void StartSideJumpCooldown()
    {
        sideJumpCooldownTimer = sideJumpCooldown;
    }

    public void PlayLeftSideJump()
    {
        animator.ResetTrigger("LeftSideJump");
        animator.SetTrigger("LeftSideJump");
    }

    public void PlayRightSideJump()
    {
        animator.ResetTrigger("RightSideJump");
        animator.SetTrigger("RightSideJump");
    }

    public bool CanBreathCharge()
    {
        return !isPhase2 && breathChargeCooldownTimer <= 0f;
    }

    public void StartBreathChargeCooldown()
    {
        breathChargeCooldownTimer = breathChargeCooldown;
    }

    public void PlayBreathCast()
    {
        animator.ResetTrigger("BreathCast");
        animator.SetTrigger("BreathCast");

        if (chargeSoundRoutine != null)
            StopCoroutine(chargeSoundRoutine);

        chargeSoundRoutine = StartCoroutine(DelayedChargeSound());
    }

    public void PlayBreathChargeLoop()
    {
        animator.ResetTrigger("BreathChargeLoop");
        animator.SetTrigger("BreathChargeLoop");
    }

    //브레스 차지 
    public void SpawnBreathCharge()
    {
        if (breathChargePrefab == null || breathSpawnPoint == null)
            return;

        if (currentBreathCharge != null)
            Destroy(currentBreathCharge);

        currentBreathCharge = Instantiate(
            breathChargePrefab,
            breathSpawnPoint.position,
            Quaternion.identity,
            breathSpawnPoint
        );

        currentBreathCharge.transform.localPosition = Vector3.zero;

        Vector3 dir = transform.forward;
        dir.y = 0f;
        dir.Normalize();

        currentBreathCharge.transform.rotation = Quaternion.LookRotation(dir);
        currentBreathCharge.transform.rotation *= Quaternion.Euler(-90f, 0f, 0f);

        ParticleSystem[] particles =
            currentBreathCharge.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in particles)
        {
            ps.gameObject.SetActive(true);
            ps.Clear(true);
            ps.Play(true);
        }

        breathChargeRestartTimer = 0f;
    }

    public void KeepBreathChargeAlive()
    {
        if (breathChargePrefab == null || breathSpawnPoint == null)
            return;

        breathChargeRestartTimer += Time.deltaTime;

        if (currentBreathCharge != null)
        {
            currentBreathCharge.transform.position = breathSpawnPoint.position;

            Vector3 dir = transform.forward;
            dir.y = 0f;
            dir.Normalize();

            currentBreathCharge.transform.rotation = Quaternion.LookRotation(dir);
            currentBreathCharge.transform.rotation *= Quaternion.Euler(-90f, 0f, 0f);
        }

        if (currentBreathCharge == null || breathChargeRestartTimer >= breathChargeFxLifeTime)
        {
            SpawnBreathCharge();
        }
    }

    public void StopBreathCharge()
    {
        if (currentBreathCharge != null)
        {
            Destroy(currentBreathCharge);
            currentBreathCharge = null;
        }
    }
    public void EndBreathChargeLoop()
    {
        SoundManager.Instance.StopSound("DragonCharge1");

        if (chargeSoundRoutine != null)
        {
            StopCoroutine(chargeSoundRoutine);
            chargeSoundRoutine = null;
        }

        StopBreathCharge();

        animator.ResetTrigger("EndBreathCharge");
        animator.SetTrigger("EndBreathCharge");
    }

    private void CheckBreathChargeEventRequest()
    {
        if (isPhase2 || phase2Requested || phase2Roared)
            return;

        if (breathChargeEventRequested)
            return;

        if (AreBothWingBroken())
            return;

        // 양쪽 날개 다 부쉈으면 더 이상 브레스 차지 안 함
        if (AreBothWingBroken())
            return;

        float hpRate = (float)_currentHP / _maxHP;

        if (!breathCharge1Used && hpRate <= breathChargeHp1)
        {
            breathCharge1Used = true;
            RequestBreathChargeEvent(1);
            return;
        }

        if (!breathCharge2Used && hpRate <= breathChargeHp2)
        {
            breathCharge2Used = true;
            RequestBreathChargeEvent(2);
            return;
        }

        if (!breathCharge3Used && hpRate <= breathChargeHp3)
        {
            breathCharge3Used = true;
            RequestBreathChargeEvent(3);
            return;
        }
    }

    private void RequestBreathChargeEvent(int index)
    {
        breathChargeEventRequested = true;
        currentBreathChargeEventIndex = index;

        Debug.Log($"브레스 차지 {index}번째 예약");
    }

    public bool ShouldUseBreathChargeEvent()
    {
        if (isPhase2)
            return false;

        if (!breathChargeEventRequested)
            return false;

        if (AreBothWingBroken())
        {
            ClearBreathChargeEvent();
            return false;
        }

        return true;
    }

    public void ClearBreathChargeEvent()
    {
        breathChargeEventRequested = false;
        currentBreathChargeEventIndex = 0;
    }

    public bool AreBothWingBroken()
    {
        return isLeftWingBroken && isRightWingBroken;
    }

    public int GetBreathChargeEventDamage()
    {
        if (currentBreathChargeEventIndex == 3 && !AreBothWingBroken())
            return finalBreathChargeDamage;

        return normalBreathChargeDamage;
    }

    public bool IsBreakingWeakPoint()
    {
        return isBreakingWeakPoint;
    }

    //Type별 부위파괴
    public void OnWeakPointBreak(WeakPointType type)
    {
        if (_isDead || isBreakingWeakPoint)
            return;

        switch (type)
        {
            case WeakPointType.Head:
                if (isHeadBroken) return;

                isHeadBroken = true;

                fireballCooldownTimer += 150f;

                StartCoroutine(
                    WeakPointBreakRoutine(
                        WeakPointType.Head,
                        "HeadBreak"
                    )
                );
                break;

            case WeakPointType.LeftWing:
                if (isLeftWingBroken) return;

                isLeftWingBroken = true;

                isBreakingWeakPoint = true;
                isBTActionPlaying = true;

                leftWingSlamCooldownTimer += 130f;

                CancelBreathCharge();

                StartCoroutine(
                    WeakPointBreakRoutine(
                        WeakPointType.LeftWing,
                        "LeftWingBreak"
                    )
                );
                break;

            case WeakPointType.RightWing:
                if (isRightWingBroken) return;

                isRightWingBroken = true;

                isBreakingWeakPoint = true;
                isBTActionPlaying = true;

                rightWingSlamCooldownTimer += 130f;

                CancelBreathCharge();

                StartCoroutine(
                    WeakPointBreakRoutine(
                        WeakPointType.RightWing,
                        "RightWingBreak"
                    )
                );
                break;
        }
    }

    //부위파괴 루틴 (머리 양쪽날개 포함)
    private IEnumerator WeakPointBreakRoutine(WeakPointType type, string triggerName)
    {
        isBreakingWeakPoint = true;
        isBTActionPlaying = true; // 추가

        StopMove();

        StopDragonWingFlapSound();
        StopDragonBreathSound();
        SoundManager.Instance.StopSound("DragonCharge1");

        switch (type)
        {
            case WeakPointType.Head:
                DisableHeadWeakPoint();
                break;

            case WeakPointType.LeftWing:
                DisableLeftWingWeakPoint();
                break;

            case WeakPointType.RightWing:
                DisableRightWingWeakPoint();
                break;
        }

        animator.ResetTrigger("HeadBreak");
        animator.ResetTrigger("LeftWingBreak");
        animator.ResetTrigger("RightWingBreak");

        // 공격 트리거들도 끊어줘야 함
        animator.ResetTrigger("BiteFront");
        animator.ResetTrigger("BiteLeft");
        animator.ResetTrigger("BiteRight");
        animator.ResetTrigger("LeftWingSlam");
        animator.ResetTrigger("RightWingSlam");
        animator.ResetTrigger("LeftTailAttack");
        animator.ResetTrigger("RightTailAttack");
        animator.ResetTrigger("Fireball");
        animator.ResetTrigger("Charge");
        animator.ResetTrigger("ChargeReady");
        animator.ResetTrigger("FireBreath");
        animator.ResetTrigger("BreathCast");
        animator.ResetTrigger("BreathChargeLoop");
        animator.ResetTrigger("FiveTornado");

        animator.SetTrigger(triggerName);

        yield return new WaitForSeconds(weakPointBreakAnimTime);

        isBreakingWeakPoint = false;
        isBTActionPlaying = false; 

        Idle();
        StartGlobalAttackRecovery();
    }

    public void DisableLeftWingWeakPoint()
    {
        if (leftWingWeakPoint != null)
            leftWingWeakPoint.gameObject.SetActive(false);
    }

    public void DisableRightWingWeakPoint()
    {
        if (rightWingWeakPoint != null)
            rightWingWeakPoint.gameObject.SetActive(false);
    }

    public bool ShouldCancelBreathCharge()
    {
        return cancelBreathCharge;
    }

    public void CancelBreathCharge()
    {
        cancelBreathCharge = true;

        SoundManager.Instance.StopSound("DragonCharge1");

        if (chargeSoundRoutine != null)
        {
            StopCoroutine(chargeSoundRoutine);
            chargeSoundRoutine = null;
        }

        StopBreathCharge();

        if (!isBreakingWeakPoint)
            EndBreathChargeLoop();
    }

    //리셋하기
    public void ResetBreathChargeCancel()
    {
        cancelBreathCharge = false;
    }

    //브레스 차지 데미지
    public void DoBreathChargeExplosionDamage()
    {
        PlayDragonChargeBombSound();

        int damage = GetBreathChargeEventDamage();

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            breathChargeExplosionRadius,
            targetLayer
        );

        foreach (Collider hit in hits)
        {
            Actor target = hit.GetComponentInParent<Actor>();

            if (target == null)
                continue;

            if (target == this)
                continue;

            target.TakeDamage(damage);
        }

        Debug.Log($"[BreathCharge] 광역 데미지 발생: {damage}");
    }

    public bool CanCharge()
    {
        return !isPhase2 && chargeCooldownTimer <= 0f;
    }

    public void StartChargeCooldown()
    {
        chargeCooldownTimer = chargeCooldown;
    }

    public bool IsTargetInChargeRange()
    {
        Transform target = GetLockedTarget();
        if (target == null) return false;

        float dist = Vector3.Distance(transform.position, target.position);
        return dist <= chargeDistance;
    }

    public void PlayChargeReady()
    {
        animator.ResetTrigger("Charge");
        animator.ResetTrigger("ChargeReady");

        animator.SetTrigger("ChargeReady");
    }

    public void PlayCharge()
    {
        animator.ResetTrigger("ChargeReady");
        animator.ResetTrigger("Charge");

        animator.SetTrigger("Charge");
    }

    public void ResetChargeTriggers()
    {
        animator.ResetTrigger("ChargeReady");
        animator.ResetTrigger("Charge");
    }

    public GameObject CreateChargeIndicator()
    {
        if (chargeIndicatorPrefab == null)
            return null;

        GameObject indicator = Instantiate(chargeIndicatorPrefab);
        indicator.SetActive(true);

        return indicator;
    }

    //메테오를 할수있나?
    public bool CanMeteor()
    {
        if (testMeteorOnly)
            return meteorCooldownTimer <= 0f;

        if (phase2FirstMeteorRequested)
            return isPhase2;

        return isPhase2 && meteorCooldownTimer <= 0f;
    }

    public void ClearPhase2FirstMeteorRequest()
    {
        phase2FirstMeteorRequested = false;
    }

    //메테오 쿨타임
    public void StartMeteorCooldown()
    {
        meteorCooldownTimer = meteorCooldown;
    }

    //날기
    public void PlayFlyUp()
    {
        animator.ResetTrigger("FlyUp");
        animator.SetTrigger("FlyUp");

        StartDragonWingFlapSound();

        meteor2SoundPlayed = false;
    }

    public void PlayDragonMeteor2SoundOnce()
    {
        if (meteor2SoundPlayed)
            return;

        meteor2SoundPlayed = true;
        PlayDragonMeteor2Sound();
    }

    //메테오 소환
    private IEnumerator MeteorWarningSpawnRoutine()
    {
        for (int i = 0; i < meteorSpawnMaxCount; i++)
        {
            Vector3 pos = GetRandomMeteorPosition();
            StartCoroutine(MeteorStrikeRoutine(pos));

            yield return new WaitForSeconds(meteorDamageInterval);
        }
    }

    //메테오 소환 이펙트
 

    public IEnumerator MeteorStrikeRoutine(Vector3 position)
    {
        GameObject warning = null;

        if (meteorWarningPrefab != null)
        {
            Vector3 warningPos = position;
            warningPos.y += 0.1f;

            warning = Instantiate(
                meteorWarningPrefab,
                warningPos,
                meteorWarningPrefab.transform.rotation
            );

            float size = meteorHitRadius * 2f;

            warning.transform.localScale =
                new Vector3(size, size, 1f);
        }

        yield return new WaitForSeconds(meteorWarningTime);

        if (warning != null)
            Destroy(warning);

        PlayDragonMeteor3Sound();
        PlayMeteorImpactEffect(position);

        Collider[] hits = Physics.OverlapSphere(
            position,
            meteorHitRadius,
            targetLayer
        );

        foreach (Collider hit in hits)
        {
            Actor target = hit.GetComponentInParent<Actor>();

            if (target == null || target == this)
                continue;

            target.TakeDamage(meteorDamage);
        }
    }

    //메테오 위치 랜덤 
    public Vector3 GetRandomMeteorPosition()
    {
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * meteorRadius;

        Vector3 pos = transform.position + new Vector3(
            randomCircle.x,
            0f,
            randomCircle.y
        );

        Vector3 rayStart = pos + Vector3.up * 50f;

        if (Physics.Raycast(
            rayStart,
            Vector3.down,
            out RaycastHit hit,
            100f,
            meteorGroundLayer))
        {
            pos = hit.point;
        }

        return pos;
    }

    //메테오 이펙트 
    private void PlayMeteorImpactEffect(Vector3 position)
    {
        if (meteorImpactEffectPrefab == null)
            return;

        Vector3 spawnPos = position;
        spawnPos.y += 0.05f;

        GameObject effect = Instantiate(
            meteorImpactEffectPrefab,
            spawnPos,
            meteorImpactEffectPrefab.transform.rotation
        );

        effect.transform.SetParent(null);

        effect.transform.localScale *= meteorImpactEffectScale;

        ParticleSystem[] particles =
            effect.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in particles)
        {
            ParticleSystem.MainModule main = ps.main;
            main.simulationSpeed = meteorImpactEffectSpeed;
            ps.Play(true);
        }

        Destroy(effect, meteorImpactEffectLifeTime);
    }

    public void StartJumpImpactWave(Vector3 center)
    {
        StartCoroutine(JumpImpactWaveRoutine(center));
    }

    //3단 장판 터짐 코드
    private IEnumerator JumpImpactWaveRoutine(Vector3 center)
    {
        float[] radiuses =
        {
        jumpImpactWaveRadius1,
        jumpImpactWaveRadius2,
        jumpImpactWaveRadius3
    };

        float[] effectScales =
        {
        jumpImpactWaveEffectScale1,
        jumpImpactWaveEffectScale2,
        jumpImpactWaveEffectScale3
    };

        for (int i = 0; i < jumpImpactWaveCount; i++)
        {
            float radius = radiuses[i];
            float effectScale = effectScales[i];

            SoundManager.Instance.PlaySound("WolfFireballExplosion");

            DoJumpWaveDamage(center, radius);
            PlayMeteorComboImpactEffect(center, effectScale);

            if (i < jumpImpactWaveCount - 1)
                yield return new WaitForSeconds(jumpImpactWaveInterval);
        }
    }

    private void DoJumpWaveDamage(Vector3 center, float radius)
    {
        Collider[] hits = Physics.OverlapSphere(
            center,
            radius,
            targetLayer
        );

        foreach (Collider hit in hits)
        {
            Actor target = hit.GetComponentInParent<Actor>();

            if (target == null)
                continue;

            if (target == this)
                continue;

            target.TakeDamage(jumpDamage);
        }
    }

    // 이펙트 콤보
    public void PlayMeteorComboImpactEffect(
    Vector3 position,
    float customScale)
    {
        SpawnEffectOnGround(
            jumpLandImpactEffectPrefab,
            position,
            jumpLandImpactEffectScale * customScale,
            meteorComboImpactEffectSpeed,
            jumpLandImpactEffectLifeTime
        );
    }

    public void StartMeteorRecovery()
    {
        globalAttackRecoveryTimer = meteorRecoveryTime;
    }

    public void SyncAgentToTransform()
    {
        if (agent == null || !agent.enabled)
            return;

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
    }

    //페이즈2스킬 콤보 믹스 1번 콤보
    public bool CanPhase2MixedCombo()
    {
        return isPhase2 && phase2MixedComboCooldownTimer <= 0f;
    }

    public void StartPhase2MixedComboCooldown()
    {
        phase2MixedComboCooldownTimer = phase2MixedComboCooldown;
    }

    //페이즈2스킬 콤보 믹스 2번 콤보
    public bool CanPhase2CrushSlamCombo()
    {
        return isPhase2 && phase2CrushSlamComboCooldownTimer <= 0f;
    }

    public void StartPhase2CrushSlamComboCooldown()
    {
        phase2CrushSlamComboCooldownTimer = phase2CrushSlamComboCooldown;
    }

    //5가지 토네이도
    public bool CanFiveTornado()
    {
        return  fiveTornadoCooldownTimer <= 0f;
    }

    public void StartFiveTornadoCooldown()
    {
        fiveTornadoCooldownTimer = fiveTornadoCooldown;
    }

    public void PlayFiveTornado()
    {
        animator.ResetTrigger("FiveTornado");
        animator.SetTrigger("FiveTornado");
    }

    //죽음
    public override void TakeDamage(
     int damage,
     float severityOverride = -1f,
     bool isHeavyAttack = false,
     bool showDamageText = true)
    {
        base.TakeDamage(damage, severityOverride, isHeavyAttack, showDamageText);

        if (_isDead) return;

        CheckBreathChargeEventRequest();
        CheckPhase2Request();
    }

    //2페이지 체크
    private void CheckPhase2Request()
    {
        if (isPhase2 || phase2Requested || phase2Roared)
            return;

        float hpRate = (float)_currentHP / _maxHP;

        if (hpRate <= phase2HpRate)
        {
            phase2Requested = true;
            Debug.Log("2페이즈 포효 예약");
        }
    }

    public bool ShouldFirstEncounterRoar()
    {
        return HasPlayerInRange() && !firstEncounterRoared;
    }
    //2페이지 포효
    public bool ShouldPhase2Roar()
    {
        return phase2Requested && !phase2Roared;
    }
    //첫조우 포효
    public void SetFirstEncounterRoared()
    {
        firstEncounterRoared = true;
        hasRoared = true;
    }
    //2페이지 진입
    public void EnterPhase2()
    {
        isPhase2 = true;
        phase2Requested = false;
        phase2Roared = true;

        phase2FirstMeteorRequested = true;
        meteorCooldownTimer = 0f;
    }

    //포효하기
    public bool ShouldRoar()
    {
        return phase2Requested && !isPhase2 && roarRequested;
    }

    public void ClearRoarRequest()
    {
        roarRequested = false;
    }

    // 애니메이션 이벤트
    public void EnableBiteHitbox()
    {
        headHitbox.EnableHitbox();
    }

    public void DisableBiteHitbox()
    {
        headHitbox.DisableHitbox();
    }

    public void EnableLeftWingHitbox()
    {
        leftWingHitbox.EnableHitbox();
    }

    public void DisableLeftWingHitbox()
    {
        leftWingHitbox.DisableHitbox();
    }

    public void EnableLeftTailHitbox()
    {
        leftTailHitbox.EnableHitbox();
    }

    public void DisableLeftTailHitbox()
    {
        leftTailHitbox.DisableHitbox();
    }

    public void EnableRightTailHitbox()
    {
        rightTailHitbox.EnableHitbox();
    }

    public void DisableRightTailHitbox()
    {
        rightTailHitbox.DisableHitbox();
    }

    public void EnableRightWingHitbox()
    {
        rightWingHitbox.EnableHitbox();
    }

    public void DisableRightWingHitbox()
    {
        rightWingHitbox.DisableHitbox();
    }

    public void SetManualMoveMode(bool value)
    {
        if (agent != null)
        {
            agent.updatePosition = !value;
            agent.updateRotation = !value;
        }
    }

    public void EnableWingWeakPoints()
    {
        if (leftWingWeakPoint != null)
        {
            leftWingWeakPoint.gameObject.SetActive(true);
            leftWingWeakPoint.Init(wingWeakPointHP, this);
        }

        if (rightWingWeakPoint != null)
        {
            rightWingWeakPoint.gameObject.SetActive(true);
            rightWingWeakPoint.Init(wingWeakPointHP, this);
        }
    }

    public void DisableWingWeakPoints()
    {
        if (leftWingWeakPoint != null)
            leftWingWeakPoint.gameObject.SetActive(false);

        if (rightWingWeakPoint != null)
            rightWingWeakPoint.gameObject.SetActive(false);
    }

    //약점 키기
    public void EnableHeadWeakPoint()
    {
        if (isHeadWeakPointBroken)
            return;

        if (headWeakPoint == null)
            return;

        headWeakPoint.gameObject.SetActive(true);
        headWeakPoint.Init(headWeakPointHP, this);
    }

    public void DisableHeadWeakPoint()
    {
        if (headWeakPoint == null)
            return;

        headWeakPoint.gameObject.SetActive(false);
    }

    //머리 약점부위파괴
    public void OnHeadWeakPointBreak()
    {
        if (isHeadWeakPointBroken || _isDead)
            return;

        isHeadWeakPointBroken = true;

        StopAttachedBreath();

        DisableHeadWeakPoint();
    }

    public bool IsInGlobalRecovery()
    {
        return globalAttackRecoveryTimer > 0f;
    }

    public bool CanFaceForAttack()
    {
        return globalAttackRecoveryTimer <= 0f
            && faceCooldownTimer <= 0f
            && !IsBreakingWeakPoint();
    }

    public void SetBTActionPlaying(bool value)
    {
        isBTActionPlaying = value;
    }


    public void DoLeftWingCrushImpact()
    {
        DoWingCrushTriangleDamage(leftWingCrushPoint);
        SpawnWingCrushEffect(leftWingCrushEffectPrefab, leftWingCrushPoint);
    }

    public void DoRightWingCrushImpact()
    {
        DoWingCrushTriangleDamage(rightWingCrushPoint);
        SpawnWingCrushEffect(rightWingCrushEffectPrefab, rightWingCrushPoint);
    }

    protected override bool IsRecovering()
    {
        return IsInGlobalRecovery() || isBTActionPlaying || IsBreakingWeakPoint();
    }


    public void ShootFiveWayTornado()
    {
        if (tornadoSpawnPoint == null)
            return;

        Vector3 baseDir = tornadoSpawnPoint.forward;
        baseDir.y = 0f;
        baseDir.Normalize();

        if (tornadoFiveWayVfxPrefab == null)
            return;

        GameObject vfx = Instantiate(
            tornadoFiveWayVfxPrefab,
            tornadoSpawnPoint.position,
            Quaternion.LookRotation(baseDir)
        );

        SoundManager.Instance.PlaySound("DragonTornado");
        Invoke(nameof(StopDragonTornadoSound), 3f);

        DragonTornadoParticleDamage[] damages =
            vfx.GetComponentsInChildren<DragonTornadoParticleDamage>(true);

        foreach (DragonTornadoParticleDamage damage in damages)
        {
            damage.Init(
                this,
                tornadoProjectileDamage,
                targetLayer
            );
        }

        Destroy(vfx, tornadoProjectileLifeTime);
    }

    //fbx 모음들
    public void PlayLeftWingSlamEffect()
    {
        SpawnWingSlamEffectByBossOffset(
            leftWingSlamEffectPrefab,
            leftWingSlamLocalOffset
        );
    }

    public void PlayRightWingSlamEffect()
    {
        SpawnWingSlamEffectByBossOffset(
            rightWingSlamEffectPrefab,
            rightWingSlamLocalOffset
        );
    }

    private void SpawnWingSlamEffectByBossOffset(
    GameObject prefab,
    Vector3 localOffset)
    {
        if (prefab == null)
            return;

        Vector3 spawnPos = transform.TransformPoint(localOffset);
        float yOffset = localOffset.y;

        if (Physics.Raycast(
            spawnPos + Vector3.up * 10f,
            Vector3.down,
            out RaycastHit hit,
            50f,
            groundLayer))
        {
            spawnPos = hit.point;
        }

        spawnPos.y += yOffset;

        Quaternion rot = transform.rotation * prefab.transform.rotation;

        GameObject effect = Instantiate(
            prefab,
            spawnPos,
            rot
        );

        effect.transform.SetParent(null);
        effect.transform.localScale *= wingSlamEffectScale;

        ParticleSystem[] particles =
            effect.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in particles)
        {
            ParticleSystem.MainModule main = ps.main;
            main.simulationSpeed = wingSlamEffectSpeed;
            ps.Play(true);
        }

        Destroy(effect, wingSlamEffectLifeTime);
    }

    public void PlayLeftTailAttackEffect()
    {
        SpawnTailAttackEffect(leftTailAttackEffectPrefab, leftTailAttackEffectPoint);
    }

    public void PlayRightTailAttackEffect()
    {
        SpawnTailAttackEffect(rightTailAttackEffectPrefab, rightTailAttackEffectPoint);
    }

    //꼬리 이펙트
    private void SpawnTailAttackEffect(GameObject prefab, Transform spawnPoint)
    {
        if (prefab == null || spawnPoint == null)
            return;

        GameObject effect = Instantiate(
            prefab,
            spawnPoint.position,
            prefab.transform.rotation
        );

        effect.transform.SetParent(null);

        effect.transform.localScale *= tailAttackEffectScale;

        ParticleSystem[] particles =
            effect.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in particles)
        {
            ParticleSystem.MainModule main = ps.main;
            main.simulationSpeed = tailAttackEffectSpeed;
            ps.Play(true);
        }

        Destroy(effect, tailAttackEffectLifeTime);
    }

    public void PlayBreathChargeExplosionEffect()
    {
        if (breathChargeExplosionEffectPrefab == null)
            return;

        Vector3 spawnPos = transform.position;

        if (Physics.Raycast(
            transform.position + Vector3.up * 5f,
            Vector3.down,
            out RaycastHit hit,
            20f))
        {
            spawnPos = hit.point;
        }

        GameObject effect = Instantiate(
            breathChargeExplosionEffectPrefab,
            spawnPos,
            breathChargeExplosionEffectPrefab.transform.rotation
        );

        effect.transform.localScale *= breathChargeExplosionEffectScale;

        ParticleSystem[] particles =
            effect.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in particles)
        {
            ParticleSystem.MainModule main = ps.main;
            main.simulationSpeed = breathChargeExplosionEffectSpeed;
            ps.Play(true);
        }

        Destroy(effect, breathChargeExplosionEffectLifeTime);
    }


    public void PlayJumpStartDustEffect()
    {
        SpawnEffectOnGround(
            jumpStartDustEffectPrefab,
            transform.position,
            jumpStartDustEffectScale,
            jumpStartDustEffectSpeed,
            jumpStartDustEffectLifeTime
        );
    }

    public void PlayJumpLandImpactEffect(Vector3 position)
    {
        SpawnEffectOnGround(
            jumpLandImpactEffectPrefab,
            position,
            jumpLandImpactEffectScale,
            jumpLandImpactEffectSpeed,
            jumpLandImpactEffectLifeTime
        );
    }

    private void SpawnEffectOnGround(
      GameObject prefab,
      Vector3 position,
      float scale,
      float speed,
      float lifeTime)
    {
        if (prefab == null)
            return;

        Vector3 spawnPos = position;

        if (Physics.Raycast(
            position + Vector3.up * 10f,
            Vector3.down,
            out RaycastHit hit,
            50f,
            meteorGroundLayer))
        {
            spawnPos = hit.point;
        }

        spawnPos.y += 0.05f;

        GameObject effect = Instantiate(
            prefab,
            spawnPos,
            prefab.transform.rotation
        );

        effect.transform.SetParent(null);
        effect.transform.localScale *= scale;

        ParticleSystem[] particles =
            effect.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in particles)
        {
            ParticleSystem.MainModule main = ps.main;
            main.simulationSpeed = speed;
            ps.Play(true);
        }

        Destroy(effect, lifeTime);
    }
    //포효하기 fbx
    public void PlayRoarEffect()
    {
        if (roarEffectPrefab == null)
            return;

        Vector3 spawnPos = transform.position;

        if (Physics.Raycast(
            transform.position + Vector3.up * 5f,
            Vector3.down,
            out RaycastHit hit,
            30f,
            groundLayer))
        {
            spawnPos = hit.point;
        }

        GameObject effect = Instantiate(
            roarEffectPrefab,
            spawnPos,
            roarEffectPrefab.transform.rotation
        );

        effect.transform.SetParent(null);
        effect.transform.localScale *= roarEffectScale;

        ParticleSystem[] particles =
            effect.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in particles)
        {
            ParticleSystem.MainModule main = ps.main;
            main.simulationSpeed = roarEffectSpeed;
            ps.Play(true);
        }

        Destroy(effect, roarEffectLifeTime);
    }

    //메테오 이펙트 
    public void PlayMeteorEffect()
    {
        SpawnMeteorEffect();
    }

    public void PlayMeteorEvent()
    {
        PlayDragonMeteor2SoundOnce();

        PlayMeteorEffect();

        StartCoroutine(MeteorWarningSpawnRoutine());
    }
    //메테오 이펙트

    private GameObject SpawnMeteorEffect()
    {
        if (meteorEffectPrefab == null)
            return null;

        GameObject effect = Instantiate(
            meteorEffectPrefab,
            transform.position,
            meteorEffectPrefab.transform.rotation
        );

        effect.transform.SetParent(null);
        effect.transform.localScale *= meteorEffectScale;

        ParticleSystem[] particles =
            effect.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in particles)
        {
            ParticleSystem.MainModule main = ps.main;
            main.simulationSpeed = meteorEffectSpeed;
            ps.Play(true);
        }

        Destroy(effect, meteorEffectLifeTime);

        return effect;
    }

    private void SpawnWingCrushEffect(GameObject prefab, Transform point)
    {
        if (prefab == null || point == null)
            return;

        Vector3 spawnPos = point.position;

        if (Physics.Raycast(
            point.position,
            Vector3.down,
            out RaycastHit hit,
            50f,
            groundLayer))
        {
            spawnPos = hit.point;
        }

        spawnPos.y += 0.05f;

        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Quaternion rot = Quaternion.LookRotation(forward) * Quaternion.Euler(0f, -90f, 0f);
        GameObject effect = Instantiate(
            prefab,
            spawnPos,
            rot
        );

        effect.transform.SetParent(null);
        effect.transform.localScale *= wingCrushEffectScale;

        ParticleSystem[] particles =
            effect.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in particles)
        {
            ParticleSystem.MainModule main = ps.main;
            main.simulationSpeed = wingCrushEffectSpeed;
            ps.Play(true);
        }

        Destroy(effect, wingCrushEffectLifeTime);
    }

    public bool IsPhase1()
    {
        return !isPhase2;
    }

    public bool IsPhase2()
    {
        return isPhase2;
    }

    public void PlayDustEffect()
    {
        if (dustEffectPrefab == null)
            return;

        Vector3 spawnPos = transform.position + Vector3.down * 0.5f;

        if (Physics.Raycast(
            transform.position + Vector3.up,
            Vector3.down,
            out RaycastHit hit,
            10f,
            meteorGroundLayer))
        {
            spawnPos = hit.point;
        }

        GameObject effect = Instantiate(
            dustEffectPrefab,
            spawnPos,
            Quaternion.identity
        );

        effect.transform.localScale *= dustEffectScale;

        Destroy(effect, dustEffectLifeTime);
    }


    #region Dragon Sound Animation Event

    public void PlayDragonBiteSound()
    {
        SoundManager.Instance.PlaySound("DragonBite");
    }

    public void PlayDragonWingSlamSound()
    {
        SoundManager.Instance.PlaySound("DragonWingSlam");
    }

    public void PlayDragonWingCrushSound()
    {
        SoundManager.Instance.PlaySound("DragonWingCrush");
    }

    public void StartDragonBreathSound()
    {
        SoundManager.Instance.PlaySound("DragonBreath");
    }

    public void StopDragonBreathSound()
    {
        SoundManager.Instance.StopSound("DragonBreath");
    }

    public void PlayDragonFireballShootSound()
    {
        SoundManager.Instance.PlaySound("DragonFireballShoot");
    }

    public void PlayDragonFireballExplosionSound()
    {
        SoundManager.Instance.PlaySound("DragonFireballExplosion");
    }

    public void PlayDragonRoarSound()
    {
        if (isPhase2)
            SoundManager.Instance.PlaySound("DragonRoar2");
        else
            SoundManager.Instance.PlaySound("DragonRoar");
    }

    public void PlayDragonRoar2Sound()
    {
        SoundManager.Instance.PlaySound("DragonRoar2");
    }

    public void PlayDragonJumpSound()
    {
        SoundManager.Instance.PlaySound("DragonJump");
    }
    public void PlayDragonJumpDownSound()
    {
        SoundManager.Instance.PlaySound("DragonJumpDown");
    }

    public void PlayDragonMeteor1Sound()
    {
        SoundManager.Instance.PlaySound("Meteor1");
    }
    public void PlayDragonMeteor2Sound()
    {
        SoundManager.Instance.PlaySound("Meteor2");
    }
    public void PlayDragonMeteor3Sound()
    {
        SoundManager.Instance.PlaySound("Meteor3");
    }

    public void PlayDragonChargeSound()
    {
        SoundManager.Instance.PlaySound("DragonCharge1");
    }

    public void PlayDragonChargeBombSound()
    {
        SoundManager.Instance.PlaySound("DragonChargeBomb");
    }

    public void PlayDragonDashSound()
    {
        SoundManager.Instance.PlaySound("DragonDash");
    }

    public void PlayDragonWeakPointSound()
    {
        SoundManager.Instance.PlaySound("DragonWeakPoint");
    }

    public void StartDragonWingFlapSound()
    {
        SoundManager.Instance.PlaySound("DragonWingFlap");
    }

    public void StopDragonWingFlapSound()
    {
        SoundManager.Instance.StopSound("DragonWingFlap");
    }

    public void StartDragonTailAttackSound()
    {
        SoundManager.Instance.PlaySound("DragonTailAttack");
    }

    public void StopDragonTornadoSound()
    {
        SoundManager.Instance.StopSound("DragonTornado");
    }


    private IEnumerator DelayedChargeSound()
    {
        yield return new WaitForSeconds(1f);

        PlayDragonChargeSound();
    }

    public void PlayDragonDieSound()
    {
        SoundManager.Instance.PlaySound("DragonDie");
    }

    #endregion
}
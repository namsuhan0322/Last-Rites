using UnityEngine;
using UnityEngine.AI;

public class DragonBoss : Enemy
{
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

    [Header("날개 내려찍기 패턴")]
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
    public DragonAttackHitbox leftWingCrushHitbox;
    public DragonAttackHitbox rightWingCrushHitbox;

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

    [Header("브레스 중 약점")]
    [SerializeField] private WeakPoint headWeakPoint;
    [SerializeField] private int headWeakPointHP = 100;

    [Header("브레스 차징 패턴")]
    public float breathCastTime = 3.5f;
    public float breathChargeTime = 2.5f;
    public float breathChargeCooldown = 12f;
    public GameObject breathChargePrefab;
    [Header("브레스 차징 FX")]
    public float breathChargeFxLifeTime = 2f;


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
        return globalAttackRecoveryTimer <= 0f;
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

    //날개를 내려찍을수 있나?
    public bool CanWingSlam()
    {
        return wingSlamCooldownTimer <= 0f;
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

    public void StartWingSlamCooldown()
    {
        wingSlamCooldownTimer = wingSlamCooldown;
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
        animator.SetTrigger("SkyLoop");
    }

    public void PlayJumpFall()
    {
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
        return wingCrushBreathCooldownTimer <= 0f;
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
        return wingSlamBiteComboCooldownTimer <= 0f;
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
        return wingSlamBiteSideBreathComboCooldownTimer <= 0f;
    }

    public void StartWingSlamBiteSideBreathComboCooldown()
    {
        wingSlamBiteSideBreathComboCooldownTimer = wingSlamBiteSideBreathComboCooldown;
    }


    //좌우로 점프 하기
    public bool CanSideJump()
    {
        return sideJumpCooldownTimer <= 0f;
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
        return breathChargeCooldownTimer <= 0f;
    }

    public void StartBreathChargeCooldown()
    {
        breathChargeCooldownTimer = breathChargeCooldown;
    }

    public void PlayBreathCast()
    {
        animator.ResetTrigger("BreathCast");
        animator.SetTrigger("BreathCast");
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
        StopBreathCharge();

        animator.ResetTrigger("EndBreathCharge");
        animator.SetTrigger("EndBreathCharge");

        Idle();
    }
    //포효하기
    public bool ShouldRoar()
    {
        return !HasRoared || roarRequested;
    }

    public void ClearRoarRequest()
    {
        roarRequested = false;
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

    //죽음
    public override void TakeDamage(int damage, float severityOverride = -1f, bool isHeavyAttack = false, bool showDamageText = true)
    {
        base.TakeDamage(damage, severityOverride, isHeavyAttack, showDamageText);

        if (_isDead) return;

        if (!HasRoared)
        {
            roarRequested = true;
            StopMove();
            Idle();
        }
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

    public void EnableLeftWingCrushHitbox()
    {
        leftWingCrushHitbox.EnableHitbox();
    }

    public void DisableLeftWingCrushHitbox()
    {
        leftWingCrushHitbox.DisableHitbox();
    }

    public void EnableRightWingCrushHitbox()
    {
        rightWingCrushHitbox.EnableHitbox();
    }

    public void DisableRightWingCrushHitbox()
    {
        rightWingCrushHitbox.DisableHitbox();
    }

    public void DisableAllWingCrushHitboxes()
    {
        leftWingCrushHitbox.DisableHitbox();
        rightWingCrushHitbox.DisableHitbox();
    }


    public void SetManualMoveMode(bool value)
    {
        if (agent != null)
        {
            agent.updatePosition = !value;
            agent.updateRotation = !value;
        }
    }

}
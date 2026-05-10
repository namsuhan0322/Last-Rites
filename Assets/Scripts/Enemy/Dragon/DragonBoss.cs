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

    public bool GetRandomPatrolPoint(out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * PatrolRadius;

            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
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



    //포효하기
    public bool ShouldRoar()
    {
        return !HasRoared || roarRequested;
    }

    public void ClearRoarRequest()
    {
        roarRequested = false;
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

}
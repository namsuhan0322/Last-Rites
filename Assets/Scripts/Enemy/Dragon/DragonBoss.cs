using UnityEngine;
using UnityEngine.AI;

public class DragonBoss : Enemy
{

    [Header("스킬후 공통 현타시간")]
    public float combatIdleTime = 2f;

    [Header("Dragon Boss Move")]
    public float turnSpeed = 80f;
    public float turnDuration = 1.2f;

    [Header("Target")]
    public float playerDetectRange = 15f;
    public float targetLoseRange = 25f;
    public float faceFinishAngle = 5f;
    public float faceCooldown = 1.2f;

    [Header("Roar")]
    public float roarDuration = 2.5f;

    private int currentMoveType = -1;
    private Transform lockedTarget;
    private float faceCooldownTimer = 0f;
    private bool hasRoared = false;


    [Header("전방 깨물기 패턴")]
    public float biteRange = 4f;
    public float biteDuration = 1.5f;
    public float biteCooldown = 2f;
    [SerializeField] private DragonHeadHitbox headHitbox;

    [Header("날개 내려찍기 패턴")]
    public float wingSlamRange = 7f;
    public float wingSlamDuration = 2f;
    public float wingSlamCooldown = 8f;
    [Header("날개 히트박스")]
    [SerializeField] private DragonWingHitbox leftWingHitbox;
    [SerializeField] private DragonWingHitbox rightWingHitbox;
    public bool HasRoared => hasRoared;
    private float biteCooldownTimer = 0f;
    private Vector3 lockedAttackPosition;
    private float wingSlamCooldownTimer = 0f;
    private float globalAttackRecoveryTimer = 0f;

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
    
    public void EnableRightWingHitbox()
    {
        rightWingHitbox.EnableHitbox();
    }

    public void DisableRightWingHitbox()
    {
        rightWingHitbox.DisableHitbox();
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

}
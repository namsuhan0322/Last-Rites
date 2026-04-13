using UnityEngine;
using System;

public class Actor : MonoBehaviour
{
    [SerializeField] protected int _currentHP;
    protected int _maxHP;

    protected bool _isDead = false;
    public bool IsDead => _isDead;

    //힐러가 ai나 플레이어의 체력을 알 수 있게 읽기
    public int CurrentHP => _currentHP;
    public int MaxHP => _maxHP;
    public bool IsFullHP => _currentHP >= _maxHP;

    private int damageReduction = 0;
    private float damageReduceTimer = 0f;

    [SerializeField] protected float _maxPoise = 20f;               // 최대 강인도
    [SerializeField] protected float _currentPoise;                 // 현재 강인도
    [SerializeField] protected float _poiseRecoveryRate = 0f;       // 자동회복

    [Header("무적 타이머")]
    [SerializeField] protected bool _isInvincible = false;
    [SerializeField] protected float _invincibleTimer = 0f;

    public event Action<int, int> OnHPChanged;
    public event Action OnDeath;
    public event Action<float> OnHit;
    public event Action OnStun;

    protected Animator animator;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
    }

    protected virtual void Start() 
    {
        _currentPoise = _maxPoise;
    }

    protected virtual void Update()
    {
        if (damageReduceTimer > 0f)
        {
            damageReduceTimer -= Time.deltaTime;

            if (damageReduceTimer <= 0f)
            {
                damageReduction = 0;
                Debug.Log($"{name} 데미지 감소 끝");
            }
        }

        if (_poiseRecoveryRate > 0 && _currentPoise < _maxPoise)
        {
            _currentPoise += _poiseRecoveryRate * Time.deltaTime;
            if (_currentPoise > _maxPoise) _currentPoise = _maxPoise;
        }

        if (_invincibleTimer > 0f)
        {
            _invincibleTimer -= Time.deltaTime;
            if (_invincibleTimer <= 0f)
            {
                _isInvincible = false;
                _invincibleTimer = 0f;
            }
        }
    }

    public virtual void InitActor(int maxHP)
    {
        Debug.Log($"[InitActor] {name} HP RESET to {maxHP}");

        _maxHP = maxHP;
        _currentHP = maxHP;
        _isDead = false;
        _currentPoise = _maxPoise;

        OnHPChanged?.Invoke(_currentHP, _maxHP);
    }

    public virtual void TakeDamage(int damage, float severityOverride = -1f, bool isHeavyAttack = false)
    {
        if (_isDead) return;
        if (_isInvincible)
        {
            Debug.Log($"{name} : 무적 상태 회피 성공!");
            return;
        }
        if (damage <= 0) return;

        damage -= damageReduction;
        if (damage < 0) damage = 0;

        _currentHP -= damage;
        OnHPChanged?.Invoke(_currentHP, _maxHP);

        float poiseDamage = damage * 0.5f;
        TakePoiseDamage(poiseDamage);

        if (_currentHP <= 0)
            Die();
        else
        {
            // [변경 사항] 큰 패턴에 맞았다면 강인도 상관없이 즉시 스턴!
            if (isHeavyAttack)
            {
                OnStun?.Invoke();
            }
            // 큰 패턴이 아닐 때만 일반 피격 이벤트 발생
            else if (_currentPoise > 0)
            {
                float hitSeverity = (severityOverride != -1f) ? severityOverride : CalculateHitSeverity(damage);
                OnHit?.Invoke(hitSeverity);
            }
        }
    }

    public virtual void Heal(int amount)
    {
        if (_isDead) return;
        if (amount <= 0) return;

        int before = _currentHP;

        _currentHP = Mathf.Clamp(_currentHP + amount, 0, _maxHP);

        if (_currentHP != before)
            OnHPChanged?.Invoke(_currentHP, _maxHP);
    }

    protected virtual void Die()
    {
        _isDead = true;
        OnDeath?.Invoke();

        if (animator != null)
            animator.SetTrigger("Die");
    }

    public void AddDamageReduction(int amount, float duration)
    {
        damageReduction = amount;
        damageReduceTimer = duration;

        Debug.Log($"{name} Damage Reduction {amount} for {duration}s");
    }

    protected virtual float CalculateHitSeverity(int damage)
    {
        if (damage >= 30) return 1.0f;      // 강한 데미지 (Critical)
        if (damage >= 10) return 0.5f;      // 중간 데미지
        return 0.0f;                        // 약한 데미지
    }

    public virtual void TakePoiseDamage(float amount)
    {
        _currentPoise -= amount;
        Debug.Log($"[강인도] {_currentPoise} / {_maxPoise}");

        if (_currentPoise <= 0)
        {
            _currentPoise = 0;              // 혹은 -로 내려가게 해서 회복 오래 걸리게 하기도 함
            OnStun?.Invoke();               // 스턴 발생!
            _currentPoise = _maxPoise;      // 스턴 터지면 강인도 초기화 (바로 또 스턴 안 걸리게)
        }
    }

    public void SetInvincible(bool value)
    {
        _isInvincible = value;
        if (!value) _invincibleTimer = 0f; // 끌 때는 타이머도 초기화
    }

    // 일정 시간(예: 0.5초) 동안만 무적을 유지하는 함수
    public void SetInvincibleForSeconds(float seconds)
    {
        _isInvincible = true;
        _invincibleTimer = seconds;
    }

    public void DrainHP(int amount)
    {
        _currentHP -= amount;
        if (_currentHP <= 0)
        {
            _currentHP = 0;
            Die();
        }

        OnHPChanged?.Invoke(_currentHP, MaxHP);
    }
}
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

    int damageReduction = 0;
    float damageReduceTimer = 0f;

    public event Action<int, int> OnHPChanged;
    public event Action OnDeath;

    protected Animator animator;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
    }

    protected virtual void Start() { }

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
    }

    public virtual void InitActor(int maxHP)
    {
        Debug.Log($"[InitActor] {name} HP RESET to {maxHP}");


        _maxHP = maxHP;
        _currentHP = maxHP;
        _isDead = false;

        OnHPChanged?.Invoke(_currentHP, _maxHP);
    }

    public virtual void TakeDamage(int damage)
    {
        if (_isDead) return;
        if (damage <= 0) return;

        damage -= damageReduction;
        if (damage < 0) damage = 0;

        _currentHP -= damage;

        OnHPChanged?.Invoke(_currentHP, _maxHP);

        if (_currentHP <= 0)
            Die();
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
        Debug.Log($"{gameObject.name} Died.");
    }

    public void AddDamageReduction(int amount, float duration)
    {
        damageReduction = amount;
        damageReduceTimer = duration;

        Debug.Log($"{name} Damage Reduction {amount} for {duration}s");
    }
}
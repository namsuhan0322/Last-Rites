using UnityEngine;
using System;

public class Actor : MonoBehaviour
{
    [SerializeField] protected int _currentHP;
    protected int _maxHP;
    protected bool _isDead = false;

    public event Action<int, int> OnHPChanged;
    public event Action OnDeath;

    protected virtual void Start() { }

    public virtual void InitActor(int maxHP)
    {
        _maxHP = maxHP;
        _currentHP = maxHP;
        _isDead = false;
    }

    public virtual void TakeDamage(int damage)
    {
        if (_isDead) return;

        _currentHP -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Current HP: {_currentHP}");

        OnHPChanged?.Invoke(_currentHP, _maxHP);

        if (_currentHP <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        _isDead = true;
        OnDeath?.Invoke();
        Debug.Log($"{gameObject.name} Died.");
    }
}
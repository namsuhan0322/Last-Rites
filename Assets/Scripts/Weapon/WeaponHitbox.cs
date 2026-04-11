using UnityEngine;
using System.Collections.Generic;

public class WeaponHitbox : MonoBehaviour
{
    [Header("연결할 콜라이더들 (인스펙터에서 할당)")]
    [SerializeField] private BoxCollider _greatSwordCol; // 대검
    [SerializeField] private BoxCollider _spearCol;      // 창
    [SerializeField] private BoxCollider _swordCol;      // 한손검
    [SerializeField] private BoxCollider _dualBladeRCol; // 쌍검(우)
    [SerializeField] private BoxCollider _dualBladeLCol; // 쌍검(좌)
    [SerializeField] private BoxCollider _shieldCol;     // 방패

    // 실제로 사용할 콜라이더 목록
    private List<BoxCollider> _activeColliders = new List<BoxCollider>();

    private int _damage;
    private List<Actor> _hitActors = new List<Actor>();

    private void Awake()
    {
        DisableAll();
    }

    public void SetupColliders(WeaponType type)
    {
        _activeColliders.Clear();
        DisableAll();

        switch (type)
        {
            case WeaponType.GreatSword:
                if (_greatSwordCol) _activeColliders.Add(_greatSwordCol);
                break;

            case WeaponType.DualBlade:
                if (_dualBladeRCol) _activeColliders.Add(_dualBladeRCol);
                if (_dualBladeLCol) _activeColliders.Add(_dualBladeLCol);
                break;

            case WeaponType.Spear:
                if (_spearCol) _activeColliders.Add(_spearCol);
                break;

            case WeaponType.SwordShield:
                if (_swordCol) _activeColliders.Add(_swordCol);
                if (_shieldCol) _activeColliders.Add(_shieldCol);
                break;
        }
    }

    // 공격 시 켜기
    public void EnableHitbox(int damage)
    {
        _damage = damage;
        _hitActors.Clear(); 

        foreach (var col in _activeColliders)
        {
            if (col != null) col.enabled = true;
        }
    }

    // 공격 종료 시 끄기
    public void DisableHitbox()
    {
        foreach (var col in _activeColliders)
        {
            if (col != null) col.enabled = false;
        }
        _hitActors.Clear();
    }

    private void DisableAll()
    {
        if (_greatSwordCol) _greatSwordCol.enabled = false;
        if (_spearCol) _spearCol.enabled = false;
        if (_swordCol) _swordCol.enabled = false;
        if (_dualBladeRCol) _dualBladeRCol.enabled = false;
        if (_dualBladeLCol) _dualBladeLCol.enabled = false;
        if (_shieldCol) _shieldCol.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        if (other.CompareTag("Enemy"))
        {
            Actor enemy = other.GetComponentInParent<Actor>();
            if (enemy != null && !enemy.IsDead && !_hitActors.Contains(enemy))
            {
                enemy.TakeDamage(_damage);
                _hitActors.Add(enemy);
                Debug.Log($"[Hit] {_damage} 데미지!");
            }
        }
    }
}
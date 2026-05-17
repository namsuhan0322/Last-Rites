using UnityEngine;
using System.Collections.Generic;

public class WeaponHitbox : MonoBehaviour
{
    [Header("연결할 콜라이더들 (인스펙터에서 할당)")]
    [SerializeField] private BoxCollider _greatSwordCol; // 대검
    [SerializeField] private BoxCollider _spearCol;      // 창
    [SerializeField] private BoxCollider _dualBladeRCol; // 쌍검(우)
    [SerializeField] private BoxCollider _dualBladeLCol; // 쌍검(좌)

    // 실제로 사용할 콜라이더 목록
    private List<BoxCollider> _activeColliders = new List<BoxCollider>();

    private int _damage;
    private List<Actor> _hitActors = new List<Actor>();
    private bool _isAttackActive = false;

    private WeaponType _currentWeaponType;

    private void Awake()
    {
        DisableAll();
    }

    public void SetupColliders(WeaponType type)
    {
        _activeColliders.Clear();
        DisableAll();

        _currentWeaponType = type;

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
        }
    }

    // 공격 시 켜기
    public void EnableHitbox(int damage)
    {
        _isAttackActive = true;
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
        _isAttackActive = false;
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
        if (_dualBladeRCol) _dualBladeRCol.enabled = false;
        if (_dualBladeLCol) _dualBladeLCol.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isAttackActive) return;

        Actor enemy = other.GetComponentInParent<Actor>();
        if (enemy != null && !enemy.IsDead && !_hitActors.Contains(enemy))
        {
            if (enemy.GetComponent<PlayerController>() != null) return;

            enemy.TakeDamage(_damage);
            _hitActors.Add(enemy);
            PlayHitSound(enemy.transform.position);
        }
    }

    private void PlayHitSound(Vector3 hitPosition)
    {
        if (SFXManager.Instance == null) return;

        switch (_currentWeaponType)
        {
            case WeaponType.GreatSword:
                SFXManager.Instance.PlaySFX("GS_002", hitPosition);
                break;

            case WeaponType.DualBlade:
                SFXManager.Instance.PlaySFX("DB_002", hitPosition);
                break;

            case WeaponType.Spear:
                SFXManager.Instance.PlaySFX("SP_002", hitPosition);
                break;
        }
    }
}
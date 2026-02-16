using UnityEngine;
using System.Collections.Generic;

public class WeaponHitbox : MonoBehaviour
{
    [SerializeField] private BoxCollider _collider;

    private int _damage;

    private List<Actor> _hitActors = new List<Actor>();

    private void Awake()
    {
        if (_collider == null) _collider = GetComponent<BoxCollider>();

        _collider.enabled = false;
        _collider.isTrigger = true;
    }

    // 공격 시작
    public void EnableHitbox(int damage)
    {
        _damage = damage;           // 데미지 주입 받음
        _hitActors.Clear();         // 맞은 목록 초기화
        _collider.enabled = true;   // 판정 켜기
    }

    // 공격 종료
    public void DisableHitbox()
    {
        _collider.enabled = false;
        _hitActors.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"무언가와 닿음: {other.name}");

        if (other.CompareTag("Enemy"))
        {
            Actor enemy = other.GetComponent<Actor>();

            if (enemy != null && !enemy.IsDead && !_hitActors.Contains(enemy))
            {
                enemy.TakeDamage(_damage);

                _hitActors.Add(enemy);

                Debug.Log($"[Hit] 적에게 {_damage} 데미지!");
            }
            else
            {
                Debug.Log($"때렸으나 데미지 안 들어감 (Actor: {enemy}, IsDead: {enemy?.IsDead})");
            }
        }
    }
}
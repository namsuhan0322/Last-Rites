using UnityEngine;

/// <summary>
/// [역할] 공격 실행: 데미지 계산(PlayerDamageCalculator) → 마법사면 마법탄, 아니면 히트박스 → 창 버프면 추가 투사체.
///        엑스칼리버(창 V) 범위 공격 생성.
/// [이유] EnableWeaponCollider의 실행부와 투사체·범위 공격 생성을 한곳에 모은다.
/// [근거] 명세 3.3 PlayerAttack(결정 R10), R27 / 이상한 동작 Q2·Q6 유지, Q11은 버그 수정 3단계에서 고침(B8)
/// </summary>
public class PlayerAttack
{
    private readonly PlayerController _owner;
    private readonly WizardCaster _wizard;

    public PlayerAttack(PlayerController owner, WizardCaster wizard)
    {
        _owner = owner;
        _wizard = wizard;
    }

    /// EnableWeaponCollider의 실행부. 공격·스킬 상태인지 검사는 창구(PlayerController)에서 한다.
    /// hitState = 이 타격을 낸 애니메이션의 상태 (Q11 고침)
    public void EnableHit(AnimatorStateInfo hitState)
    {
        if (_owner.Hitbox == null || _owner.CurrentWeapon == null) return;

        WeaponSO weapon = _owner.CurrentWeapon;
        int baseAtk = _owner.Stats != null ? _owner.Stats.BaseAttackPower : 0;
        int weaponAtk = _owner.CurrentDecoratedWeapon != null ? _owner.CurrentDecoratedWeapon.GetAttackPower() : weapon.Combo_1;

        // 공식은 PlayerDamageCalculator (명세 3.2). 여기서는 애니메이터 상태로 '어떤 타격인지'만 고른다
        int damageToDeal = PlayerDamageCalculator.WeaponHit(baseAtk, weaponAtk, GetHitKind(hitState),
            weapon.Combo_1, weapon.Combo_2, weapon.Combo_3, _owner.CurrentSkillDamage);

        // R 버프(증폭) 적용
        damageToDeal = PlayerDamageCalculator.ApplyGreatSwordBuff(damageToDeal, _owner.HasGreatSwordBuff, _owner.CurrentSkillVal);

        // 히트박스 켜기 (마법사는 히트박스 대신 마법탄)
        if (weapon.weaponType == WeaponType.Wizard)
        {
            _wizard.FireBasicMagic(damageToDeal);
        }
        else
        {
            if (_owner.Hitbox != null) _owner.Hitbox.EnableHitbox(damageToDeal, _owner.EnemyLayer);   // 적 레이어만 검사 (공격 판정 H5)
        }

        if (_owner.HasSpearBuff)
        {
            FireSpearBuffProjectile(PlayerDamageCalculator.SpearProjectile(damageToDeal)); // 창기는 데미지의 50%만
        }
    }

    // 타격을 낸 애니의 상태 → 타격 종류. 판정 순서(Attack1 → Attack2 → Attack3 → Skill 태그)는 그대로
    private static AttackHitKind GetHitKind(AnimatorStateInfo stateInfo)
    {
        if (stateInfo.IsName(PlayerAnimParams.StateAttack1)) return AttackHitKind.Combo1;
        if (stateInfo.IsName(PlayerAnimParams.StateAttack2)) return AttackHitKind.Combo2;
        if (stateInfo.IsName(PlayerAnimParams.StateAttack3)) return AttackHitKind.Combo3;
        if (stateInfo.IsTag(PlayerAnimParams.TagSkill)) return AttackHitKind.Skill;
        return AttackHitKind.Other;
    }

    public void DisableHit()
    {
        if (_owner.Hitbox != null)
        {
            _owner.Hitbox.DisableHitbox();
        }
    }

    private void FireSpearBuffProjectile(int damage)
    {
        GameObject prefab = _owner.spearBuffProjectilePrefab;
        Transform bodyPos = _owner.bodyEffectPos;
        if (prefab == null || bodyPos == null) return;

        GameObject projObj = Object.Instantiate(prefab, bodyPos.position, _owner.transform.rotation);

        ISkillDamageObject projectile = projObj.GetComponent<ISkillDamageObject>();   // 창 투사체 프리팹의 Projectile
        if (projectile != null)
        {
            projectile.Initialize(damage, _owner.EnemyLayer);
        }
    }

    /// 창 V 애니메이션 이벤트 UseExcaliburSkill → 앞쪽 spawnDistance(5m)에 범위 공격 생성. 데미지는 스킬뎀만 (Q6)
    public void FireExcalibur()
    {
        GameObject prefab = _owner.excaliburSkillPrefab;
        if (prefab == null) return;

        Transform tr = _owner.transform;
        Vector3 spawnPosition = tr.position + tr.forward * _owner.spawnDistance;

        GameObject skillInstance = Object.Instantiate(prefab, spawnPosition, tr.rotation);

        ISkillDamageObject aoe = skillInstance.GetComponent<ISkillDamageObject>();   // 엑스칼리버 프리팹의 AoESkillEffect
        if (aoe != null)
        {
            int damageToDeal = PlayerDamageCalculator.SpawnedSkill(_owner.CurrentSkillDamage, _owner.HasGreatSwordBuff, _owner.CurrentSkillVal);

            aoe.Initialize(damageToDeal, _owner.EnemyLayer);
        }
    }
}

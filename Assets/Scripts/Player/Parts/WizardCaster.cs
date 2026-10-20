using UnityEngine;

/// <summary>
/// [역할] 마법사 전용: 평타 마법탄, Q/W/E/V 스킬 오브젝트 생성·효과음, E 보호막 오브젝트 관리와 방어 처리.
/// [이유] 마법사만 쓰는 코드가 PlayerController 여기저기 섞여 있던 것을 한곳에 모은다.
/// [근거] 명세 3.3 WizardCaster, 결정 R4, R27 / 이상한 동작 Q4·Q7 유지
/// </summary>
public class WizardCaster
{
    private const float SpawnDuplicateGuard = 0.1f;   // 0.1초 안에 다시 불리면 무시 (모든 마법사 스킬이 하나를 같이 씀)

    private readonly PlayerController _owner;
    private readonly WizardShieldCounter _shield;
    private float _lastMagicFireTime = 0f;
    private GameObject _shieldInstance;

    public WizardCaster(PlayerController owner, WizardShieldCounter shield)
    {
        _owner = owner;
        _shield = shield;
    }

    /// 마법사 평타·스킬 애니의 EnableWeaponCollider → 히트박스 대신 마법탄 (데미지는 히트박스형 공식)
    public void FireBasicMagic(int damage)
    {
        GameObject prefab = _owner.wizardVfxProjectilePrefab;
        if (prefab == null) return;

        GameObject projObj = Object.Instantiate(prefab, _owner.bodyEffectPos.position, _owner.transform.rotation);

        ISkillDamageObject magic = projObj.GetComponent<ISkillDamageObject>();   // 마법탄 프리팹의 VFXProjectile
        if (magic != null)
        {
            magic.Initialize(damage, _owner.EnemyLayer);
        }
    }

    // 효과음 위치는 생성 위치. 생성 위치가 비어 있으면 생성은 건너뛰지만 효과음 줄에서 오류가 난다 (Q7, 유지)
    public void FireQ()
    {
        SpawnMagicSkill(_owner.wizardSkillQPrefab, _owner.WizardQSpawnPoint, false);
        SFXManager.Instance?.PlaySFX("Wz_002", _owner.WizardQSpawnPoint.position);
    }

    public void FireW()
    {
        SpawnMagicSkill(_owner.wizardSkillWPrefab, _owner.WizardWSpawnPoint, false);
        SFXManager.Instance?.PlaySFX("Wz_003", _owner.WizardWSpawnPoint.position);
    }

    public void FireE()
    {
        GameObject shieldObj = SpawnMagicSkill(_owner.wizardSkillEPrefab, _owner.WizardESpawnPoint, true);

        if (shieldObj != null)
        {
            _shield.Activate(_owner.wizardShieldMaxHits);
            _shieldInstance = shieldObj;
            Debug.Log($"보호막 전개! 방어 가능 횟수: {_owner.wizardShieldMaxHits}");
            SFXManager.Instance?.PlaySFX("Wz_004", _owner.transform.position);
        }
    }

    public void FireV()
    {
        SpawnMagicSkill(_owner.wizardSkillVPrefab, _owner.WizardVSpawnPoint, false);
        SFXManager.Instance?.PlaySFX("Wz_005", _owner.WizardVSpawnPoint.position);
    }

    private GameObject SpawnMagicSkill(GameObject skillPrefab, Transform spawnPoint, bool isAttached)
    {
        if (skillPrefab == null || spawnPoint == null) return null;

        if (Time.time - _lastMagicFireTime < SpawnDuplicateGuard) return null;
        _lastMagicFireTime = Time.time;

        GameObject skillInstance;

        if (isAttached) skillInstance = Object.Instantiate(skillPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);
        else skillInstance = Object.Instantiate(skillPrefab, spawnPoint.position, spawnPoint.rotation);

        skillInstance.SetActive(true);

        int damageToDeal = PlayerDamageCalculator.SpawnedSkill(_owner.CurrentSkillDamage, _owner.HasGreatSwordBuff, _owner.CurrentSkillVal);

        // 범위 공격·레이저·블랙홀 중 붙어 있는 것에 데미지를 맡긴다 (프리팹마다 하나씩, U3)
        ISkillDamageObject damageObject = skillInstance.GetComponent<ISkillDamageObject>();
        if (damageObject != null) damageObject.Initialize(damageToDeal, _owner.EnemyLayer);

        return skillInstance;
    }

    /// Actor.TakeDamage → PlayerStats.TryBlockIncomingDamage → PlayerController.TryBlockDamageWithShield → 여기. 0 데미지도 1회 소모 (Q4)
    public bool TryBlockDamage()
    {
        if (!_shield.TryConsumeHit()) return false;
        Debug.Log($"[보호막 방어] 데미지 무시! (남은 방어 횟수: {_shield.RemainingHits})");

        if (_shield.RemainingHits <= 0)
        {
            BreakShield();
        }

        return true;
    }

    public void BreakShield()
    {
        _shield.Break();
        if (_shieldInstance != null)
        {
            Object.Destroy(_shieldInstance);
            _shieldInstance = null;
        }
        Debug.Log("보호막이 깨졌습니다! 이제부터 데미지를 입습니다.");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// [역할] 플레이어 이펙트: 공격·스킬 액션 이펙트(애니메이션 이벤트 코드별 파티클), R 버프 아우라·화면 이펙트,
///        포션 힐 이펙트, 피격 화면 효과(피 튀김·위험 모드).
/// [이유] PlayerController에 흩어져 있던 이펙트 코드를 한곳에 모은다.
/// [근거] 명세 3.3 PlayerActionEffects, 결정 R4, R27
/// </summary>
public class PlayerActionEffects
{
    private const float HitScreenDuration = 1.5f;     // 맞으면 1.5초 동안 피 튀김
    private const float DangerHPRatio = 0.2f;         // HP 20% 이하이고 살아 있으면 위험 모드
    private const float HealEffectDuration = 2.0f;    // 힐 이펙트는 2초 뒤 끈다
    private const float RAuraDestroyDelay = 2f;       // R 아우라는 끈 뒤 2초 후 제거

    private readonly PlayerController _owner;
    private readonly Dictionary<string, ParticleSystem[]> _actionEffects = new Dictionary<string, ParticleSystem[]>();
    private GameObject _rAuraInstance;            // 몸에 생성된 R 아우라
    private GameObject _rScreenEffectInstance;    // 현재 켜진 R 화면 이펙트
    private GameObject _healInstance;
    private Coroutine _healCoroutine;

    public PlayerActionEffects(PlayerController owner)
    {
        _owner = owner;

        // 같은 코드가 두 번 등록되면 처음 것만 쓴다 (지금 Awake와 같음)
        foreach (var mapping in owner.weaponEffects)
        {
            if (!_actionEffects.ContainsKey(mapping.actionCode))
            {
                _actionEffects.Add(mapping.actionCode, mapping.particles);
            }
        }
    }

    #region 액션 이펙트
    public void Play(string actionCode)
    {
        if (_actionEffects.TryGetValue(actionCode, out ParticleSystem[] particles))
        {
            foreach (var p in particles)
            {
                PlayParticle(p);
            }
        }
        else
        {
            Debug.LogWarning($"[Effect] '{actionCode}' 이펙트가 리스트에 등록되지 않았습니다!");
        }
    }

    public void Stop(string actionCode)
    {
        if (_actionEffects.TryGetValue(actionCode, out ParticleSystem[] particles))
        {
            foreach (var p in particles)
            {
                StopParticle(p);
            }
        }
    }

    public void StopAll()
    {
        foreach (var particles in _actionEffects.Values)
        {
            foreach (var p in particles)
            {
                if (p != null)
                {
                    p.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    p.gameObject.SetActive(false);
                }
            }
        }
    }

    private void PlayParticle(ParticleSystem effect)
    {
        if (effect == null) return;
        if (!effect.gameObject.activeSelf) effect.gameObject.SetActive(true);
        effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var mainModule = effect.main;
        mainModule.simulationSpeed = _owner.Anim.speed;    // 파티클 속도 = 애니 속도 (쌍검 R 공속 반영)
        effect.Play(true);
    }

    private void StopParticle(ParticleSystem effect)
    {
        if (effect != null)
        {
            effect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
    #endregion

    #region R 버프 이펙트
    public void EnableR()
    {
        WeaponSO weapon = _owner.CurrentWeapon;
        Transform bodyPos = _owner.bodyEffectPos;
        if (weapon == null || bodyPos == null) return;

        WeaponEffectMapping mapping = _owner.weaponSpecialEffects.Find(x => x.weaponType == weapon.weaponType);

        if (mapping.auraPrefab != null)
        {
            if (_rAuraInstance != null) Object.Destroy(_rAuraInstance);

            _rAuraInstance = Object.Instantiate(mapping.auraPrefab, bodyPos.position, Quaternion.identity, bodyPos);
            _rAuraInstance.SetActive(true);
        }
        else
        {
            // 로그 문구는 지금과 같게 둔다 (결정 R1)
            Debug.LogWarning($"[PlayerController] {weapon.weaponType} 타입의 아우라 프리팹이 매핑되지 않았습니다!");
        }

        if (mapping.screenEffectObj != null)
        {
            _rScreenEffectInstance = mapping.screenEffectObj;
            _rScreenEffectInstance.SetActive(true);
        }
    }

    /// 대검·창 버프 중이면 끄지 않는 검사는 창구(PlayerController.DisableREffect)에서 한다
    public void DisableR()
    {
        if (_rAuraInstance != null)
        {
            _rAuraInstance.SetActive(false);
            Object.Destroy(_rAuraInstance, RAuraDestroyDelay);
            _rAuraInstance = null;
        }

        if (_rScreenEffectInstance != null)
        {
            _rScreenEffectInstance.SetActive(false);
            _rScreenEffectInstance = null;
        }
    }
    #endregion

    #region 힐·피격 화면
    public void PlayHeal()
    {
        GameObject healPrefab = _owner.HealEffect;
        Transform bodyPos = _owner.bodyEffectPos;
        if (healPrefab == null || bodyPos == null) return;

        if (_healInstance == null)
            _healInstance = Object.Instantiate(healPrefab, bodyPos.position, Quaternion.identity, bodyPos);

        _healInstance.SetActive(false);
        _healInstance.SetActive(true);

        ParticleSystem[] particles = _healInstance.GetComponentsInChildren<ParticleSystem>();
        foreach (var p in particles)
        {
            p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            p.Play(true);
        }

        if (_healCoroutine != null) _owner.StopCoroutine(_healCoroutine);

        _healCoroutine = _owner.StartCoroutine(DisableHealAfterDelay(HealEffectDuration));
    }

    private IEnumerator DisableHealAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_healInstance != null)
        {
            _healInstance.SetActive(false);
            _healCoroutine = null;
        }
    }

    public void PlayHitScreen()
    {
        if (_owner.screenBloodEffect != null)
        {
            _owner.screenBloodEffect.PlayHitEffect(HitScreenDuration);
        }
    }

    public void UpdateDangerScreen(int currentHP, int maxHP)
    {
        ScreenBloodController blood = _owner.screenBloodEffect;
        if (blood == null) return;

        float hpPercent = (float)currentHP / maxHP;

        bool isDanger = (hpPercent <= DangerHPRatio && currentHP > 0);
        blood.SetDangerMode(isDanger);
    }
    #endregion
}

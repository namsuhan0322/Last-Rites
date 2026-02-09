using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private WeaponSO _weaponData;
    private Animator _animator;
    private PlayerMovement _playerMovement;

    [Header("콤보 설정")]
    private int _comboStep = 0; // 현재 콤보 단계 (0=대기, 1=1타, 2=2타, 3=3타)
    private bool _isAttacking = false;
    private float _lastAttackTime;
    public float _comboResetTime = 2.0f; // 이 시간이 지나면 콤보 초기화

    // 전투 모드 유지를 위한 타이머
    private float _combatModeTimer = 0f;
    private bool _inCombat = false;
    private const float COMBAT_COOLDOWN = 5.0f; // 5초간 공격 없으면 무기 집어넣음(Idle)

    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        CheckComboReset();
        HandleCombatMode();
    }

    public void OnAttackInput()
    {
        // 공격 중 이동 멈춤
        _playerMovement.StopForAttack();

        // 전투 태세 돌입
        _combatModeTimer = COMBAT_COOLDOWN;
        if (!_inCombat)
        {
            _inCombat = true;
            _animator.SetBool("InCombat", true);
        }

        PerformComboAttack();
    }

    private void PerformComboAttack()
    {
        // 쿨타임 체크
        if (Time.time - _lastAttackTime < _weaponData.Atk_Spd) return;

        _lastAttackTime = Time.time;

        // 콤보 단계 증가
        _comboStep++;
        if (_comboStep > 3)
        {
            _comboStep = 1; 
        }

        // 애니메이터 파라미터 전달
        _animator.SetInteger("Combo", _comboStep);
        _animator.SetTrigger("Attack");

        // 데미지 처리 (여기서는 로그만, 실제로는 Raycast나 OverlapSphere 사용)
        CalculateDamage(_comboStep);
    }

    private void CalculateDamage(int comboStep)
    {
        int damage = 0;
        switch (comboStep)
        {
            case 1: damage = _weaponData.Combo_1; break;
            case 2: damage = _weaponData.Combo_2; break;
            case 3: damage = _weaponData.Combo_3; break;
        }

        Debug.Log($"공격 {comboStep}타! 데미지: {damage}");
    }

    private void CheckComboReset()
    {
        // 마지막 공격 후 일정 시간이 지나면 콤보를 0으로 초기화
        if (Time.time - _lastAttackTime > _comboResetTime && _comboStep != 0)
        {
            _comboStep = 0;
            _animator.SetInteger("Combo", 0);
        }
    }

    private void HandleCombatMode()
    {
        if (_inCombat)
        {
            _combatModeTimer -= Time.deltaTime;
            if (_combatModeTimer <= 0)
            {
                _inCombat = false;
                _animator.SetBool("InCombat", false); // Idle로 복귀
                _comboStep = 0;
            }
        }
    }
}
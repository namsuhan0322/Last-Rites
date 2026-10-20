using System;
using UnityEngine;

/// <summary>
/// [역할] 플레이어 체력·스태미나(PlayerSO 값). 강인도 피해는 받지 않는다(스턴은 큰 패턴으로만).
///        Actor의 확장 지점을 덮어써 마법사 보호막 방어, 데미지 숫자 끄기, 사망 트리거 끄기를 맡는다.
/// [이유] Actor가 플레이어를 직접 알지 않도록, 플레이어만 다른 점(보호막 방어·데미지 숫자·사망 트리거)을 여기서 덮어쓴다.
/// [근거] 리팩토링 명세 3.5·결정 R15, 버그 수정 명세 B6
/// </summary>
public class PlayerStats : Actor
{
    [Header("Data")]
    [SerializeField] private PlayerSO _playerData;

    public int BaseAttackPower => _playerData.Base_Atk;
    public float MoveSpeed => _playerData.Move_Spd;
    public int MaxStamina => _playerData.Max_Stamina;
    public float CurrentStamina => _currentStamina;

    private float _currentStamina;

    public event Action<float, float> OnStaminaChanged;

    protected override void Start()
    {
        base.Start();
        InitActor(_playerData.HP);

        _currentStamina = _playerData.Max_Stamina;

        OnStaminaChanged?.Invoke(_currentStamina, MaxStamina);
    }

    protected override void Update()
    {
        base.Update();

        // 스태미나 자동 회복
        if (_currentStamina < MaxStamina)
        {
            _currentStamina += _playerData.Stamina_Regen * Time.deltaTime;

            if (_currentStamina > MaxStamina)
                _currentStamina = MaxStamina;

            // 회복 중일 때도 UI 갱신
            OnStaminaChanged?.Invoke(_currentStamina, MaxStamina);
        }
    }

    public bool UseStamina(int cost)
    {
        if (_currentStamina >= cost)
        {
            _currentStamina -= cost;
            OnStaminaChanged?.Invoke(_currentStamina, MaxStamina);
            return true;
        }
        return false;
    }

    public override void TakePoiseDamage(float amount) { }

    // 마법사 보호막이 켜져 있으면 막는다 (예전 Actor.TakeDamage 안의 GetComponent<PlayerController>() 검사를 옮김)
    protected override bool TryBlockIncomingDamage()
    {
        PlayerController controller = GetComponent<PlayerController>();
        return controller != null && controller.TryBlockDamageWithShield();
    }

    // 플레이어는 머리 위 데미지 숫자를 띄우지 않는다 (피격은 화면 효과로 보여 줌)
    protected override bool ShowsDamageText => false;

    // 플레이어 애니메이터에는 "Die"가 없다. 사망 애니는 PlayerDeadState가 IsDead로 튼다 (Q12, 버그 수정 B6)
    protected override string DeathAnimTrigger => null;
}
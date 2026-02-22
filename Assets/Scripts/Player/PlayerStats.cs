using System;
using UnityEngine;

public class PlayerStats : Actor
{
    [Header("Data")]
    [SerializeField] private PlayerSO _playerData;

    public float MoveSpeed => _playerData.Move_Spd;
    public float DashSpeed => _playerData.Dash_Spd; 
    public int DashCost => _playerData.Dash_Cost;
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
        base.Update(); // Actor.cs의 강인도 계산 등 실행

        // [추가] 스태미나 자동 회복
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
}
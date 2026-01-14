using UnityEngine;

public class PlayerStats : Actor
{
    [Header("Data")]
    [SerializeField] private PlayerSO _playerData;

    public float MoveSpeed => _playerData.Move_Spd;
    public float DashSpeed => _playerData.Dash_Spd;
    public int MaxStamina => _playerData.Max_Stamina;

    private float _currentStamina;

    protected override void Start()
    {
        base.Start();
        InitActor(_playerData.HP);
        _currentStamina = _playerData.Max_Stamina;
    }

    public bool UseStamina(int cost)
    {
        if (_currentStamina >= cost)
        {
            _currentStamina -= cost;
            return true;
        }
        return false;
    }
}
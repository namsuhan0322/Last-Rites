using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("UI 슬라이더")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider staminaSlider;

    [Header("플레이어 스탯")]
    [SerializeField] private PlayerStats playerStats;

    private void Start()
    {
        if (playerStats != null)
        {
            playerStats.OnHPChanged += UpdateHPUI;
            playerStats.OnStaminaChanged += UpdateStaminaUI;

            UpdateHPUI(playerStats.CurrentHP, playerStats.MaxHP);
            UpdateStaminaUI(playerStats.CurrentStamina, playerStats.MaxStamina);
        }
    }

    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnHPChanged -= UpdateHPUI;
            playerStats.OnStaminaChanged -= UpdateStaminaUI;
        }
    }

    private void UpdateHPUI(int current, int max)
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = max;
            hpSlider.value = current;
        }
    }

    private void UpdateStaminaUI(float current, float max)
    {
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = max;
            staminaSlider.value = current;
        }
    }
}
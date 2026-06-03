using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("UI 슬라이더")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider staminaSlider;

    [Header("UI 텍스트 (현재 / 최대)")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI staminaText;

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

        if (hpText != null)
        {
            hpText.text = $"{current} / {max}";
        }
    }

    private void UpdateStaminaUI(float current, float max)
    {
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = max;
            staminaSlider.value = current;
        }

        if (staminaText != null)
        {
            staminaText.text = $"{current.ToString("F0")} / {max.ToString("F0")}";
        }
    }
}
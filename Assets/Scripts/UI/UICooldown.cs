using UnityEngine;
using UnityEngine.UI;

public class UICooldown : MonoBehaviour
{
    [Header("플레이어 레퍼런스")]
    public PlayerController Player;

    [Header("쿨타임 가림막 이미지")]
    [SerializeField] private Image _QSkillImage;
    [SerializeField] private Image _WSkillImage;
    [SerializeField] private Image _ESkillImage;
    [SerializeField] private Image _RSkillImage;
    [SerializeField] private Image _VSkillImage;

    private void Update()
    {
        if (Player == null || Player.CurrentWeapon == null) return;

        UpdateUI();
    }

    private void UpdateUI()
    {
        UpdateSkillUI(_QSkillImage, Player.Q_Timer, Player.CurrentWeapon.Q_Cool);
        UpdateSkillUI(_WSkillImage, Player.W_Timer, Player.CurrentWeapon.W_Cool);
        UpdateSkillUI(_ESkillImage, Player.E_Timer, Player.CurrentWeapon.E_Cool);
        UpdateSkillUI(_RSkillImage, Player.R_Timer, Player.CurrentWeapon.R_Cool);
        UpdateSkillUI(_VSkillImage, Player.V_Timer, Player.CurrentWeapon.V_Cool);
    }

    private void UpdateSkillUI(Image coolImage, float currentTimer, float maxCooldown)
    {
        if (coolImage == null) return;

        if (currentTimer > 0)
        {
            coolImage.gameObject.SetActive(true);

            coolImage.fillAmount = currentTimer / maxCooldown;
        }
        else
        {
            coolImage.gameObject.SetActive(false);
        }
    }
}
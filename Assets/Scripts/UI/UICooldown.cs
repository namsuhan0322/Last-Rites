using TMPro;
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
    [SerializeField] private Image _PotionImage;
    [SerializeField] private Image _RollImage;

    [SerializeField] private GameObject _rollBg;

    [Header("아이템 텍스트")]
    [SerializeField] private TextMeshProUGUI _potionCountText;
    [SerializeField] private TextMeshProUGUI _rollCoolTimeText;

    [Header("쿨타임 색상 설정")]
    [SerializeField] private Color _normalCooldownColor = new Color(0f, 0f, 0f, 0.7f);
    [SerializeField] private Color _emptyPotionColor = new Color(1f, 0f, 0f, 0.5f);

    private void Start()
    {
        if (_rollBg != null) 
            _rollBg.SetActive(false);
    }

    private void Update()
    {
        if (Player == null || Player.CurrentWeapon == null) return;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (Player == null || Player.CurrentWeapon == null) return;

        if (Player.CurrentWeapon != null)
        {
            UpdateSkillUI(_QSkillImage, Player.Q_Timer, Player.CurrentWeapon.Q_Cool);
            UpdateSkillUI(_WSkillImage, Player.W_Timer, Player.CurrentWeapon.W_Cool);
            UpdateSkillUI(_ESkillImage, Player.E_Timer, Player.CurrentWeapon.E_Cool);
            UpdateSkillUI(_RSkillImage, Player.R_Timer, Player.CurrentWeapon.R_Cool);
            UpdateSkillUI(_VSkillImage, Player.V_Timer, Player.CurrentWeapon.V_Cool);
        }

        UpdateSkillUI(_PotionImage, Player.Potion_Timer, Player.potionCooldown);

        if (Player.DashTimer > 0f)
        {
            if (!_rollBg.activeSelf) _rollBg.SetActive(true);
            UpdateSkillUI(_RollImage, Player.DashTimer, Player.dashCooldown);

            if (_rollCoolTimeText != null)
            {
                _rollCoolTimeText.text = $"{Mathf.CeilToInt(Player.DashTimer)}s";
            }
        }
        else
        {
            if (_rollBg != null && _rollBg.activeSelf) _rollBg.SetActive(false);
        }

        if (_potionCountText != null && _PotionImage != null)
        {
            if (Player.currentPotionCount <= 0)
            {
                _potionCountText.text = "0";

                _PotionImage.color = _emptyPotionColor;
                _PotionImage.gameObject.SetActive(true);
                _PotionImage.fillAmount = 1f;
            }
            else
            {
                _potionCountText.text = Player.currentPotionCount.ToString();

                _PotionImage.color = _normalCooldownColor;
                UpdateSkillUI(_PotionImage, Player.Potion_Timer, Player.potionCooldown);
            }
        }
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
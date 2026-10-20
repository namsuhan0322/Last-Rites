using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// [역할] 스킬 5칸·포션·회피의 쿨타임 가림막과 포션 개수·회피 남은 초를 화면에 보여 준다.
/// [이유] 스킬 5칸을 칸 순서(SkillSlot) 반복문으로 처리해 같은 줄 5번 반복을 없앤다.
/// [근거] 명세 3.5, 결정 R31 / Q10(_rollBg 비었을 때 오류)은 버그 수정 1단계에서 고침
/// </summary>
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

    private Image[] _skillImages;   // Q/W/E/R/V 순서 (SkillSlot 값과 같은 순서)

    private void Awake()
    {
        // 인스펙터의 이미지 5개를 칸 순서대로 한 번 묶는다 (필드는 씬에 저장된 참조라 그대로 둔다)
        _skillImages = new[] { _QSkillImage, _WSkillImage, _ESkillImage, _RSkillImage, _VSkillImage };
    }

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

        // 스킬 5칸 (바로 위에서 무기가 없으면 return하므로 무기 검사는 한 번이면 된다)
        for (int i = 0; i < _skillImages.Length; i++)
        {
            SkillSlot slot = (SkillSlot)i;
            UpdateSkillUI(_skillImages[i], Player.GetSkillTimer(slot), Player.CurrentWeapon.GetSkill(slot).Cooldown);
        }

        UpdateSkillUI(_PotionImage, Player.Potion_Timer, Player.potionCooldown);

        if (Player.DashTimer > 0f)
        {
            if (_rollBg != null && !_rollBg.activeSelf) _rollBg.SetActive(true);   // 비어 있으면 건너뜀 (프리팹은 비어 있고 실제 씬만 연결, Q10 고침)
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
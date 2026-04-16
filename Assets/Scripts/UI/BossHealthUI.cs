using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthUI : MonoBehaviour
{
    public GameObject bossUIPanel;

    public Slider hpSlider;

    public TMP_Text currentHpText;
    public TMP_Text maxHpText;

    public Actor bossActor;

    private void Start()
    {
        if (bossUIPanel != null)
        {
            bossUIPanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (bossActor != null)
        {
            bossActor.OnHPChanged += UpdateHealthBar;

            UpdateHealthBar(bossActor.CurrentHP, bossActor.MaxHP);
        }
    }

    private void OnDisable()
    {
        if (bossActor != null)
        {
            bossActor.OnHPChanged -= UpdateHealthBar;
        }
    }

    private void UpdateHealthBar(int currentHp, int maxHp)
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHp;
            hpSlider.value = currentHp;
        }

        if (currentHpText != null)
        {
            currentHpText.text = currentHp.ToString();
        }

        if (maxHpText != null)
        {
            maxHpText.text = maxHp.ToString();
        }
    }

    public void ShowBossUI()
    {
        if (bossUIPanel != null)
        {
            bossUIPanel.SetActive(true);

            UpdateHealthBar(bossActor.CurrentHP, bossActor.MaxHP);
        }
    }

    public void HideBossUI()
    {
        if (bossUIPanel != null)
        {
            bossUIPanel.SetActive(false);
        }
    }
}
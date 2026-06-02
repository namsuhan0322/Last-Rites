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
            bossUIPanel.SetActive(false);

        if (bossActor != null)
            UpdateBossReference(bossActor);
    }

    private void OnDisable()
    {
        if (bossActor != null)
            bossActor.OnHPChanged -= UpdateHealthBar;
    }

    private void UpdateHealthBar(int currentHp, int maxHp)
    {
        int displayHp = Mathf.Max(0, currentHp);

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHp;
            hpSlider.value = displayHp;
        }

        if (currentHpText != null)
            currentHpText.text = displayHp.ToString();

        if (maxHpText != null)
            maxHpText.text = maxHp.ToString();
    }

    public void ShowBossUI()
    {
        if (bossUIPanel != null)
            bossUIPanel.SetActive(true);

        if (bossActor != null)
            UpdateHealthBar(bossActor.CurrentHP, bossActor.MaxHP);
    }

    public void HideBossUI()
    {
        if (bossUIPanel != null)
            bossUIPanel.SetActive(false);
    }

    public void UpdateBossReference(Actor newBoss)
    {
        if (bossActor != null)
            bossActor.OnHPChanged -= UpdateHealthBar;

        bossActor = newBoss;

        if (bossActor != null)
        {
            bossActor.OnHPChanged += UpdateHealthBar;
            UpdateHealthBar(bossActor.CurrentHP, bossActor.MaxHP);
        }
    }

    public void SetBossOnSpawn(Actor spawnedBoss)
    {
        UpdateBossReference(spawnedBoss);
        ShowBossUI();
    }
}
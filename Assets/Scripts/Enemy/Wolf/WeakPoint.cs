using UnityEngine;

public enum WeakPointType
{
    Head,
    LeftWing,
    RightWing
}

/// <summary>
/// [역할] 보스의 부술 수 있는 약점(머리·날개·팔). 맞으면 약점 HP가 깎이고 같은 데미지를 주인 보스에게 넘긴다. 부서지면 보스에게 알린다.
/// [이유] 플레이어 칼 판정이 한 번 휘두를 때 보스에게 한 번만 데미지를 주도록, 주인 읽기와 "넘기지 않고 맞기"를 둔다.
/// [근거] 공격 판정 명세 2.2 ①·3.1, 결정 H2·H7
/// </summary>
public class WeakPoint : Actor
{
    private Actor boss;
    public WeakPointUI ui;

    [SerializeField] private WeakPointType weakPointType;

    private bool isInitialized = false;

    /// 이 약점의 주인 보스 (Init 전이면 null). 플레이어 칼 판정이 보스와 약점을 묶는 데 쓴다 (H2·H7)
    public Actor Owner => boss;

    public void Init(int hp, Actor owner)
    {
        boss = owner;

        if (!isInitialized)
        {
            InitActor(hp);

            if (ui != null)
                ui.Init(hp);

            isInitialized = true;
        }
        else
        {
            if (ui != null)
                ui.SetHP(_currentHP);
        }
    }

    public override void TakeDamage(
     int damage,
     float severityOverride = -1f,
     bool isHeavyAttack = false,
     bool showDamageText = true)
    {
        base.TakeDamage(damage, severityOverride, isHeavyAttack, false);

        if (ui != null)
            ui.SetHP(_currentHP);

        if (boss != null)
            boss.TakeDamage(damage, severityOverride, isHeavyAttack, false);
    }

    /// 약점 HP만 깎고 보스로 넘기지 않는다. 한 번 휘두를 때 보스가 이미 데미지를 받았을 때 쓴다 (H2)
    public void TakeDamageWithoutForward(int damage, float severityOverride = -1f, bool isHeavyAttack = false)
    {
        base.TakeDamage(damage, severityOverride, isHeavyAttack, false);

        if (ui != null)
            ui.SetHP(_currentHP);
    }
    protected override void Die()
    {
        base.Die();

        DragonBoss dragon = boss as DragonBoss;
        if (dragon != null)
        {
            dragon.OnWeakPointBreak(weakPointType);
        }

        WolfBoss wolf = boss as WolfBoss;
        if (wolf != null)
        {
            wolf.OnWeakPointBreak();
        }

        gameObject.SetActive(false);
    }
}
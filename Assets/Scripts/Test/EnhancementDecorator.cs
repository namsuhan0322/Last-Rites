public class EnhancementDecorator : SwordDecorator
{
    private int _currentLevel;
    private int _bonusAttack;

    public EnhancementDecorator(ISword sword, int currentLevel, int bonusAttack = 5) : base(sword)
    {
        _currentLevel = currentLevel;
        _bonusAttack = bonusAttack;
    }

    public override string GetName()
    {
        return $"{base.GetName()} +{_currentLevel}";
    }

    public override int GetAttackPower()
    {
        return base.GetAttackPower() + _bonusAttack;
    }
}
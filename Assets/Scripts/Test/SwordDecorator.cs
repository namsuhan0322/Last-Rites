public abstract class SwordDecorator : ISword
{
    protected ISword _wrappedSword;

    public SwordDecorator(ISword sword)
    {
        _wrappedSword = sword;
    }

    public virtual string GetName() => _wrappedSword.GetName();
    public virtual int GetAttackPower() => _wrappedSword.GetAttackPower();
}
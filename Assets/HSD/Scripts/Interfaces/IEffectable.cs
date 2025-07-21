using Unit;


public interface IEffectable
{
    public void Apply(EffectType type, float amount = 0);
    public void Revoke(EffectType type, float amount = 0);
}

using LDH.LDH_Scripts.Temp;
using Unit;

//todo: IEffectable 수정
//using IEffectable =  LDH.LDH_Scripts.Temp.Temp_IEffectable;
namespace Units
{
    public class SlowEffect : Effect
    {
        public float Amount;

        public SlowEffect(float duration, float amount) : base(EffectType.Slow, duration) 
        {
            Amount = amount;
        }

        public override void Apply(IEffectable target)
        {
            target.Apply(Type, Amount);
        }

        public override void Remove(IEffectable target)
        {
            target.Revoke(Type, Amount);
        }
    }
}
using LDH.LDH_Scripts.Temp;
using Unit;

//todo: IEffectable 수정
using IEffectable =  LDH.LDH_Scripts.Temp.Temp_IEffectable;
namespace Units
{
    public class SlowEffect : Effect
    {
        public SlowEffect(float duration) : base(EffectType.Slow, duration) { }

        public override void Apply(IEffectable target)
        {
            
        }

        public override void Remove(IEffectable target)
        {
        }
    }
}
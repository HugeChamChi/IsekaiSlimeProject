using LDH.LDH_Scripts.Temp;
using Unit;
using UnityEngine;


//todo: IEffectable 수정
//using IEffectable =  LDH.LDH_Scripts.Temp.Temp_IEffectable;
namespace Units
{
    public class FaintEffect : Effect
    {
        public FaintEffect(float duration) : base(EffectType.Faint, duration) {}

        public override void Apply(IEffectable target)
        {
            target.Apply(Type);
        }

        public override void Remove(IEffectable target)
        {
            target.Revoke(Type);
        }

    }
}
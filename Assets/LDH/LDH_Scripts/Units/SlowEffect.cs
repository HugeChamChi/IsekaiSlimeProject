using LDH.LDH_Scripts.Temp;
using Unit;
using UnityEngine;

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
            Debug.Log($"[Slow Effect] : {Amount} 이속 감소");
            target.Apply(Type, Amount);
        }

        public override void Remove(IEffectable target)
        {
            target.Revoke(Type, Amount);
        }
    }
}
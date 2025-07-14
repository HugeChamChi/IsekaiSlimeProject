using LDH.LDH_Scripts.Temp;
using Unit;
using UnityEngine;


//todo: IEffectable 수정
using IEffectable =  LDH.LDH_Scripts.Temp.Temp_IEffectable;
namespace Units
{

    public abstract class Effect : MonoBehaviour
    {
        public EffectType Type;
        public float Duration;

        protected Effect(EffectType type, float duration)
        {
            Type = type;
            Duration = duration;
        }

        public virtual void Apply(IEffectable target)
        {
            
        }

        public virtual void Remove(IEffectable target)
        {
        }

    }
}
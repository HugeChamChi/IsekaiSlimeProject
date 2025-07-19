using System.Collections;
using Unit;
using System.Collections.Generic;
using UnityEngine;

//todo: IDamagable 수정
using IDamagable = LDH.LDH_Scripts.Temp.Temp_IDamagable;
//todo: IEffectable 수정
using IEffectable =  LDH.LDH_Scripts.Temp.Temp_IEffectable;

namespace Units
{
    public class UnitSkill
    {
        public string Description;
        public int Damage;
        public float Duration;
        public float CoolDown;
        
        public Effect Effect;

        public UnitSkill(string description, int damage, float duration, float coolDown, EffectType effectType, float effectDuration)
        {
      
            Description = description;
            Damage = damage;
            Duration = duration;
            CoolDown = coolDown;
            Effect = effectType switch
            {
                EffectType.Faint => new FaintEffect(effectDuration),
                EffectType.Slow => new SlowEffect(effectDuration)
                
            };
        }
        
        public void Execute(Unit caster, List<IDamagable> targets) {
            foreach (var target in targets) {
                // target.TakeDamage(caster.Stat.Attack * 1.5f);  // 예: 스킬은 1.5배 데미지

                if (target is IEffectable effectTarget)
                {
                    Effect?.Apply(effectTarget);
                    caster.StartCoroutine(RemoveEffect(effectTarget, Effect));
                }
            
            }
        }
        
        private IEnumerator RemoveEffect(IEffectable target, Effect effect) {
            yield return new WaitForSeconds(effect.Duration);
            effect.Remove(target);
        }
    }
}
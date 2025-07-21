using System.Collections;
using Unit;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Util;


namespace Units
{
    public class UnitSkill
    {
        public string Description;
        public int Damage;
        public float Duration;
        public float CoolTime;
        
        public Effect Effect;

        private Collider2D[] buffer = new Collider2D[100];

        private WaitForSeconds skillIntervalWait; // 스킬 지속 시간 내 스킬 사용 상태에서 스킬 적용간의 간격.. 잠깐 딜레이주는 시간
        private float skillIntervalTime = 0.5f;

        private int skillCount;
        public UnitSkill(string description, int damage, float duration, float coolTime, EffectType effectType, float effectDuration, float amount = 0)
        {
      
            Description = description;
            Damage = damage;
            Duration = duration;
            CoolTime = coolTime;
            Effect = effectType switch
            {
                EffectType.Faint => new FaintEffect(effectDuration),
                EffectType.Slow => new SlowEffect(effectDuration, amount),
                EffectType.None => null
                
            };

            skillIntervalWait = new WaitForSeconds(skillIntervalTime);
        }
        
        
        
        
        public IEnumerator Execute(Unit caster)
        {
            Debug.Log("[SkillState] 스킬 횟수 카운트 변수 초기화");
            skillCount = 0;
            UnitHolder holder = caster.GetComponentInParent<UnitHolder>();
            holder.ShowSkillApplyRange();
            
            float remainDuration = Duration;
            do
            {
                skillCount++;
                Debug.Log($"[skill state] 스킬 ({caster.Type.ToString()}) 실행 횟수 : {skillCount}회");
               
                //애니메이션
                caster.Controller.anim.SetTrigger("2_Attack");
                
                
                UnitHolder currentHolder = caster.GetComponentInParent<UnitHolder>();
                
               if (holder != currentHolder)
               {
                   holder.HideSkillApplyRange();
                   holder = currentHolder;
                   holder.ShowSkillApplyRange();
               }
                
               
                foreach (var target in GetTargetListInSkillRange(currentHolder, caster))
                {

                    //데미지
                    float damage = caster.Controller.CalcDamage(true);
                    target.TakeDamage(damage);
                    
                    //디버프
                    if (target is IEffectable effectTarget)
                    {
                        Effect?.Apply(effectTarget);
                        caster.StartCoroutine(RemoveEffect(effectTarget, Effect));
                    }
                    
                }


                yield return skillIntervalWait;
                remainDuration -= skillIntervalTime;
                
                
            } while (remainDuration > 0f);

            
            yield return null;
            holder.HideSkillApplyRange();
        }

        

        private List<IDamageable> GetTargetListInSkillRange(UnitHolder currentHolder, Unit caster)
        {
            List<IDamageable> targets = new();
            
            foreach (var slot in currentHolder.SkillApplySlots) //todo: 고쳐야함
            {
                int cnt = Utils.GetTargets(slot.SpawnPosition, 1, OverlapType.Box, caster.Controller.TargetLayer, 0f,
                    slot.transform.lossyScale, buffer);

               for(int i=0; i<cnt; i++)
                {
                    if (buffer[i].TryGetComponent(out IDamageable damageable))
                    {
                        targets.Add(damageable);
                    }
                }
            }

            return targets;
        }
        
        private IEnumerator RemoveEffect(IEffectable target, Effect effect) {
            yield return new WaitForSeconds(effect.Duration);
            effect.Remove(target);
        }


        #region Legacy

        // private IEnumerator RangeAttackCoroutine(Unit caster)
        // {
        //     float remainDuration = Duration;
        //     do
        //     {
        //         skillCount++;
        //         Debug.Log($"[skill state] 스킬 ({caster.Type.ToString()}) 실행 횟수 : {skillCount}회");
        //         UnitHolder currentHolder = caster.GetComponentInParent<UnitHolder>();
        //
        //         currentHolder.ShowSkillApplyRange();
        //
        //         foreach (var target in GetTargetListInSkillRange(currentHolder, caster))
        //         {
        //
        //             float damage = caster.Controller.CalcDamage(true);
        //             target.TakeDamage(damage); // 예: 1.5배 데미지
        //         }
        //
        //
        //         yield return skillIntervalWait;
        //         remainDuration -= 0.5f;
        //         
        //
        //         currentHolder.HideSkillApplyRange();
        //         
        //     } while (remainDuration > 0f);
        // }


        #endregion
    }
    
    
}

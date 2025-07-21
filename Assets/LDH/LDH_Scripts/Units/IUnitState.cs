using System.Collections;
using Unit;
using UnityEngine;
using Util;

namespace Units
{
    public interface IUnitState
    {
        void Enter();
        
        void Exit();
        IEnumerator Run();
    }
    public class UnitIdleState : IUnitState
    {
        private Unit unit;
        private UnitController unitController;
        
        public UnitIdleState(Unit unit, UnitController unitController)
        {
            this.unit = unit;
            this.unitController = unitController;
        }
        
        public void Enter()
        {
            Debug.Log("[Idle State] 진입");
            //애니메이션
            unitController.anim.SetTrigger("NoTarget");
            unitController.SetOutline(OutlineType.None);
            
        }
        

        public IEnumerator Run()
        {
            
            yield return new WaitUntil(() => unitController.canAttack == true);
            
            Exit();
            
        }

        public void Exit()
        {
            Debug.Log("[Idle State] 종료");
            //unitController.CheckStateChange();
        }
    }
    public class UnitAttackState : IUnitState
    {
        private Unit unit;
        private UnitController unitController;
        
        public UnitAttackState(Unit unit, UnitController unitController)
        {
            this.unit = unit;
            this.unitController = unitController;
        }
        
        public void Enter()
        {
            Debug.Log("[Attack State] 진입");
            unitController.SetOutline(OutlineType.None);
            unitController.canAttack = false;
        }
        
        
        public IEnumerator Run()
        {
            
            //이걸 1번만 실행할껀데 update가 아니라 enter에 써야하는건가?
            Debug.Log("[Attack State] 일반 공격을 처리한다.");
            
            var target = unitController.GetLowestHpTarget();;

            if (unitController.IsTargetAlive(target))
            {
                //타겟 있음
                Debug.Log("[Attack State] 타겟 발견");

                // 스프라이트 방향 전환
                unitController.UpdateSpriteFlip(Utils.DirToTarget(target.position, unit.transform.position));

                Debug.Log("[Attack State] 공격 시작");
                unitController.anim.SetTrigger("Attack");
                unitController.Attack(target.GetComponent<IDamageable>());

                yield return unitController.attackAnimWait;
            }
            
            yield return null;
            
            Exit();
        }

        //
        //
        // private void Attack()
        // {
        //     //이걸 1번만 실행할껀데 update가 아니라 enter에 써야하는건가?
        //     Debug.Log("[Attack State] 일반 공격을 처리한다.");
        //     
        //     var target = unitController.GetLowestHpTarget();;
        //
        //     if (unitController.IsTargetAlive(target))
        //     {
        //         //타겟 있음
        //         Debug.Log("[Attack State] 타겟 발견");
        //
        //         // 스프라이트 방향 전환
        //         unitController.UpdateSpriteFlip(Utils.DirToTarget(target.position, unit.transform.position));
        //
        //         Debug.Log("[Attack State] 공격 시작");
        //         unitController.anim.SetTrigger("Attack");
        //         unitController.Attack(target.GetComponent<IDamageable>());
        //     }
        //
        //     unitController.attackCoolTime = unitController.Stat.AttackDelay;
        // }

        public void Exit()
        {
            Debug.Log("[Attack State] 종료");
            unitController.attackCoolTime = unitController.Stat.AttackDelay;
            //unitController.CheckStateChange();
        }
    }
    public class UnitSkillState : IUnitState
    {
        private Unit unit;
        private UnitController unitController;

        
        public UnitSkillState(Unit unit, UnitController unitController)
        {
            this.unit = unit;
            this.unitController = unitController;
        }
        
        public void Enter()
        {
            Debug.Log("[Skill State] 진입");
            Debug.Log("[Skill State] 스킬 실행");
            
            unitController.SetOutline(OutlineType.Skill);
            //스킬 지속시간 동안 스킬을 실행한다.
           ;
        }


        public IEnumerator Run()
        {
            yield return unitController.StartCoroutine(unitController.Skill.Execute(unit));
            Exit();
            
        }

        public void Exit()
        {
            Debug.Log("[Skill State] 종료");
            unitController.skillCoolTime = unitController.Skill.CoolTime;
            //unitController.CheckStateChange();
        }
        
        
    } 
}
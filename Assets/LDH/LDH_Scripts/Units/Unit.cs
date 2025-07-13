using System;
using System.Collections;
using System.Collections.Generic;
using LDH.LDH_Scripts.Temp;
using Unit;
using UnityEngine;

//todo: IDamagable 수정
using IDamagable = LDH.LDH_Scripts.Temp.Temp_IDamagable;

namespace Units
{
    public class Unit : MonoBehaviour
    {
        [field:SerializeField] public int Index { get; private set;}
        [field:SerializeField] public string Name  { get; private set;}
        [field:SerializeField] public UnitTier Tier  { get; private set;}
        [field:SerializeField] public UnitType Type  { get; private set;}
        [field:SerializeField] public string Description  { get; private set;}
        [field:SerializeField] public string ModelFileName  { get; private set;}

         public UnitStat Stat = new();
            // = new UnitStat(800, 1, 210, 0);
        public UnitSkill Skill;
        
        private Animator _anim;
        private Coroutine _attackCoroutine;
        private WaitForSeconds _attackWait;
        
        [Header("Target Setting")]
        [SerializeField] private LayerMask targetLayer;

        private void Awake()
        {
            // Stat = new UnitStat();
            // Skill = new();
            Init();
        }

        protected virtual void Init()
        {
            _anim = GetComponentInChildren<Animator>();
            
            _attackWait = new WaitForSeconds(Stat.AttackDelay);
        }

        private void Start()
        {
            _attackCoroutine = StartCoroutine(AttackCoroutine());
        }

        private void OnDestroy()
        {
            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }
        }


        private void Update()
        {
           
        }


        #region Attack / Skill

        private IDamagable FindNearestTarget()
        {
            Collider2D[] monsterInRange = Physics2D.OverlapCircleAll(transform.position, Stat.AttackRange, targetLayer);
            
            //가장 가까운 몬스터를 타겟으로 설정
            //가장 가까운 몬스터 찾기
            IDamagable target = null;
            float minDist = float.MaxValue;
            foreach (Collider2D monster in monsterInRange)
            {
                float distance = Vector2.Distance(transform.position, monster.transform.position);

                if (minDist > distance)
                {
                    minDist = distance;
                    target = monster.GetComponent<IDamagable>();
                }
            }

            return target;
        }

        private IEnumerator AttackCoroutine()
        {
            while (true)
            {
                //콜라이더 추척
                IDamagable target = FindNearestTarget();
            
                //쿨타임 내에서 일반 공격
                if (target != null)
                {
                    Attack(target);
                }

                yield return _attackWait;
            }

        }

        #endregion
        
        
        

        public void Attack(IDamagable target)
        {
            //todo: 테스트 코드
            TempMonster monster = target as TempMonster;
            Debug.Log($"{monster.name} 을 공격합니다.");
        }

        public void UseSkill(int index, List<IDamagable> targets)
        {
            // Skill.Execute(this, targets);
        }


        #region Gizmo

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, Stat.AttackRange);
        }

        #endregion
        
        
    }
}
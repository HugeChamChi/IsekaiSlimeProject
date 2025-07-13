using System;
using System.Collections;
using System.Collections.Generic;
using LDH.LDH_Scripts.Temp;
using Unit;
using UnityEngine;
using Util;

// todo: IDamagable 수정
using IDamagable = LDH.LDH_Scripts.Temp.Temp_IDamagable;

namespace Units
{
    /// <summary>
    /// 게임 내 유닛(영웅) 기본 클래스.
    /// 스탯, 일반 공격, 스킬, 애니메이션, 타겟 탐색 및 공격 루프 관리.
    /// </summary>
    public class Unit : MonoBehaviour
    {
        [field: SerializeField] public int Index { get; private set; }
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public UnitTier Tier { get; private set; }
        [field: SerializeField] public UnitType Type { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public string ModelFileName { get; private set; }

        /// <summary>
        /// 유닛 능력치 정보 (공격력, 사거리, 속도 등)
        /// </summary>
        [SerializeField] public UnitStat Stat = new();

        /// <summary>
        /// 유닛 보유 스킬
        /// </summary>
        public UnitSkill Skill;

        private Animator _anim;
        private Coroutine _attackCoroutine;
        private WaitForSeconds _attackWait;

        [Header("Target Setting")]
        [SerializeField] private LayerMask targetLayer;

        private void Awake()
        {
            Init();
        }

        /// <summary>
        /// 유닛 초기화 (애니메이터, 공격 대기 시간 등)
        /// </summary>
        protected virtual void Init()
        {
            _anim = GetComponentInChildren<Animator>();
            _attackWait = new WaitForSeconds(Stat.AttackDelay);
        }

        private void Start()
        {
            // 일반 공격 루프 코루틴 시작
            _attackCoroutine = StartCoroutine(AttackCoroutine());
        }

        private void OnDestroy()
        {
            // 유닛 파괴 시 코루틴 정리
            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }
        }

        #region Attack / Skill

        // /// <summary>
        // /// 공격 범위 내 가장 가까운 적 탐색
        // /// </summary>
        // /// <returns>가장 가까운 IDamagable 대상 (없으면 null)</returns>
        // private IDamagable FindNearestTarget()
        // {
        //     Collider2D[] monstersInRange = Physics2D.OverlapCircleAll(transform.position, Stat.AttackRange, targetLayer);
        //
        //     IDamagable nearestTarget = null;
        //     float minDistance = float.MaxValue;
        //
        //     foreach (Collider2D monster in monstersInRange)
        //     {
        //         float distance = Vector2.Distance(transform.position, monster.transform.position);
        //
        //         if (distance < minDistance)
        //         {
        //             minDistance = distance;
        //             nearestTarget = monster.GetComponent<IDamagable>();
        //         }
        //     }
        //
        //     return nearestTarget;
        // }

        /// <summary>
        /// 일반 공격 반복 실행 루프.
        /// Stat.AttackDelay 간격으로 FindNearestTarget → Attack 반복.
        /// </summary>
        private IEnumerator AttackCoroutine()
        {
            while (true)
            {
                //IDamagable target = FindNearestTarget();
                Transform targetTransform = Utils.FindClosestTarget(transform.position, Stat.AttackRange, OverlapType.Circle, targetLayer);
                if (targetTransform != null && targetTransform.TryGetComponent<IDamagable>(out IDamagable target))
                {
                    Attack(target);
                }

                yield return _attackWait;
            }
        }

        /// <summary>
        /// 대상에게 일반 공격 실행 (현재는 로그 출력용)
        /// </summary>
        /// <param name="target">공격할 대상</param>
        public void Attack(IDamagable target)
        {
            TempMonster monster = target as TempMonster;
            Debug.Log($"{monster.name} 을 공격합니다.");
        }

        /// <summary>
        /// 유닛 스킬 사용
        /// </summary>
        /// <param name="targets">대상 목록</param>
        public void UseSkill(List<IDamagable> targets)
        {
            // Skill.Execute(this, targets);
        }

        #endregion

        #region Gizmo

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, Stat.AttackRange);
        }

        #endregion
    }
}

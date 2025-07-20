using UnityEngine;

namespace Units
{
    /// <summary>
    /// 유닛의 주요 능력치(공격력, 사거리, 공격 속도, 버프 범위)를 관리하는 클래스.
    /// 생성 시 초기화되며, 각 능력치는 Setter 메서드를 통해 수정할 수 있다.
    /// </summary>
    [System.Serializable]
    public class UnitStat
    {
        /// <summary>
        /// 공격력
        /// </summary>
        [field:SerializeField] public float Attack { get; private set; }

        /// <summary>
        /// 초당 타격 수 (공격 속도)
        /// </summary>
        [field:SerializeField] public float AttackSpeed { get; private set; }
        
        
        /// <summary>
        /// 공격 범위 (유닛 중심 기준 반지름)
        /// </summary>
        [field:SerializeField] public float AttackRange { get; private set; }
 
  
        //초당 공격횟수 = AttackSpeed
        //딜레이(쿨타임..?) 일반 공격 쿨타임..? 1초당 attack speed 만큼 => 1번 공격 후 1/attackspeed 만큼 딜레이
        public float AttackDelay => 1 / AttackSpeed;
        
        
        /// <summary>
        /// 생성자: UnitStat 객체를 초기화한다.
        /// </summary>
        /// <param name="attack">공격력</param>
        /// <param name="attackRange">공격 범위</param>
        /// <param name="attackSpeed">공격 속도</param>
        /// <param name="buffRange">버프 범위</param>
        public UnitStat(float attack,  float attackSpeed, float attackRange)
        {
            Attack = attack;
            AttackRange = attackRange / 100f;
            AttackSpeed = attackSpeed;
        }

        public UnitStat() { }

        /// <summary>
        /// 공격력 값을 갱신한다.
        /// </summary>
        public void SetAttack(float value)
        {
            Attack = value;
        }

        /// <summary>
        /// 공격 범위 값을 갱신한다.
        /// </summary>
        public void SetAttackRange(float value)
        {
            AttackRange = value;
        }

        /// <summary>
        /// 공격 속도 값을 갱신한다.
        /// </summary>
        public void SetAttackSpeed(float value)
        {
            AttackSpeed = value;
        }

    }
}
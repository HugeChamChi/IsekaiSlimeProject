using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;
using Managers;
using Photon.Pun;
using PlayerField;
using System;
using System.Linq;

namespace Units
{
    
    /// <summary>
    /// 유닛 전투, 애니메이션, 타겟 탐색, 공격 루프를 담당하는 컨트롤러.
    /// </summary>
    public class UnitController : MonoBehaviour
    {
        [Header("Stats")] 
        [SerializeField] public UnitStat Stat; // 유닛 능력치 정보 (공격력, 사거리, 속도 등)

        [Header("Skill")]
        public UnitSkill Skill; // 유닛 보유 스킬
        
        [Header("Sprite & Animator")]
        private SpriteRenderer _sr;
        private Animator _anim;
        
        [Header("Target Setting")]
        [SerializeField] private LayerMask targetLayer;         // 공격 대상 레이어
        [SerializeField] private bool isLeftFacingSprite;       // 스프라이트 기본 방향 (왼쪽)

        // --- 코루틴 --- //
        private Coroutine _attackCoroutine;          // 공격 루프 코루틴 핸들
        // private WaitForSeconds _attackWait;          // 공격 대기 시간 (공격 속도 기반)
        private bool _isAttacking = false;

        #region Unity LifeCycle

        private void Awake() => Init();

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


        #endregion
     
        
        #region Controller Initialization
        
        /// <summary>
        /// Animator, SpriteRenderer, AttackDelay 초기화.
        /// </summary>
        private void Init()
        {
            _anim = GetComponentInChildren<Animator>();
            _sr = GetComponentInChildren<SpriteRenderer>();
            // _attackWait = new WaitForSeconds(Stat.AttackDelay);
            
            
            //카메라로 각 클라이언트 필드 렌더링하는 방식으로 변경함에 따라 주석 처리
            // InitPhotonData();
            //SetPositionScale();
        }

        /// <summary>
        /// UnitInfo 기반 스탯 및 애니메이터 세팅.
        /// </summary>
        public void InitData(UnitDataManager.UnitInfo info)
        {
            Stat = new UnitStat(info.Attack, info.AttackSpeed, info.AttackRange, info.BuffRange);
            
            //애니메이션 변경
            // Resources 폴더에서 AnimatorController 로드
            var animatorController = Manager.Resources.Load<RuntimeAnimatorController>($"Animators/Unit {info.Index}");
            if (animatorController!=null)
            {
                _anim.runtimeAnimatorController = animatorController;
            }
            else
            {
                Debug.LogError($"AnimatorController 'Unit {info.Index}'를 Resources/Animators에서 찾을 수 없습니다.");
            }
            Debug.Log($"[UnitController] 초기화 완료! 공격력: {Stat.Attack}");

        }
        
        #endregion
        
        

        #region Attack / Skill

        /// <summary>
        /// 일반 공격 반복 실행 루프.
        /// Stat.AttackDelay 간격으로 FindClosestTarget → Attack 반복.
        /// </summary>
        private IEnumerator AttackCoroutine()
        {
            while (true)
            {
                // 범위 내에서 가장 가까운 대상 찾기
                var targetTransform = GetClosestTarget();

                if (IsTargetAlive(targetTransform))
                {
                    //타겟 있음
                    Debug.Log("타겟 발견");
                 
                    // 스프라이트 방향 전환
                    UpdateSpriteFlip(Utils.DirToTarget(targetTransform.position, transform.position));

                    
                    //현재 공격 중인 상태가 아니었다면
                    if (!_isAttacking)
                    {
                        Debug.Log("공격 시작");
                        _isAttacking = true;
                        
                        //공격 애니메이션 재생
                        _anim.SetTrigger("Attack");
                        Attack(targetTransform.GetComponent<IDamageable>());
                        
                        yield return WaitForAttackCooldown(targetTransform);
                        
                        _isAttacking = false;
                    }
                }
                else
                {
                    //타겟 없음 -> Idle로 전환
                    _anim.SetTrigger("NoTarget");
                    _isAttacking = false;
                }
                yield return null;
            }
        }

        private Transform GetClosestTarget()
        {
            return Utils.FindLowestHpMonster(transform.position, Stat.AttackRange, OverlapType.Circle, targetLayer);
        }
        
        private bool IsTargetAlive(Transform target)
        {
            return target != null && target.TryGetComponent<IDamageable>(out IDamageable damagable);
        }
        
        private IEnumerator WaitForAttackCooldown(Transform target)
        {
            Debug.Log("대기 상태 돌입");
            
            float elapsed = 0f;
            while (elapsed < Stat.AttackDelay)
            {
                if (!IsTargetAlive(target))
                {
                    Debug.Log("타겟 사라짐, 공격 중단");
                    _anim.SetTrigger("NoTarget");
                    _isAttacking = false;
                    yield break;
                }
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        /// <summary>
        /// 대상에게 일반 공격 실행 (현재는 로그 출력용)
        /// </summary>
        /// <param name="target">공격할 대상</param>
        public void Attack(IDamageable target)
        {

            float damage = CalcDamage();
            var monster = target as MonsterStatusController;
            
            Debug.Log($"[유닛] : {monster.name} 을 공격합니다. 유닛이 넘겨주는 데미지 : {damage}");
            target.TakeDamage(damage);
            
        }

        private float CalcDamage()
        {
           // 데미지 계산식 : (캐릭터 최종 공격력) X (1 + 캐릭터 공격력 증가율 %) X (1 - 적 최종 데미지 감소율) X (스킬이라면 스킬 계수)
           // 유닛이 계산할 것 :( 캐릭터 최종 공격력) X (1 + 캐릭터 공격력 증가율 %)  X (스킬이라면 스킬 계수)
           
           //캐릭터 최종 공격력 = 캐릭터 공격력 X (1 + 캐릭터 돌파 레벨 X 0.05) => 돌파 레벨업 마다 공격력 5% 증가  //todo: 캐릭터 돌파 레벨
           //캐릭터 공격력 = UnitStat.Attack
           //캐릭터 공격력 증가율 = CardManager.Instance.AttakPower
           //스킬 계수 : skill.Damage

           int level = 0; //todo : 캐릭터 돌파 레벌
           float finalAttack = Stat.Attack * (1 + level * 0.05f);
           float damage = finalAttack * (1 + CardManager.Instance.AttackPower.Value) * Skill.Damage;


           return damage;
        }
        

        /// <summary>
        /// 유닛 스킬 사용
        /// </summary>
        /// <param name="targets">대상 목록</param>
        public void UseSkill(List<IDamageable> targets)
        {
            // Skill.Execute(this, targets);
        }
        
        #endregion
        
        
        #region Sprite Flip

        /// <summary>
        /// 방향 벡터를 기준으로 스프라이트 좌우 뒤집기 처리.
        /// </summary>
        /// <param name="dir">타겟 방향 벡터</param>
        public void UpdateSpriteFlip(Vector2 dir)
        {
            if (dir.x > 0)
                _sr.flipX = isLeftFacingSprite;      // 오른쪽 보기 -> 기본 방향이 왼쪽이면 flip하기
            else if (dir.x < 0)
                _sr.flipX = !isLeftFacingSprite;     // 왼쪽 보기 -> 기본 방향이 왼쪽이면 flip 취소
            // dir.x == 0 → 기존 flipX 유지
        }

        #endregion
        
        
        #region Gizmos

        /// <summary>
        /// 에디터에서 공격 범위 시각화.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            // 공격 범위 시각화
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, Stat.AttackRange);
        }

        #endregion
    }
}
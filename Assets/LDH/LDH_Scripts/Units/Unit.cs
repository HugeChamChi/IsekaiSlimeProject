using System;
using System.Collections;
using System.Collections.Generic;
using LDH.LDH_Scripts.Temp;
using Managers;
using Photon.Pun;
using PlayerField;
using Unit;
using UnityEngine;
using Util;

// todo: IDamagable 수정(현재는 임시 클래스 사용 중)
using IDamagable = LDH.LDH_Scripts.Temp.Temp_IDamagable;

namespace Units
{
    /// <summary>
    /// 게임 내 유닛(영웅) 기본 클래스.
    /// 스탯, 일반 공격, 스킬, 애니메이션, 타겟 탐색 및 공격 루프 관리.
    /// </summary>
    public class Unit : MonoBehaviour
    {
        #region Variables

        [field: Header("Unit Info")]
        [field: SerializeField] public int Index { get; private set; }
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public UnitTier Tier { get; private set; }
        [field: SerializeField] public UnitType Type { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public string ModelFileName { get; private set; }

        [Header("Stats")]
        [SerializeField] public UnitStat Stat = new(); // 유닛 능력치 정보 (공격력, 사거리, 속도 등)

        [Header("Skill")]
        public UnitSkill Skill; // 유닛 보유 스킬

        [Header("Sprite & Animator")]
        private SpriteRenderer _sr;
        private Animator _anim;
        
        [Header("Target Setting")]
        [SerializeField] private LayerMask targetLayer;         // 공격 대상 레이어
        [SerializeField] private bool isLeftFacingSprite;       // 스프라이트 기본 방향 (왼쪽)

        
        // --- 코루틴 --- //
        private Coroutine _attackCoroutine;
        private WaitForSeconds _attackWait;
        
        // -- photon data -- //
        private PhotonView _photonView;
        private int _ownerActorNumber;
        private int _slotIndex;
        
        #endregion

        
        
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
        
  

        #region Initialization
        
        /// <summary>
        /// 유닛 초기화 (애니메이터, 스프라이트, 공격 대기 시간 등)
        /// </summary>
        protected virtual void Init()
        {
            _anim = GetComponentInChildren<Animator>();
            _sr = GetComponentInChildren<SpriteRenderer>();
            _attackWait = new WaitForSeconds(Stat.AttackDelay);
            _photonView = GetComponent<PhotonView>();
            
            InitPhotonData();
            SetPositionScale();

            
        }
        
        private void InitPhotonData()
        {
            object[] data = _photonView.InstantiationData;
            _ownerActorNumber = (int)data[0];
            _slotIndex = (int)data[1];
        }
        
        private void SetPositionScale()
        {
            PlayerFieldController field = PlayerFieldManager.Instance.GetFieldController(_ownerActorNumber);
            if (field == null)
            {
                Debug.LogError($"{_ownerActorNumber}의 필드 위치를 찾을 수 없습니다.");
                return;
            }

            //position 설정
            transform.position = field.SpawnList[_slotIndex];
            
            //scale 설정
            transform.localScale = Vector3.one * field.transform.localScale.x;
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
                Transform targetTransform = Utils.FindClosestTarget(
                    transform.position,
                    Stat.AttackRange,
                    OverlapType.Circle,
                    targetLayer
                );

                if (targetTransform != null && targetTransform.TryGetComponent<IDamagable>(out IDamagable target))
                {
                    // 스프라이트 방향 전환
                    Vector2 dir = Utils.DirToTarget(targetTransform.position, transform.position);
                    UpdateSpriteFlip(dir);

                    // 공격 실행
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

        #region Sprite Flip

        /// <summary>
        /// 방향 벡터를 기준으로 스프라이트 좌우 뒤집기 처리.
        /// </summary>
        /// <param name="dir">타겟 방향 벡터</param>
        private void UpdateSpriteFlip(Vector2 dir)
        {
            if (dir.x > 0)
                _sr.flipX = isLeftFacingSprite;      // 오른쪽 → 기본 방향
            else if (dir.x < 0)
                _sr.flipX = !isLeftFacingSprite;     // 왼쪽 → 반대 방향
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

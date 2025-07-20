using Managers;
using Photon.Pun;
using PlayerField;
using System;
using System.Collections.Generic;
using System.Linq;
using Unit;
using UnityEngine;

namespace Units
{
    public class UnitHolder : MonoBehaviour
    {
        private string _unitPrefabPath = "Prefabs/LDH_TestResource/Unit";  // 소환할 유닛 경로
        
        private int holderIndex; //유닛 인덱스
        public int HolderIndex => holderIndex;
        public bool HasUnit => currentUnit != null;

        private Unit currentUnit = null;
        private GridSlot currentSlot = null;
        public Unit CurrentUnit => currentUnit;
        public GridSlot CurrentSlot => currentSlot;
        

        private List<GridSlot> skillRangeSlots = new();
        private List<GridSlot> skillApplySlots = new();
        public List<GridSlot> SkillApplySlots => skillApplySlots;


        private void OnDestroy()
        {
            StopAllCoroutines();
        }


        public void ChangeUnit(UnitHolder targetHolder)
        {
            currentUnit?.ChangePosition(targetHolder);
        }
        
        public void SetCurrentUnit(Unit unit)
        {
            currentUnit = unit;
            GetSkillRange();
        }


        public void ClearCurrentUnit()
        {
            currentUnit = null;
        }
        
        public void SetCurrentSlot(GridSlot slot)
        {
            currentSlot = slot;
            GetSkillRange();
        }

        public void SetHolderIndex(int index)
        {
            holderIndex = index;
        }


        [Header("Skill Range Test")] public SkillRangeType testSkillRangeType;
        
        private void Start() => Init();

        private void Init()
        {
            //box colider 생성
            AddBoxCollider();
        }

        private void AddBoxCollider()
        {
            var collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = PlayerFieldManager.Instance.GetLocalFieldController().SlotScale;
        }

        public void SpawnUnit(int unitIndex)
        {
            holderIndex = unitIndex;
            
            //todo: 테스트중 → 추후 Manager.Resources.NetworkInstantiate 사용 가능한지 확인 필요 , 로직 변동 될 수 있음
            var unit= PhotonNetwork.Instantiate(_unitPrefabPath, transform.position, Quaternion.identity, 0, new object[] { unitIndex});

            int holderID = ComponentProvider.Get<InGameObject>(gameObject).uniqueID;
            //unit.transform.SetParent(transform);  // 홀더 안으로 배치
            //rpc로 수정
            ComponentProvider.Get<PhotonView>(unit).RPC("SetParentRPC", RpcTarget.All, holderID);
            
            SetCurrentUnit(unit.GetComponent<Unit>());
            
            
            //유닛 
        }
        
        //스킬 범위
        public void ShowSkillRange()
        {
            //todo: 공격 범위, 스킬 범위 보여주기(shader), 시간 등
            //Debug.Log(skillRangeSlots.Count);
            if(currentUnit==null || skillRangeSlots.Count == 0) return;
            
            foreach (GridSlot gridSlot in skillRangeSlots)
            {
                gridSlot.SetColor(true);
            }
        }

        public void ShowSkillApplyRange()
        {
            if (currentUnit == null || skillApplySlots.Count == 0) return;
            
            foreach (GridSlot gridSlot in skillApplySlots)
            {
                gridSlot.SetColor(true);
            }
        }

        public void HideSkillRange()
        {
            foreach (GridSlot gridSlot in skillRangeSlots)
            {
                gridSlot.SetColor(false);
            }            
        }

        private void GetSkillRange()
        {
            //Debug.Log("get skill range 호출");
            if (currentUnit == null || currentSlot == null)
            {
                //Debug.Log("current unit null or currentslot null");
                skillRangeSlots.Clear();
                skillApplySlots.Clear();
                
                HideSkillRange();
                return;
            }
            
            skillRangeSlots.Clear();
            skillApplySlots.Clear();
            
            // var offsetList = SkillRangePattern.Offsets[currentUnit.SkillRangeType];
            
            var offsetList = SkillRangePattern.Offsets[currentUnit.SkillRangeType];
           
            var gridSlots = PlayerFieldManager.Instance.GetLocalFieldController().MapSlot;
            
            foreach (var offset in offsetList)
            {
                int targetRow = currentSlot.Row + offset.x;
                int targetCol = currentSlot.Column + offset.y;
                foreach (GridSlot gridSlot in gridSlots)
                {
                    if (gridSlot.Row == targetRow && gridSlot.Column == targetCol)
                    {
                        skillRangeSlots.Add(gridSlot);
                        if(gridSlot.SlotType == SlotType.Outer)
                            skillApplySlots.Add(gridSlot);
                        
                    }
                }
                
            }

        }

        public void DeleteUnit()
        {
            Debug.Log($"유닛 {currentUnit.Index} {currentUnit.Name} 삭제!");
            PhotonNetwork.Destroy(currentUnit.gameObject);
            
            //선택한 홀더 초기화
            InGameManager.Instance.ClearSelectedHolder();
            
            //현재 홀더의 current unit 초기화
            ClearCurrentUnit();
            
            //skill range 숨기기
            HideSkillRange();
            
            //유닛 변경을 알림
            PlayerFieldManager.Instance.GetLocalFieldController().NotifyUnitChanged();
        }
        
        
    }
}
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
    public class UnitHolder : NetworkUnit
    {
        private string _unitPrefabPath = "Prefabs/LDH_TestResource/Unit";  // 소환할 유닛 경로
        
        public int Holder_Index;
        public bool HasUnit;

        private Unit currentUnit = null;
        private GridSlot currentSlot = null;
        public Unit CurrentUnit => currentUnit;
        public GridSlot CurrentSlot => currentSlot;
        

        private List<GridSlot> skillRangeSlots = new();
        
        
        public void SetCurrentUnit(Unit unit)
        {
            Debug.Log("Set Current unit 호출");
            currentUnit = unit;
            GetSkillRange();
        }

        public void SetCurrentSlot(GridSlot slot)
        {
            Debug.Log("Set Current Slot 호출");
            currentSlot = slot;
            GetSkillRange();
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
            Holder_Index = unitIndex;
            HasUnit = true;
            
            //todo: 테스트중 → 추후 Manager.Resources.NetworkInstantiate 사용 가능한지 확인 필요 , 로직 변동 될 수 있음
            var unit= PhotonNetwork.Instantiate(_unitPrefabPath, transform.position, Quaternion.identity, 0, new object[] { unitIndex});

            unit.transform.SetParent(transform);  // 홀더 안으로 배치
            
            SetCurrentUnit(unit.GetComponent<Unit>());
            
        }

        public void SpawnSkillRangeSlot()
        {
            switch (currentUnit.SkillRangeType)
            {
                case SkillRangeType.Short1:
                    break;
                case SkillRangeType.Short2:
                    break;
            }
        }
        
        
        //스킬 범위
        public void ShowSkillRange()
        {
            //todo: 공격 범위, 스킬 범위 보여주기(shader), 시간 등
            Debug.Log(skillRangeSlots.Count);
            if(currentUnit==null || skillRangeSlots.Count == 0) return;
            
            foreach (GridSlot gridSlot in skillRangeSlots)
            {
                gridSlot.SetColor(true);
            }
        }

        public void ShowSkillApplyRange()
        {
            foreach (GridSlot gridSlot in skillRangeSlots.Where(g => g.Type == SlotType.Outer))
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
            Debug.Log("get skill range 호출");
            if (currentUnit == null || currentSlot == null)
            {
                Debug.Log("current unit null or currentslot null");
                skillRangeSlots.Clear();
                return;
            }
            
            skillRangeSlots.Clear();
            
            // var offsetList = SkillRangePattern.Offsets[currentUnit.SkillRangeType];
            
            var offsetList = SkillRangePattern.Offsets[testSkillRangeType]; //todo: 수정!!!!!!!!
            Debug.Log(offsetList.Count());

            var gridSlots = PlayerFieldManager.Instance.GetLocalFieldController().MapSlot;
            
            foreach (var offset in offsetList)
            {
                int targetRow = currentSlot.Row + offset.x;
                int targetCol = currentSlot.Column + offset.y;

                Debug.Log($"target slot : {targetRow}, {targetCol}");
                foreach (GridSlot gridSlot in gridSlots)
                {
                    Debug.Log($"{gridSlot.Row}, {gridSlot.Column}");
                    if (gridSlot.Row == targetRow && gridSlot.Column == targetCol)
                    {
                        skillRangeSlots.Add(gridSlot);
                        Debug.Log("gridslot 추가");
                    }
                }
                //var slot = gridSlots.FirstOrDefault(s => s.Row == targetRow && s.Column == targetCol);
             
             
                
            }

        }
        
        
    }
}
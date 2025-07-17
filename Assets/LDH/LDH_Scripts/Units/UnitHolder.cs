using Managers;
using Photon.Pun;
using PlayerField;
using System;
using Unit;
using UnityEngine;

namespace Units
{
    public class UnitHolder : NetworkUnit
    {
        private string _unitPrefabPath = "Prefabs/LDH_TestResource/Unit";  // 소환할 유닛 경로
        
        public int Holder_Index;
        public bool HasUnit;
        public Unit currentUnit;
        public GridSlot currentSlot;


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
            
            currentUnit = unit.GetComponent<Unit>();
            
        }

        public void SpawnSkillRangeSlot()
        {
            switch (currentUnit.SkillRangeType)
            {
                case SkillRangeType.Shor1:
                    break;
                case SkillRangeType.Short2:
                    break;
            }
        }
        
        
        //스킬 범위
        public void ShowSkillRange()
        {
            
        }

        public void HideSkillRange()
        {
            
        }

        private void MakeSkillRange()
        {
            
        }
        
    }
}
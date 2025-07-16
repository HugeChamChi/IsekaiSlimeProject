using Managers;
using Photon.Pun;
using System;
using UnityEngine;

namespace Units
{
    public class UnitHolder : NetworkUnit
    {
        private string _unitPrefabPath = "Prefabs/LDH_TestResource/Unit";  // 소환할 유닛 경로
        
        public int Holder_Index;
        public bool HasUnit;
        public Unit currentUnit;


        private void Start() => Init();

        private void Init()
        {
            //box colider 생성
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

            currentUnit = unit.GetComponent<Unit>();
        }
        
        // private void SpawnUnit(Vector3 position, int unitIndex)
        // {
        //     // Photon 네트워크 Instantiate
        //     //todo: 테스트중 → 추후 Manager.Resources.NetworkInstantiate 사용 가능한지 확인 필요 , 로직 변동 될 수 있음
        //     PhotonNetwork.Instantiate(_unitPrefabPath, position, Quaternion.identity, 0, new object[] { unitIndex});
        // }

        
    }
}
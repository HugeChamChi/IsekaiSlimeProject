using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using Photon.Pun;
using PlayerField;
using Unit;
using UnityEngine;
using UnityEngine.UI;

namespace Units
{
    /// <summary>
    /// 유닛을 소환 및 배치
    /// </summary>
    public class UnitSpawner : MonoBehaviourPun
    {
        [Header("Field Settings")]
        [SerializeField] private PlayerFieldController _fieldController;  // 로컬 플레이어 필드 (현재 클라이언트 필드)

        //[Header("Holder Setting")]
        // [SerializeField] private GameObject _unitPrefab;  // 소환할 유닛 프리팹
        //private string _unitPrefabPath = "Prefabs/LDH_TestResource/Unit";  // 소환할 유닛 경로
        
        [Header("Unit Composition Setting")] 
        [SerializeField] private int  _epicUnitsForLegendary = 5;  // 레전더리 등급 유닛 소환을 위해 필요한 에픽 유닛 개수
        [SerializeField] private int _baseUnitsForMerge = 3; // 레전더리 제외 유닛들을 합성할 때 필요한 유닛 개수. 즉 보통 유닛 3개면 -> 레어 유닛 1개로 합성되는데 이때 이 유닛 '3'개를 가리키는 변수
        
        
        [Header("UI")] 
        [SerializeField] private Button _summonButton;    // 소환 버튼
        
        
        

        //--- 코드 이동된 부분---//
        // [SerializeField] private List<Vector2> spawnList = new();          // 슬롯 좌표 목록
        // private List<bool> spawnListArray = new();        // 슬롯 점유 여부 (false = 빈칸)

        #region Unity LifeCycle

        private void Awake() => Init();
        private void Start() => Subscribe();
        private void OnDestroy() => Unsubscribe();

        #endregion


        #region Initialization
        
        /// <summary>
        /// 초기화 및 이벤트 구독.
        /// </summary>
        private void Init()
        {
            
        }

        //구독
        private void Subscribe()
        {
            _summonButton.onClick.AddListener(() =>Summon()); //소환 버튼 이벤트 구독
            PlayerFieldManager.Instance.OnRegisterLocalFieldcontroller += SetSpawnPanel;
        }

        //구독 해제
        private void Unsubscribe()
        {
            _summonButton.onClick.RemoveListener(() =>Summon()); //소환 버튼 이벤트 구독 해제
        }
        
        /// <summary>
        /// 클라이언트 필드 컨트롤러 설정.
        /// </summary>
        public void SetSpawnPanel(PlayerFieldController fieldController)
        {
            _fieldController = fieldController;
        }
        #endregion
        

        #region Slot
        
        /// <summary>
        /// 현재 비어있는 슬롯 인덱스를 반환
        /// 없으면 -1 반환
        /// </summary>
        public int GetEmptySlotIndex()
        {
            for (int i = 0; i < _fieldController.MapSlot.Count; i++)
            {
                if (_fieldController.MapSlot[i].SlotType == SlotType.Inner && !_fieldController.MapSlot[i].IsOccupied)
                    return i;
            }
            return -1;
        }

        #endregion

        #region Summon
        /// <summary>
        /// 유닛 소환 로직.
        /// 1) 랜덤 유닛 index 선택
        /// 2) 빈 슬롯 탐색 → 없으면 경고 출력
        /// 3) Photon 네트워크 Instantiate 실행
        /// </summary>
        private void Summon(UnitTier tier = UnitTier.Common)
        {
            //todo: 보유 재화... 랑 소환 비용이랑 비교해서 처리해주기
            //소환할때마다.. 비용 올라가나..?
            
            //슬롯 점유 상태 확인
            int slotIndex = GetEmptySlotIndex();
            if (slotIndex == -1)
            {
                Debug.LogWarning("빈 슬롯 없음!!");
                return;
            }
            
            
            //랜덤 유닛 뽑기
            Debug.Log("랜덤 유닛 뽑기");
            int unitIndex = Manager.UnitData.PickRandomUnitIndexByTier(tier);
            SpawnUnit(tier, unitIndex, slotIndex);
            
            
            // Manager.Resources.NetworkInstantiate<GameObject>(_unitPrefabPath, position);
            
        }
        
       

        // private void SpawnUnit()
        // {
        //     // // 슬롯 정보
        //     // GridSlot spawnSlot = _fieldController.MapSlot[slotIndex];
        //     // // 슬롯 위치 가져오기
        //     // Vector3 position = spawnSlot.SpawnPosition;
        //     //
        //     // Debug.Log($"생성할 위치 : {position}");
        //     //
        //     // // Photon 네트워크 Instantiate
        //     // //todo: 테스트중 → 추후 Manager.Resources.NetworkInstantiate 사용 가능한지 확인 필요 , 로직 변동 될 수 있음
        //     // var holder =  PhotonNetwork.Instantiate(_unitHolderPrefabPath, position, Quaternion.identity, 0).GetComponent<UnitHolder>();
        //     // holder.SetCurrentSlot(spawnSlot);
        //     // holder.SpawnUnit(unitIndex);
        //     //
        //     //
        //     // //홀더 리스트에 추가
        //     // _fieldController.UnitHolders.Add(holder);
        // }

        private void SpawnUnit(UnitTier tier, int unitIndex, int emptySlotIndex)
        {
            UnitHolder holder = _fieldController.MapSlot[emptySlotIndex].Holder;
            holder.SpawnUnit(unitIndex);

            if (tier != UnitTier.Epic)
            {
                List<UnitHolder> existingHolders = GetUnitHoldersByIndex(tier, unitIndex);

                if (existingHolders.Count >= _baseUnitsForMerge)
                { 
                    //합성 후 다음 등급의 유닛 소환
                    MergeUnits(existingHolders, tier+1);
                }
            }
            
            _fieldController.NotifyUnitChanged();
           
        }
        

        #endregion


        public List<UnitHolder> GetUnitHoldersByIndex(UnitTier tier, int unitIndex) //tier unitIndex와 일치하는 홀더 모두 가져오기
        {
            List<UnitHolder> holders = new();
            foreach (UnitHolder holder in PlayerFieldManager.Instance.GetLocalFieldController().UnitHolders)
            {
                if(holder.CurrentUnit !=null && holder.CurrentUnit.Tier == tier && holder.CurrentUnit.Index == unitIndex)
                    holders.Add(holder);
            }

            return holders;
        }
        
        private List<UnitHolder> GetUnitHoldersByTier(UnitTier tier)
        {
            List<UnitHolder> holders = new();
            foreach (UnitHolder holder in PlayerFieldManager.Instance.GetLocalFieldController().UnitHolders)
            {
                if (holder.CurrentUnit != null && holder.CurrentUnit.Tier == tier)
                    holders.Add(holder);
            }

            return holders;
        }
        #region composition
        
        public void MergeUnits(List<UnitHolder> holders, UnitTier tier)
        {
            if (tier > UnitTier.Epic)
            {
                Debug.Log("최대 티어 도달, 합성 중단.");
                return;
            }
            
            UnitHolder firstHolder = holders[0];
           
            // 기존 유닛 삭제
            for (int i = 0; i < 3; i++)
            {
                holders[i].DeleteUnit();
            }

            //첫번째 홀더 위치에 강화된 유닛 소한하기
            int unitIndex = Manager.UnitData.PickRandomUnitIndexByTier(tier);
            firstHolder.SpawnUnit(unitIndex);
            
            
            //---재합성 체크하기---//
            holders = GetUnitHoldersByIndex(tier, unitIndex);
            //필드에 같은 유닛이 2개보다 적은 경우, 2개인데 방금 생성된 유닛을 포함하는 경우는 재합성 대상이 아님
            if(holders.Count >= _baseUnitsForMerge)
                MergeUnits(holders, tier+1);
        }

        public void MergeEpicUnits(int legendaryCombinationOrder)
        {
            var combination = PlayerFieldManager.Instance.UnitCombinations[legendaryCombinationOrder];

            List<UnitHolder> combinationHolders = new();
          
            foreach (var entry in combination.Entries)
            {
                var temp = GetUnitHoldersByIndex(entry.Tier, entry.UnitIndex);
                Debug.Log(temp.Count);
                
                for (int i = 0; i < entry.RequiredCount; i++)
                {
                    
                    combinationHolders.Add(temp[i]);
                }
            }

            UnitHolder firstHolder = combinationHolders[0];

            foreach (UnitHolder holder in combinationHolders)
            {
                holder.DeleteUnit();
            }
            
            // //필드에 있는 에픽 유닛 리스트 가져오기
            // List<UnitHolder> epicHolders = GetUnitHoldersByTier(UnitTier.Epic);
            //
            // if (epicHolders.Count < 5) return;
            
            // UnitHolder firstHolder = epicHolders[0];
            
            // 기존 유닛 삭제
            // for (int i = 0; i < 5; i++)
            // {
            //     epicHolders[i].DeleteUnit();
            // }
            //
            //첫번째 홀더 위치에 강화된 유닛 소한하기
            int unitIndex = Manager.UnitData.GetLegendaryIndex(legendaryCombinationOrder);
            firstHolder.SpawnUnit(unitIndex);
            
            _fieldController.NotifyUnitChanged();
            
        }


        #endregion
      


        #region Legacy(미사용)

        //------ 코드 이동 ------//
        // /// <summary>
        // /// 유닛이 배치될 패널 영역을 X, Y로 나누어 그리드 슬롯 좌표를 계산한다.
        // /// 좌상단부터 우측 → 아래 방향으로 순차 배치
        // /// </summary>
        // private void GenerateGridSlots()
        // {
        //     Bounds panelBounds = _spawnPanel.GetComponent<SpriteRenderer>().bounds;
        //     float panelWidth = panelBounds.size.x;
        //     float panelHeight = panelBounds.size.y;
        //
        //     float slotScaleX = _spawnPanel.lossyScale.x / _xCount;
        //     float slotScaleY = _spawnPanel.lossyScale.y / _yCount;
        //
        //     float startX = -panelWidth / 2 + slotScaleX / 2;
        //     float startY = panelHeight / 2 - slotScaleY / 2;
        //
        //     for (int row = 0; row < _yCount; row++)
        //     {
        //         for (int col = 0; col < _xCount; col++)
        //         {
        //             float xPos = startX + col * slotScaleX;
        //             float yPos = startY - row * slotScaleY;
        //             Vector2 slotPos = new Vector2(xPos, yPos) + (Vector2)_spawnPanel.position;
        //
        //             spawnList.Add(slotPos);
        //             spawnListArray.Add(false);
        //         }
        //     }
        // }
        
        

        #endregion
    }
}

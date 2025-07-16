using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using Photon.Pun;
using PlayerField;
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
        [SerializeField] private Transform _spawnPanel;    // 유닛이 배치될 부모 패널

        [Header("Unit Settings")]
        // [SerializeField] private GameObject _unitPrefab;  // 소환할 유닛 프리팹
        private string _unitPrefabPath = "Prefabs/LDH_TestResource/Unit";  // 소환할 유닛 경로
        
        [Header("UI")] 
        [SerializeField] private Button _summonButton;    // 소환 버튼

        //--- 코드 이동된 부분---//
        // [SerializeField] private List<Vector2> spawnList = new();          // 슬롯 좌표 목록
        // private List<bool> spawnListArray = new();        // 슬롯 점유 여부 (false = 빈칸)

        #region Unity LifeCycle

        private void Awake() => Init();
        // private void Start() => GenerateGridSlots();
        private void OnDestroy() => Unsubscribe();

        #endregion


        #region Initialization
        
        /// <summary>
        /// 초기화 및 이벤트 구독.
        /// </summary>
        private void Init()
        {
            Subscribe();
        }

        //구독
        private void Subscribe()
        {
            _summonButton.onClick.AddListener(Summon);
        }

        //구독 해제
        private void Unsubscribe()
        {
            _summonButton.onClick.RemoveListener(Summon);
        }
        
        /// <summary>
        /// 스폰 패널 및 필드 컨트롤러 설정.
        /// </summary>
        public void SetSpawnPanel(PlayerFieldController fieldController, Transform spawnPanel)
        {
            _fieldController = fieldController;
            _spawnPanel = spawnPanel;
            //GenerateGridSlots();  //코드 이동
        }
        #endregion
        

        #region Slot
        
        /// <summary>
        /// 현재 비어있는 슬롯 인덱스를 반환
        /// 없으면 -1 반환
        /// </summary>
        public int GetEmptySlotIndex()
        {
            
            for (int i = 0; i < _fieldController.SpawnList.Count; i++)
            {
                if (!_fieldController.SpawnListArray[i])
                    return i;
            }
            return -1;
        }

        #endregion

        #region Summon

        /// <summary>
        /// 유닛 소환
        /// 비어있는 슬롯이 없으면 경고 로그 출력
        /// </summary>
        private void Summon()
        {
            //todo: 보유 재화... 랑 소환 비용이랑 비교해서 처리해주기
            //소환할때마다.. 비용 올라가나..?
            
            int slotIndex = GetEmptySlotIndex();
            if (slotIndex == -1)
            {
                Debug.LogWarning("빈 슬롯 없음!!");
                return;
            }

            _fieldController.SpawnListArray[slotIndex] = true;
            Vector3 position =  _fieldController.SpawnList[slotIndex];

            // Manager.Resources.NetworkInstantiate<GameObject>(_unitPrefabPath, position);
            
            //todo: 테스트중 → 추후 Manager.Resources.NetworkInstantiate 사용 가능한지 확인 필요 , 로직 변동 될 수 있음
            PhotonNetwork.Instantiate(_unitPrefabPath, position, Quaternion.identity);
        }

        #endregion



        #region Legacy

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

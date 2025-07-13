using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace Units
{
    public class UnitSpawner : MonoBehaviourPun
    {
        [Header("Spawn Setting")] 
        [SerializeField] private Transform _spawnPanel;    // 유닛이 배치될 부모 패널
        [SerializeField] private int _xCount = 4;         // X축 슬롯 개수 (열)
        [SerializeField] private int _yCount = 6;         // Y축 슬롯 개수 (행)

        [Header("Unit")] 
        [SerializeField] private GameObject _unitPrefab;  // 소환할 유닛 프리팹

        [Header("UI")] 
        [SerializeField] private Button _summonButton;    // 소환 버튼

        [SerializeField] private List<Vector2> spawnList = new();          // 슬롯 좌표 목록
        private List<bool> spawnListArray = new();        // 슬롯 점유 여부 (false = 빈칸)

        #region Unity LifeCycle

        private void Awake() => Init();
        private void Start() => GenerateGridSlots();
        private void OnDestroy() => Unsubscribe();

        #endregion

        private void Init()
        {
            Subscribe();
        }

        private void Subscribe()
        {
            _summonButton.onClick.AddListener(Summon);
        }

        private void Unsubscribe()
        {
            _summonButton.onClick.RemoveListener(Summon);
        }

        #region Spawn Slot

        /// <summary>
        /// 유닛이 배치될 패널 영역을 X, Y로 나누어 그리드 슬롯 좌표를 계산한다.
        /// 좌상단부터 우측 → 아래 방향으로 순차 배치
        /// </summary>
        private void GenerateGridSlots()
        {
            Bounds panelBounds = _spawnPanel.GetComponent<SpriteRenderer>().bounds;
            float panelWidth = panelBounds.size.x;
            float panelHeight = panelBounds.size.y;

            float slotScaleX = _spawnPanel.lossyScale.x / _xCount;
            float slotScaleY = _spawnPanel.lossyScale.y / _yCount;

            float startX = -panelWidth / 2 + slotScaleX / 2;
            float startY = panelHeight / 2 - slotScaleY / 2;

            for (int row = 0; row < _yCount; row++)
            {
                for (int col = 0; col < _xCount; col++)
                {
                    float xPos = startX + col * slotScaleX;
                    float yPos = startY - row * slotScaleY;
                    Vector2 slotPos = new Vector2(xPos, yPos) + (Vector2)_spawnPanel.position;

                    spawnList.Add(slotPos);
                    spawnListArray.Add(false);
                }
            }
        }

        /// <summary>
        /// 현재 비어있는 슬롯 인덱스를 반환
        /// 없으면 -1 반환
        /// </summary>
        public int GetEmptySlotIndex()
        {
            for (int i = 0; i < spawnListArray.Count; i++)
            {
                if (!spawnListArray[i])
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
                Debug.LogWarning("No Empty Slot");
                return;
            }

            spawnListArray[slotIndex] = true;
            Vector3 position = spawnList[slotIndex];

            var go = Manager.Resources.Instantiate<GameObject>(_unitPrefab, position);
            
            //유닛 스케일 조정
            go.transform.localScale = Vector3.Scale(go.transform.localScale, transform.parent.lossyScale);
            
            
            go.name = "Unit"; // 임시 이름
            
            
        }

        // [PunRPC()]
        // private void ServerSummon()
        // {
        //     
        // }

        #endregion
    }
}

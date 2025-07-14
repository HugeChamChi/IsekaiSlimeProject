using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Managers;
using Unity.VisualScripting;

namespace Player
{
    /// <summary>
    /// 각 플레이어의 유닛 배치 공간(패널)을 관리하는 컨트롤러.
    /// </summary>
    public class PlayerFieldController : MonoBehaviour
    {
        //private PhotonView _photonView;
        
        // --- reference --- //
        private Transform _spawnPanel; // 패널 (유닛 스폰 영역)
        public Transform SpawnPanel => _spawnPanel;
        private int _actorNumber; // 각 패널의 ActorNumber
        
        
        // --- Grid Info --- //
        private List<Vector2> spawnList = new();  // 유닛을 배치할 수 있는 좌표 리스트
        private List<bool> spawnListArray = new(); // 각 슬롯의 점유 여부
        public IReadOnlyList<Vector2> SpawnList => spawnList;
        public List<bool> SpawnListArray => spawnListArray;

        
        // --- Grid Setting --- //
        private int _xCount; 
        private int _yCount;
        
        
        #region Unity LifeCycle
        
        private void Awake() => Init();

        private void Start() => GenerateGridSlots();
        #endregion
        
        
        /// <summary>
        /// 컴포넌트 초기화.
        /// </summary>
        private void Init()
        {
            //_photonView = GetComponent<PhotonView>();
            _spawnPanel = transform.GetChild(0);
            
            _xCount = PlayerFieldManager.Instance.XCount;
            _yCount = PlayerFieldManager.Instance.YCount;
        }


        #region Grid

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
                    spawnListArray.Add(false);  // false = 빈 슬롯
                }
            }
            Debug.Log("generate grid");
        }
        
        #endregion


        #region Field Register / Field Setting

        /// <summary>
        /// ActorNumber 등록 및 스케일, 색상 설정.
        /// </summary>
        public void RegisterField(int actorNumber)
        {
            _actorNumber = actorNumber;
            
            SetScale();
            
            PlayerFieldManager.Instance.RegisterPlayerField(actorNumber, this);
        }
        
        /// <summary>
        /// 내 필드는 메인 스케일, 다른 플레이어 필드는 서브 스케일로 설정한다.
        /// </summary>
        private void SetScale()
        {
            int localActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

            float scale = (_actorNumber == localActorNumber)
                ? PlayerFieldManager.Instance.MainScale
                : PlayerFieldManager.Instance.SubScale;

            transform.localScale = Vector3.one * scale;
            
            
            //색깔도 임시로 설정 (임시 색상: 본인 = 파란색, 타인 = 흰색)
            //todo: 임시 설정 부분
            Color color = (_actorNumber == localActorNumber)
                ? Color.blue
                : Color.white;

            _spawnPanel.GetComponent<SpriteRenderer>().color = color;

        }

        #endregion
        
        
        #region Gizmo

        private void OnDrawGizmos()
        {
            if (_spawnPanel == null)
            {
                return;
            }
            
            float slotScaleX = _spawnPanel.lossyScale.x / _xCount;
            float slotScaleY = _spawnPanel.lossyScale.y / _yCount;

            Vector3 slotScale = new Vector3(slotScaleX, slotScaleY, 1);
            
            Gizmos.color = Color.yellow;
            foreach (Vector2 spawnPosition in SpawnList)
            {
                Gizmos.DrawWireCube(spawnPosition, slotScale);
            }
            
        }

        #endregion
      
    }
}
using System.Collections.Generic;
using Photon.Pun;
using Player;
using Units;
using UnityEngine;

namespace Managers
{
    /// <summary>
    /// 게임 내 각 플레이어의 필드(유닛 배치 공간)를 관리하는 매니저
    /// </summary>
    public class PlayerFieldManager : MonoBehaviourPunCallbacks
    {
        // 싱글톤 (게임 씬에서만 유지)
        public static PlayerFieldManager Instance { get; private set; }
        
        // 각 플레이어의 ActorNumber로 PlayerFieldController 매핑
        public Dictionary<int, PlayerFieldController> playerFields = new();

        
        // 필드 격자 X, Y 개수 및 스케일 설정
        [field: Header("Field Settings")]
        [field: SerializeField] public int XCount { get; private set; } = 4;
        [field: SerializeField] public int YCount { get; private set; } = 4;
        [SerializeField] private float _mainScale = 1.4f;     // 로컬 플레이어 필드 스케일
        [SerializeField] private float _subScale = 0.4f;     // 다른 플레이어 필드 스케일
        public float MainScale => _mainScale;
        public float SubScale => _subScale;
        
        
        [Header("Player Field Prefab")]
        [SerializeField] private GameObject playerFieldPrefab;  // PlayerField 프리팹
        [SerializeField] private string _playerPrefabPath = "LDH_TestResource/PlayerField";
        
        [Header("Spawn Settings")]
        [SerializeField] private List<Transform> spawnPoints;   // 인원 수별 스폰 위치
        [SerializeField] private UnitSpawner _unitSpawner;
        
        
        #region Unity LifeCycle
        private void Awake()
        {
            //싱글톤 초기화
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Start() => SpawnPlayerFields();
        #endregion


        #region Field Manage

        


        /// <summary>
        /// 모든 플레이어의 PlayerField 생성 및 초기 등록
        /// </summary>
        public void SpawnPlayerFields()
        {
            var players = PhotonNetwork.PlayerList;

            int idx = 1;  // 본인(로컬)은 spawnPoints[0], 다른 사람은 순서대로
            
            for (int i = 0; i < players.Length; i++)
            {
                var player = players[i];
                Vector3 spawnPos = player.IsLocal? spawnPoints[0].position : spawnPoints[idx++].position;

                // var fieldObj = PhotonNetwork.Instantiate(playerFieldPrefab.name, spawnPos, Quaternion.identity);
                
                // 리소스 매니저를 통한 프리팹 인스턴스화 (클라이언트에서 로컬로 생성)
                // 네트워크 동기화 불필요
                var playerField = Manager.Resources.Instantiate(playerFieldPrefab, spawnPos).GetComponent<PlayerFieldController>();
                
                playerField.RegisterField(player.ActorNumber);
            }
        }


        /// <summary>
        /// PlayerField 등록 및 로컬 플레이어의 경우 UnitSpawner 연동
        /// </summary>
        public void RegisterPlayerField(int actorNumber, PlayerFieldController fieldController)
        {
            playerFields[actorNumber] = fieldController;
            
            if(actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                _unitSpawner.SetSpawnPanel(fieldController, fieldController.SpawnPanel);
        }
        
        /// <summary>
        /// 특정 ActorNumber의 PlayerFieldController 반환
        /// </summary>
        public PlayerFieldController GetFieldController(int actorNumber)
        {
            if (playerFields.TryGetValue(actorNumber, out var field))
            {
                return field;
            }
            else
            {
                Debug.LogWarning($"{actorNumber}의 필드를 찾을 수 없습니다.");
                return null;
            }
        }
        #endregion
      
    }
}

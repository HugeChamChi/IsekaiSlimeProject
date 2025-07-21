using DesignPattern;
using System;
using System.Collections.Generic;
using Units;
using UnityEngine;
using Photon.Pun;
using System.Collections;


namespace Managers
{
    public class InGameManager : MonoBehaviourPunCallbacks
    {
        [Header("Game Settings")] 
        
        [SerializeField] private float gameStartDelay = 5f;
        [SerializeField] private MapManager mapManager;
        
        
        public static InGameManager Instance { get; private set; }
        
        private int idCounter = 1000; // 시작 값 (원하면 1, 1000, 10000 등으로)
        public UnitHolder SelectedHolder { get; private set; }

        public event Action<UnitHolder> OnSelectedHolderChanged;
        
        public event Action OnGameStarted;

        private bool gameStarted = false;
        
        
        
        
        public Dictionary<int, Transform> inGameObjects = new();


        private void Awake()
        {
            Debug.Log("초기화");
            //싱글톤 초기화
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                StartCoroutine(PrepareGameStart());
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
        
        public int GenerateUniqueID()
        {
            idCounter++;
            return idCounter;
        }

        /// <summary>
        /// 게임 시작 준비 - 지연 시간 후 자동 시작하게끔 
        /// </summary>

        private IEnumerator PrepareGameStart()
        {
            // 일단 모든 플레이어가 씬에 로드 될때까지 대기함
            yield return new WaitForSeconds(1f);
            
            // 게임 시작 카운트 시작
            yield return new WaitForSeconds(gameStartDelay - 1f);

            StartGame();
        }

        void StartGame()
        {
            if (gameStarted) return;

            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("OnGameStart", RpcTarget.All);
            }
        }

        [PunRPC]
        void OnGameStart()
        {
            if (gameStarted) return;
            
            gameStarted = true;

            // mapmanager 초기화
            if (mapManager != null)
            {
                mapManager.InitializeGame();
            }
            
            OnGameStarted?.Invoke();
        }
        
        public void RegisterInGameObject(InGameObject obj)
        {
            inGameObjects[obj.uniqueID] = obj.transform;
            //Debug.Log($"Registered object {obj.name} with ID {obj.uniqueID}");
        }
        public Transform GetInGameObjectByID(int uniqueID)
        {
            inGameObjects.TryGetValue(uniqueID, out var transform);
            return transform;
        }
        
        
        public void SetSelectedHolder(UnitHolder holder)
        {
            SelectedHolder = holder;
            OnSelectedHolderChanged?.Invoke(holder);

            Debug.Log($"[GameManager] Selected holder: {holder.name}");
            
        }
        public void ClearSelectedHolder()
        {
            SelectedHolder = null;
            OnSelectedHolderChanged?.Invoke(null);
            Debug.Log($"[GameManager] Cleared selected holder");
        }
        
        
    }
}
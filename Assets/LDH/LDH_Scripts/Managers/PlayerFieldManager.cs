using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Managers
{
    public class PlayerFieldManager : MonoBehaviourPunCallbacks
    {
        // 싱글톤 (게임 씬에서만 유지)
        public static PlayerFieldManager Instance { get; private set; }
        
        // ActorNumber별 PlayerField Transform 관리
        public Dictionary<int, Transform> playerFields = new();

        [Header("필드 스케일 설정")]
        [SerializeField] private float _mainScale = 1.4f;     // 내 필드 스케일
        [SerializeField] private float _subScale = 0.4f;    // 다른 사람 필드 스케일

        [Header("생성용 프리팹 및 위치")]
        [SerializeField] private GameObject playerFieldPrefab;  // PlayerField 프리팹
        [SerializeField] private List<Transform> spawnPoints;   // 인원 수별 스폰 위치치

        
        private void Awake()
        {
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
        
        
        /// <summary>
        /// PlayerField를 인원 수에 맞게 생성하고 등록한다.
        /// </summary>
        public void SpawnPlayerFields()
        {
            var players = PhotonNetwork.PlayerList;

            for (int i = 0; i < players.Length; i++)
            {
                var player = players[i];
                Vector3 spawnPos = spawnPoints[i].position;

                var fieldObj = PhotonNetwork.Instantiate(playerFieldPrefab.name, spawnPos, Quaternion.identity);
                RegisterPlayerField(player.ActorNumber, fieldObj.transform);
            }

            SetupViews();
        }


        /// <summary>
        /// ActorNumber에 따라 PlayerField를 등록한다.
        /// </summary>
        public void RegisterPlayerField(int actorNumber, Transform field)
        {
            playerFields[actorNumber] = field;
        }


        /// <summary>
        /// ActorNumber로 PlayerField Transform을 가져온다.
        /// </summary>
        public Transform GetFieldTransform(int actorNumber)
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

        /// <summary>
        /// 내 필드는 메인 스케일, 다른 플레이어 필드는 서브 스케일로 설정한다.
        /// </summary>
        public void SetupViews()
        {
            int localActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

            foreach (var playerFieldEntry in playerFields)
            {
                int actorNumber = playerFieldEntry.Key;
                Transform field = playerFieldEntry.Value;

                if (actorNumber == localActorNumber)
                {
                    field.localScale = Vector3.one * _mainScale;
                    // 추가: 내 필드 특수 처리
                }
                else
                {
                    field.localScale = Vector3.one * _subScale;
                    // 추가: 서브 필드 특수 처리
                }
            }
        }
    }
}

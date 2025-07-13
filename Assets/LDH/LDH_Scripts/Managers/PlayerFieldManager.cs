using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Managers
{
    public class PlayerFieldManager : MonoBehaviourPunCallbacks
    {
        // 싱글톤
        public static PlayerFieldManager Instance { get; private set; } 
        
        // ActorNumber별 PlayerField Transform 관리
        public Dictionary<int, Transform> playerFields = new();

        [SerializeField] private float _subScale = 0.3f;
        [SerializeField] private float _mainScale;
        
        
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

        // 플레이어 등록
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

        // 뷰 셋팅
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

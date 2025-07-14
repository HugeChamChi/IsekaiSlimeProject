using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace LDH.LDH_Scripts.Temp
{
    public class NetworkTest : MonoBehaviourPunCallbacks
    {
        [SerializeField] private string roomName = "TestRoom";
        [SerializeField] private Button switchSceneButton;
        [SerializeField] private string targetSceneName;
        private void Start()
        {
            PhotonNetwork.AutomaticallySyncScene = true;

            // ParrelSync clone name → 닉네임 구분
#if UNITY_EDITOR
            PhotonNetwork.NickName = UnityEditor.EditorPrefs.GetString("ParrelSync_CloneName", "Main");
#else
        PhotonNetwork.NickName = "Build";
#endif

            Debug.Log($"[Photon] Connecting as {PhotonNetwork.NickName}");
            PhotonNetwork.ConnectUsingSettings();
            
           
            
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("[Photon] Connected to Master");

            RoomOptions roomOptions = new RoomOptions { MaxPlayers = 4 };
            PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log($"[Photon] Joined Room: {roomName} as {PhotonNetwork.NickName}");
            // 버튼 초기화
            if (PhotonNetwork.IsMasterClient)
            {
                switchSceneButton.gameObject.SetActive(true);
                switchSceneButton.onClick.AddListener(OnSwitchSceneButtonClicked);
            }
            else
            {
                switchSceneButton.gameObject.SetActive(false);
            }
            
            
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"[Photon] Failed to join room: {message}");
        }
        
        private void OnSwitchSceneButtonClicked()
        {
            Debug.Log("[Photon] MasterClient: Switching scene...");
            PhotonNetwork.LoadLevel(targetSceneName);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPun, IMatchmakingCallbacks, IInRoomCallbacks
{
    [Header("Room Code UI")]
    [SerializeField] private TextMeshProUGUI roomCodeText;
    [SerializeField] private Button generateCodeButton;
    [SerializeField] private Button joinRoomButton;
    
    [Header("Code Input (Join Room)")]
    [SerializeField] private GameObject codeInputPanel;
    [SerializeField] private TMP_InputField codeInputField;
    [SerializeField] private Button confirmJoinButton;
    [SerializeField] private Button cancelJoinButton;
    
    [Header("Game UI")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button settingButton;
    
    [Header("Audio")]
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    
    [Header("Settings Panel")]
    [SerializeField] private GameObject settingPanel;
    
    
    private string currentRoomCode = "0000";
    private bool isInRoom = false;
    private bool hasGeneratedCode = false; 
    
    private void Start()
    {
        InitializeUI();
        InitializeAudio();
        
      
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    
    private void InitializeUI()
    {
     
        if (roomCodeText != null) roomCodeText.text = currentRoomCode;
        if (codeInputPanel != null) codeInputPanel.SetActive(false);
        if (settingPanel != null) settingPanel.SetActive(false);
        
        // 버튼 이벤트 연결
        if (generateCodeButton != null) generateCodeButton.onClick.AddListener(OnGenerateCodeClick);
        if (joinRoomButton != null) joinRoomButton.onClick.AddListener(OnJoinRoomClick);
        if (confirmJoinButton != null) confirmJoinButton.onClick.AddListener(OnConfirmJoinClick);
        if (cancelJoinButton != null) cancelJoinButton.onClick.AddListener(OnCancelJoinClick);
        if (startButton != null) startButton.onClick.AddListener(OnStartClick);
        if (shopButton != null) shopButton.onClick.AddListener(OnShopClick);
        if (settingButton != null) settingButton.onClick.AddListener(OnSettingClick);
        
        // 입력 필드 엔터키 처리
        if (codeInputField != null)
        {
            codeInputField.onEndEdit.AddListener(delegate { 
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    OnConfirmJoinClick();
                }
            });
        }
        
        // 시작 버튼 비활성화
        if (startButton != null) startButton.interactable = false;
        
        
        SetGenerateButtonText("GetCodeKey");
    }
    
    private void InitializeAudio()
    {
        if (sfxAudioSource != null && sfxSlider != null)
        {
            sfxSlider.value = sfxAudioSource.volume;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        if (bgmAudioSource != null && bgmSlider != null)
        {
            bgmSlider.value = bgmAudioSource.volume;
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }
    }
    
    #region Button Events
    
    public void OnGenerateCodeClick()
    {
        PlaySFX();
        
        if (isInRoom)
        {
            // 방에 있으면 방 나가기
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            // 코드가 아직 생성되지 않았거나 GetCodeKey 상태면 새로운 방 생성
            if (!hasGeneratedCode || GetGenerateButtonText() == "GetCodeKey")
            {
                CreateRandomRoom();
            }
            else
            {
                // 이미 코드가 생성되었다면 해당 코드로 방 다시 생성
                CreateRandomRoom();
            }
        }
    }
    
    public void OnJoinRoomClick()
    {
        PlaySFX();
        
        if (isInRoom)
        {
            // 방에 있으면 방 나가기
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            // 코드 입력 패널 열기
            OpenCodeInputPanel();
        }
    }
    
    public void OnConfirmJoinClick()
    {
        PlaySFX();
        TryJoinRoom();
    }
    
    public void OnCancelJoinClick()
    {
        PlaySFX();
        CloseCodeInputPanel();
    }
    
    public void OnStartClick()
    {
        PlaySFX();
        
        if (isInRoom && PhotonNetwork.IsMasterClient)
        {
                SceneManager.LoadScene("InGameScene");
        }
    }
    
    public void OnShopClick()
    {
        PlaySFX();
        SceneManager.LoadScene("GachaScene");
    }
    
    public void OnSettingClick()
    {
        PlaySFX();
        OpenSettingPanel();
    }
    
    #endregion
    
    #region Room Management
    
    private void CreateRandomRoom()
    {
        if (!PhotonNetwork.IsConnected) return;
    
        currentRoomCode = GenerateRandomRoomCode();
    
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4;
        roomOptions.IsVisible = false;
        roomOptions.IsOpen = true;
    
        PhotonNetwork.CreateRoom(currentRoomCode, roomOptions);
    
        Debug.Log($"방 생성 시도: {currentRoomCode}");
    }
    
    private string GenerateRandomRoomCode()
    {
        return Random.Range(1000, 9999).ToString();
    }
    
    private void TryJoinRoom()
    {
        if (!PhotonNetwork.IsConnected) return;
        
        string inputCode = codeInputField.text.Trim();
        
        if (inputCode.Length != 4)
        {
            Debug.Log("4자리 방 코드를 입력하세요");
            return;
        }
        
        if (!int.TryParse(inputCode, out int code))
        {
            Debug.Log("숫자만 입력 가능합니다");
            return;
        }
        
        PhotonNetwork.JoinRoom(inputCode);
        CloseCodeInputPanel();
        
        Debug.Log($"방 참가 시도: {inputCode}");
    }
    
    #endregion
    
    #region UI Management
    
    private void OpenCodeInputPanel()
    {
        if (codeInputPanel != null)
        {
            codeInputPanel.SetActive(true);
            if (codeInputField != null)
            {
                codeInputField.text = "";
                codeInputField.Select();
            }
        }
    }
    
    private void CloseCodeInputPanel()
    {
        if (codeInputPanel != null)
            codeInputPanel.SetActive(false);
    }
    
    public void OpenSettingPanel()
    {
        if (settingPanel != null)
            settingPanel.SetActive(true);
    }

    public void CloseSettingPanel()
    {
        if (settingPanel != null)
            settingPanel.SetActive(false);
    }
    
    private void UpdateRoomUI()
    {
        if (isInRoom)
        {
            // 방에 있을 때
            if (roomCodeText != null) roomCodeText.text = PhotonNetwork.CurrentRoom.Name;
            
            // 방에 있을 때는 Generate 버튼 텍스트를 "방 나가기"로 변경하지 않음
            // 대신 생성된 코드를 유지
            UpdateButtonText(joinRoomButton, "방 나가기");
            
            // 시작 버튼은 방장만 활성화
            if (startButton != null)
                startButton.interactable = PhotonNetwork.IsMasterClient;
        }
        else
        {
            // 방에 없을 때
            if (roomCodeText != null) roomCodeText.text = "0000";
            
            // 버튼 텍스트 원래대로 - 코드 생성 여부에 따라 다르게 표시
            if (hasGeneratedCode)
            {
                SetGenerateButtonText(currentRoomCode); // 생성된 코드 표시
            }
            else
            {
                SetGenerateButtonText("GetCodeKey"); // 초기 상태
            }
            
            UpdateButtonText(joinRoomButton, "참가하기");
            
            // 시작 버튼 비활성화
            if (startButton != null)
                startButton.interactable = false;
        }
    }
    
    private void UpdateButtonText(Button button, string text)
    {
        if (button != null)
        {
            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null) buttonText.text = text;
        }
    }
    
    // Generate 버튼 텍스트 설정 전용 메서드
    private void SetGenerateButtonText(string text)
    {
        if (generateCodeButton != null)
        {
            Debug.Log("이거 호출되나요 ? 근데 왜 안ㄷ지ㅗ나요 ? ");
            var buttonText = generateCodeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null) buttonText.text = text;
        }
    }
    
    // Generate 버튼 텍스트 가져오기 전용 메서드
    private string GetGenerateButtonText()
    {
        if (generateCodeButton != null)
        {
            var buttonText = generateCodeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null) return buttonText.text;
        }
        return "";
    }
    
    #endregion
    
    #region Audio Controls
    
    public void SetSFXVolume(float volume)
    {
        if (sfxAudioSource != null)
            sfxAudioSource.volume = volume;
    }

    public void SetBGMVolume(float volume)
    {
        if (bgmAudioSource != null)
            bgmAudioSource.volume = volume;
    }
    
    private void PlaySFX()
    {
        if (sfxAudioSource != null)
            sfxAudioSource.Play();
    }
    
    #endregion
    
    #region Photon Callbacks (필수만)
    
    public void OnCreatedRoom()
    {
        isInRoom = true;
        hasGeneratedCode = true; // 코드 생성 완료
        currentRoomCode = PhotonNetwork.CurrentRoom.Name;
        
        if(roomCodeText!=null) roomCodeText.text = PhotonNetwork.CurrentRoom.Name;
        
        
        // 방 생성 직후에는 버튼에 생성된 코드를 표시
        SetGenerateButtonText(currentRoomCode);
        
        UpdateRoomUI();
        Debug.Log($"방 생성 완료: {currentRoomCode}");
    }
    
    public void OnJoinedRoom()
    {
        isInRoom = true;
        currentRoomCode = PhotonNetwork.CurrentRoom.Name;
        UpdateRoomUI();
        Debug.Log($"방 참가 완료: {currentRoomCode}");
    }
    
    public void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log($"방 생성 실패: {message}");
        // 다시 시도
        CreateRandomRoom();
    }
    
    public void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log($"방 참가 실패: {message}");
        // 다시 입력 패널 열기
        OpenCodeInputPanel();
    }
    
    public void OnLeftRoom()
    {
        isInRoom = false;
        // 방을 나갔을 때 코드는 유지하되 UI 업데이트
        UpdateRoomUI();
        Debug.Log("방 나가기 완료");
    }
    
    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"{newPlayer.NickName} 입장");
    }
    
    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"{otherPlayer.NickName} 퇴장");
        
        // 방장이 바뀌었을 때 시작 버튼 업데이트
        if (startButton != null)
            startButton.interactable = PhotonNetwork.IsMasterClient;
    }
    
    // 필수 인터페이스 메서드들
    public void OnFriendListUpdate(List<FriendInfo> friendList) { }
    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics) { }
    public void OnRoomListUpdate(List<RoomInfo> roomList) { }
    public void OnJoinRandomFailed(short returnCode, string message) { }
    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) { }
    public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps) { }
    public void OnMasterClientSwitched(Player newMasterClient) 
    {
        // 방장이 바뀌었을 때 시작 버튼 업데이트
        if (startButton != null)
            startButton.interactable = PhotonNetwork.IsMasterClient;
    }
    
    #endregion
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }
}
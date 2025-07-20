using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks, IMatchmakingCallbacks
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
    
    [Header("Player Panel")]
    [SerializeField] private GameObject playerPanel;
    [SerializeField] private PlayerPanelItem[] playerPanelItems = new PlayerPanelItem[4];
    
    [Header("Audio")]
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    
    [Header("Settings Panel")]
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private SettingPanels settingPanelScript; 
    
    private string currentRoomCode = "0000";
    private bool isInRoom = false;
    private bool hasGeneratedCode = false;
    private float bgmVolume = 1f;
    private float sfxVolume = 1f;
    
    // Player Panel 관리용
    private Dictionary<int, PlayerPanelItem> playerPanelDict = new Dictionary<int, PlayerPanelItem>();
    
    private void Start()
    {
        InitializeUI();
        InitializeAudio();

        // 씬 동기화 활성화
        PhotonNetwork.AutomaticallySyncScene = true;
        
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
        if (playerPanel != null) playerPanel.SetActive(false); // 플레이어 패널 초기 비활성화

        if (generateCodeButton != null) generateCodeButton.onClick.AddListener(OnGenerateCodeClick);
        if (joinRoomButton != null) joinRoomButton.onClick.AddListener(OnJoinRoomClick);
        if (confirmJoinButton != null) confirmJoinButton.onClick.AddListener(OnConfirmJoinClick);
        if (cancelJoinButton != null) cancelJoinButton.onClick.AddListener(OnCancelJoinClick);
        if (startButton != null) startButton.onClick.AddListener(OnStartClick);
        if (shopButton != null) shopButton.onClick.AddListener(OnShopClick);
        if (settingButton != null) settingButton.onClick.AddListener(OnSettingClick);

        SetupAudioSliders();

        if (codeInputField != null)
        {
            codeInputField.onEndEdit.AddListener(delegate { 
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    OnConfirmJoinClick();
                }
            });
        }
        if (startButton != null) 
        {
            startButton.gameObject.SetActive(false); // 초기에는 숨김
        }
        SetGenerateButtonText("GetCodeKey");
        
        ValidatePlayerPanelComponents();
    }
    
    private void ValidatePlayerPanelComponents()
    {
        if (playerPanel == null)
        {
            Debug.LogError("LobbyManager: playerPanel이 할당되지 않았습니다.");
        }
        
        // 4개의 PlayerPanelItem이 모두 할당되었는지 확인
        for (int i = 0; i < playerPanelItems.Length; i++)
        {
            if (playerPanelItems[i] == null)
            {
                Debug.LogError($"LobbyManager: playerPanelItems[{i}]이 할당되지 않았습니다.");
            }
        }
    }

    private void SetupAudioSliders()
    {
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVolume;
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        if (bgmSlider != null)
        {
            bgmSlider.value = bgmVolume;
            bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }

        ApplyAudioSettings();
    }

    private void InitializeAudio()
    {
        ApplyAudioSettings();
    }

    #region Button Events

    public void OnGenerateCodeClick()
    {
        PlaySFX();

        if (isInRoom)
        {
            PhotonNetwork.LeaveRoom();
            Invoke("CreateRandomRoom", 0.5f);
        }
        else
        {
            if (PhotonNetwork.IsConnected)
            {
                CreateRandomRoom();
            }
            else
            {
                PhotonNetwork.ConnectUsingSettings();
            }
        }
    }

    public void OnJoinRoomClick()
    {
        PlaySFX();

        if (isInRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
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
            PhotonNetwork.LoadLevel("TestScene");
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

    #region Player Panel Management

    private void ShowPlayerPanel()
    {
        if (playerPanel != null)
        {
            playerPanel.SetActive(true);
            UpdatePlayerPanels();
            UpdateStartButtonVisibility();
        }
    }

    private void HidePlayerPanel()
    {
        if (playerPanel != null)
        {
            playerPanel.SetActive(false);
            ClearAllPlayerPanels();
        }
    }

    private void UpdatePlayerPanels()
    {
        if (!isInRoom)
        {
            return;
        }

        // 모든 패널 초기화 (빈 상태로)
        ClearAllPlayerPanels();

        // 현재 방의 모든 플레이어 가져오기
        Player[] players = PhotonNetwork.PlayerList;
        
        // 플레이어를 ActorNumber 순으로 정렬 (입장 순서대로)
        System.Array.Sort(players, (p1, p2) => p1.ActorNumber.CompareTo(p2.ActorNumber));
        
        // 플레이어 수만큼 패널에 할당 (왼쪽부터)
        for (int i = 0; i < players.Length && i < playerPanelItems.Length; i++)
        {
            if (playerPanelItems[i] != null)
            {
                playerPanelItems[i].Init(players[i]);
                playerPanelDict[players[i].ActorNumber] = playerPanelItems[i];
                
                Debug.Log($"플레이어 패널 설정: {players[i].NickName} -> Panel {i}");
            }
        }
        
        UpdateStartButtonVisibility();
    }

    private void AddPlayerToPanel(Player newPlayer)
    {
        if (!isInRoom || newPlayer == null)
        {
            return;
        }

        // 이미 패널에 있는 플레이어인지 확인
        if (playerPanelDict.ContainsKey(newPlayer.ActorNumber))
        {
            return;
        }

        // 빈 패널 찾기 (첫 번째 빈 패널에 추가)
        for (int i = 0; i < playerPanelItems.Length; i++)
        {
            if (playerPanelItems[i] != null && !playerPanelItems[i].HasPlayer())
            {
                playerPanelItems[i].Init(newPlayer);
                playerPanelDict[newPlayer.ActorNumber] = playerPanelItems[i];
                
                Debug.Log($"새 플레이어 패널 추가: {newPlayer.NickName} -> Panel {i}");
                UpdateStartButtonVisibility();
                return;
            }
        }

        Debug.LogWarning("빈 플레이어 패널을 찾을 수 없습니다.");
    }

    private void RemovePlayerFromPanel(Player leftPlayer)
    {
        if (leftPlayer == null || !playerPanelDict.ContainsKey(leftPlayer.ActorNumber))
        {
            return;
        }

        PlayerPanelItem panelItem = playerPanelDict[leftPlayer.ActorNumber];
        if (panelItem != null)
        {
            panelItem.ClearPlayer(); // 패널을 빈 상태로 만들기
            Debug.Log($"플레이어 패널 클리어: {leftPlayer.NickName}");
        }

        playerPanelDict.Remove(leftPlayer.ActorNumber);
        
        // 플레이어가 나간 후 패널 재정렬 (왼쪽으로 밀기)
        ReorganizePlayerPanels();
        UpdateStartButtonVisibility();
    }

    /// <summary>
    /// 플레이어가 나간 후 빈 공간을 없애고 왼쪽으로 밀어서 정렬
    /// </summary>
    private void ReorganizePlayerPanels()
    {
        if (!isInRoom)
        {
            return;
        }

        // 현재 활성화된 플레이어들 수집
        List<Player> activePlayers = new List<Player>();
        foreach (var kvp in playerPanelDict)
        {
            if (kvp.Value.HasPlayer())
            {
                activePlayers.Add(kvp.Value.GetCurrentPlayer());
            }
        }

        // ActorNumber 순으로 정렬
        activePlayers.Sort((p1, p2) => p1.ActorNumber.CompareTo(p2.ActorNumber));

        // 모든 패널 클리어
        ClearAllPlayerPanels();

        // 다시 왼쪽부터 순서대로 배치
        for (int i = 0; i < activePlayers.Count && i < playerPanelItems.Length; i++)
        {
            if (playerPanelItems[i] != null)
            {
                playerPanelItems[i].Init(activePlayers[i]);
                playerPanelDict[activePlayers[i].ActorNumber] = playerPanelItems[i];
            }
        }
    }

    private void ClearAllPlayerPanels()
    {
        // 모든 패널을 빈 상태로 만들기 (패널 자체는 활성화 유지)
        for (int i = 0; i < playerPanelItems.Length; i++)
        {
            if (playerPanelItems[i] != null)
            {
                playerPanelItems[i].ClearPlayer();
            }
        }

        // Dictionary 초기화
        playerPanelDict.Clear();
    }

    /// <summary>
    /// 스타트 버튼 표시 여부 업데이트 (방장만 보이게)
    /// </summary>
    private void UpdateStartButtonVisibility()
    {
        if (startButton != null)
        {
            bool showStartButton = isInRoom && PhotonNetwork.IsMasterClient;
            startButton.gameObject.SetActive(showStartButton);
            
            if (showStartButton)
            {
                startButton.interactable = true;
            }
        }
    }

    #endregion

    #region Audio Settings

    private void OnBGMVolumeChanged(float volume)
    {
        bgmVolume = volume;
        ApplyAudioSettings();
        SaveAudioSettings();
    }

    private void OnSFXVolumeChanged(float volume)
    {
        sfxVolume = volume;
        ApplyAudioSettings();
        SaveAudioSettings();
    }

    private void ApplyAudioSettings()
    {
        if (bgmAudioSource != null)
            bgmAudioSource.volume = bgmVolume;
        if (sfxAudioSource != null && sfxAudioSource.enabled)
            sfxAudioSource.volume = sfxVolume;
    }

    private void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    private void PlaySFX()
    {
        if (sfxAudioSource != null && sfxAudioSource.enabled)
            sfxAudioSource.Play();
    }

    #endregion

    #region Room Management

    private void CreateRandomRoom()
    {
        currentRoomCode = GenerateRandomRoomCode();

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4;
        roomOptions.IsVisible = false;
        roomOptions.IsOpen = true;
        
        PhotonNetwork.CreateRoom(currentRoomCode, roomOptions);
    }

    private string GenerateRandomRoomCode()
    {
        return UnityEngine.Random.Range(1000, 9999).ToString();
    }

    private void TryJoinRoom()
    {
        if (!PhotonNetwork.IsConnected) 
        {
            ShowMessage("서버에 연결되지 않았습니다.");
            return;
        }

        string inputCode = codeInputField.text.Trim();

        if (inputCode.Length != 4)
        {
            ShowMessage("4자리 방 코드를 입력하세요");
            return;
        }

        if (!int.TryParse(inputCode, out int code))
        {
            ShowMessage("숫자만 입력 가능합니다");
            return;
        }

        PhotonNetwork.JoinRoom(inputCode);
        CloseCodeInputPanel();
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
        if (settingPanelScript != null)
        {
            settingPanelScript.OpenPanel();
        }
        else if (settingPanel != null)
        {
            settingPanel.SetActive(true);
        }
    }

    public void CloseSettingPanel()
    {
        if (settingPanelScript != null)
        {
            settingPanelScript.ClosePanel();
        }
        else if (settingPanel != null)
        {
            settingPanel.SetActive(false);
        }
    }

    private void UpdateRoomUI()
    {
        if (isInRoom)
        {
            if (roomCodeText != null) roomCodeText.text = PhotonNetwork.CurrentRoom.Name;

            UpdateButtonText(generateCodeButton, "방 나가기");
            UpdateButtonText(joinRoomButton, "방 나가기");

            ShowPlayerPanel(); // 방에 입장하면 플레이어 패널 표시
        }
        else
        {
            if (roomCodeText != null) roomCodeText.text = "0000";

            if (hasGeneratedCode)
            {
                SetGenerateButtonText(currentRoomCode);
            }
            else
            {
                SetGenerateButtonText("GetCodeKey");
            }

            UpdateButtonText(joinRoomButton, "참가하기");

            // 방을 나갔을 때 스타트 버튼 숨기기
            if (startButton != null)
            {
                startButton.gameObject.SetActive(false);
            }
                
            HidePlayerPanel(); // 방을 나가면 플레이어 패널 숨김
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

    private void SetGenerateButtonText(string text)
    {
        if (generateCodeButton != null)
        {
            var buttonText = generateCodeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null) buttonText.text = text;
        }
    }

    private void ShowMessage(string message)
    {
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowPopup(message);
        }
    }

    #endregion

    #region Photon Callbacks

    public void OnCreatedRoom()
    {
        isInRoom = true;
        hasGeneratedCode = true;
        currentRoomCode = PhotonNetwork.CurrentRoom.Name;

        if(roomCodeText != null) 
        {
            roomCodeText.text = currentRoomCode;
        }

        SetGenerateButtonText(currentRoomCode);
        UpdateButtonText(joinRoomButton, "방 나가기");

        ShowMessage($"방이 생성되었습니다! 코드: {currentRoomCode}");
        ShowPlayerPanel(); // 방 생성 시 플레이어 패널 표시
    }

    public void OnJoinedRoom()
    {
        isInRoom = true;
        currentRoomCode = PhotonNetwork.CurrentRoom.Name;

        if(roomCodeText != null) 
        {
            roomCodeText.text = currentRoomCode;
        }

        SetGenerateButtonText($"Code: {currentRoomCode}");
        UpdateButtonText(joinRoomButton, "방 나가기");

        ShowMessage($"방에 참가했습니다! 코드: {currentRoomCode}");
        ShowPlayerPanel(); // 방 참가 시 플레이어 패널 표시
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"플레이어 입장: {newPlayer.NickName}");
        
        if (isInRoom)
        {
            AddPlayerToPanel(newPlayer);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"플레이어 퇴장: {otherPlayer.NickName}");
        
        RemovePlayerFromPanel(otherPlayer);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"방장 변경: {newMasterClient.NickName}");
        
        UpdateStartButtonVisibility();
        
        // 모든 패널의 방장 표시 업데이트
        foreach (var panelItem in playerPanelItems)
        {
            if (panelItem != null && panelItem.HasPlayer())
            {
                // PlayerPanelItem의 OnMasterClientSwitched가 자동으로 호출되어 업데이트됨
            }
        }
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
        ShowMessage($"방 생성 실패: {message}");
        Invoke("CreateRandomRoom", 1f);
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
        ShowMessage($"방 참가 실패: {message}");
        OpenCodeInputPanel();
    }

    public void OnLeftRoom()
    {
        isInRoom = false;

        if (roomCodeText != null) 
            roomCodeText.text = "0000";

        SetGenerateButtonText("Get CodeKey");
        UpdateButtonText(joinRoomButton, "참가하기");

        if (startButton != null)
            startButton.gameObject.SetActive(false); // 스타트 버튼 숨기기
            
        HidePlayerPanel(); // 방을 나갔을 때 플레이어 패널 숨김
    }

    public void OnFriendListUpdate(List<FriendInfo> friendList) { }
    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics) { }
    public void OnRoomListUpdate(List<RoomInfo> roomList) { }
    public void OnJoinRandomFailed(short returnCode, string message) { }

    #endregion
}
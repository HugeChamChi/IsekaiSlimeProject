using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerPanelItem : MonoBehaviourPunCallbacks
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private Image hostImage; 
    
    private Player currentPlayer;
    private bool isInitialized = false;
    private bool hasPlayer = false; 
    
    private void Awake()
    {
        InitializeEmptyState(); 
    }
    
    
    /// <summary>
    /// 빈 상태로 초기화 (플레이어 없음)
    /// </summary>
    private void InitializeEmptyState()
    {
        hasPlayer = false;
        currentPlayer = null;
        isInitialized = false;
        
        // 텍스트와 이미지 숨기기
        if (nicknameText != null)
        {
            nicknameText.gameObject.SetActive(false);
        }
        
        if (hostImage != null)
        {
            hostImage.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 플레이어 정보로 패널 초기화
    /// </summary>
    public void Init(Player player)
    {
        if (player == null)
        {
            Debug.LogError("PlayerPanelItem.Init: player가 null입니다.");
            InitializeEmptyState();
            return;
        }
        
        currentPlayer = player;
        hasPlayer = true;
        isInitialized = true;
        
        // UI 요소들 활성화
        ActivatePlayerUI();
        
        // 플레이어 정보 업데이트
        UpdatePlayerName();
        UpdateHostStatus();
        
        Debug.Log($"PlayerPanelItem 초기화 완료: {player.NickName}");
    }
    
    /// <summary>
    /// 플레이어 UI 요소들 활성화
    /// </summary>
    private void ActivatePlayerUI()
    {
        if (nicknameText != null)
        {
            nicknameText.gameObject.SetActive(true);
        }
        
        // hostImage는 UpdateHostStatus()에서 제어
    }
    
    /// <summary>
    /// 빈 상태로 되돌리기
    /// </summary>
    public void ClearPlayer()
    {
        InitializeEmptyState();
        Debug.Log($"PlayerPanelItem 클리어 완료: {gameObject.name}");
    }
    
    private void UpdatePlayerName()
    {
        if (!hasPlayer || nicknameText == null || currentPlayer == null)
        {
            return;
        }
        
        string displayName = currentPlayer.NickName;
        
        if (GameManager.Instance != null)
        {
            string firebaseUID = GameManager.Instance.GetPlayerFirebaseUID(currentPlayer);
            if (!string.IsNullOrEmpty(firebaseUID) && currentPlayer.CustomProperties.ContainsKey("displayName"))
            {
                string firebaseDisplayName = currentPlayer.CustomProperties["displayName"]?.ToString();
                if (!string.IsNullOrEmpty(firebaseDisplayName))
                {
                    displayName = firebaseDisplayName;
                }
            }
        }
        
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = $"Player{currentPlayer.ActorNumber}";
        }
        
        nicknameText.text = displayName;
    }
    
    private void UpdateHostStatus()
    {
        if (!hasPlayer || currentPlayer == null)
        {
            return;
        }
        
        bool isMaster = currentPlayer.IsMasterClient;
        
        if (hostImage != null)
        {
            hostImage.gameObject.SetActive(isMaster);
        }
        
        // 방장일 때 닉네임 색상 변경
        if (nicknameText != null)
        {
            nicknameText.color = isMaster ? Color.yellow : Color.white;
        }
    }
    
    #region Photon Callbacks
    
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (hasPlayer && isInitialized)
        {
            UpdateHostStatus();
        }
    }
    
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (hasPlayer && currentPlayer != null && targetPlayer.ActorNumber == currentPlayer.ActorNumber && isInitialized)
        {
            if (changedProps.ContainsKey("displayName"))
            {
                UpdatePlayerName();
            }
        }
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (hasPlayer && currentPlayer != null && otherPlayer.ActorNumber == currentPlayer.ActorNumber)
        {
            // 플레이어가 나갔으므로 패널을 빈 상태로 만들기
            ClearPlayer();
        }
    }
    
    #endregion
    
    #region Public Methods
    
    public Player GetCurrentPlayer()
    {
        return hasPlayer ? currentPlayer : null;
    }
    
    public bool HasPlayer()
    {
        return hasPlayer;
    }
    
    public bool IsPlayerMaster()
    {
        return hasPlayer && currentPlayer != null && currentPlayer.IsMasterClient;
    }
    
    #endregion
}
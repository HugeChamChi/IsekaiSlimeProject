using Firebase.Auth;
using Firebase.Extensions;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class LoginPanel : MonoBehaviourPun
{
    [Header("Panel References")]
    [SerializeField] GameObject signUpPanel;

    [Header("Input Fields")]
    [SerializeField] TMP_InputField emailInput;
    [SerializeField] TMP_InputField passInput;

    [Header("Buttons")]
    [SerializeField] Button signUpButton;
    [SerializeField] Button loginButton;

    [Header("UI Feedback")]
    [SerializeField] TMP_Text statusText;

    private bool isFirebaseLoggedIn = false;
    private bool isPhotonConnected = false;
    private bool isCustomPropertiesSet = false;
    
    private Coroutine connectionTimeoutCoroutine;
    private const float CONNECTION_TIMEOUT = 15f;

    private void Awake()
    {
        SetupButtonListeners();
        PhotonNetwork.AddCallbackTarget(this);
        Debug.Log("Photon 콜백 등록 완료");
    }

    private void Start()
    {
        if (FirebaseManager.Auth != null)
        {
            FirebaseManager.Auth.StateChanged += OnAuthStateChanged;
        }
        
        UpdateStatusText("로그인 대기 중...");
        StartCoroutine(PeriodicConnectionCheck());
    }

    private void OnDestroy()
    {
        if (FirebaseManager.Auth != null)
        {
            FirebaseManager.Auth.StateChanged -= OnAuthStateChanged;
        }
        
        PhotonNetwork.RemoveCallbackTarget(this);
        
        if (connectionTimeoutCoroutine != null)
        {
            StopCoroutine(connectionTimeoutCoroutine);
        }
    }

    private void SetupButtonListeners()
    {
        signUpButton.onClick.AddListener(SignUp);
        loginButton.onClick.AddListener(Login);
    }
    
    private void OnAuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        FirebaseAuth auth = sender as FirebaseAuth;
        if (auth != null)
        {
            if (auth.CurrentUser == null)
            {
                if (PhotonNetwork.IsConnected)
                {
                    PhotonNetwork.Disconnect();
                }
                
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ClearUserInfo();
                }
                
                ResetLoginStates();
                UpdateStatusText("로그아웃됨");
            }
        }
    }

    private void ResetLoginStates()
    {
        isFirebaseLoggedIn = false;
        isPhotonConnected = false;
        isCustomPropertiesSet = false;
        
        if (connectionTimeoutCoroutine != null)
        {
            StopCoroutine(connectionTimeoutCoroutine);
            connectionTimeoutCoroutine = null;
        }
    }

    private void SignUp()
    {
        signUpPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    private void Login()
    {
        if (string.IsNullOrEmpty(emailInput.text) || string.IsNullOrEmpty(passInput.text))
        {
            UpdateStatusText("이메일과 비밀번호를 입력해주세요");
            ShowPopup("이메일과 비밀번호를 입력해주세요.");
            return;
        }

        UpdateStatusText("Firebase 로그인 중...");
        loginButton.interactable = false;
        ResetLoginStates();

        FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(emailInput.text, passInput.text)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    UpdateStatusText("로그인이 취소되었습니다");
                    ShowPopup("로그인이 취소되었습니다.");
                    ResetLoginUI();
                    return;
                }
                if (task.IsFaulted)
                {
                    UpdateStatusText("로그인 실패했습니다");
                    ShowPopup($"로그인에 실패했습니다: {task.Exception?.GetBaseException()?.Message}");
                    ResetLoginUI();
                    return;
                }
            
                FirebaseUser user = task.Result.User;
                Debug.Log($"Firebase 로그인 성공: {user.Email}");
                
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetUserInfo(user);
                }

                isFirebaseLoggedIn = true;
                UpdateStatusText("Firebase 로그인 성공 - Photon 연결 중...");
                
                StartCoroutine(DelayedPhotonConnect());
            });
    }

    private IEnumerator DelayedPhotonConnect()
    {
        yield return new WaitForSeconds(0.5f);
        ConnectToPhoton();
    }

    private void ConnectToPhoton()
    {
        // 기존 연결 해제
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            StartCoroutine(WaitForDisconnectAndReconnect());
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.IsLoggedIn)
        {
            // AuthValues 설정
            PhotonNetwork.AuthValues = new AuthenticationValues(GameManager.Instance.UserID);
            PhotonNetwork.NickName = GameManager.Instance.GetPhotonPlayerName();
            
            Debug.Log("Photon 연결 시도 중...");
            
            // 연결 시도
            bool connectResult = PhotonNetwork.ConnectUsingSettings();
            
            if (connectResult)
            {
                connectionTimeoutCoroutine = StartCoroutine(ConnectionTimeoutCheck());
                StartCoroutine(MonitorPhotonConnection());
            }
            else
            {
                Debug.LogError("Photon 연결 시도 실패");
                UpdateStatusText("네트워크 연결 실패");
                ShowPopup("네트워크 연결에 실패했습니다.");
                ResetLoginUI();
            }
        }
        else
        {
            Debug.LogError("사용자 정보 없음");
            UpdateStatusText("사용자 정보 오류");
            ShowPopup("사용자 정보가 올바르지 않습니다.");
            ResetLoginUI();
        }
    }

    private IEnumerator WaitForDisconnectAndReconnect()
    {
        float waitTime = 0f;
        while (PhotonNetwork.IsConnected && waitTime < 3f)
        {
            yield return new WaitForSeconds(0.1f);
            waitTime += 0.1f;
        }
        
        if (!PhotonNetwork.IsConnected)
        {
            ConnectToPhoton();
        }
        else
        {
            Debug.LogError("기존 연결 해제 실패");
            UpdateStatusText("연결 해제 실패");
            ShowPopup("기존 연결을 해제할 수 없습니다.");
            ResetLoginUI();
        }
    }

    private IEnumerator ConnectionTimeoutCheck()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < CONNECTION_TIMEOUT && !PhotonNetwork.IsConnected)
        {
            yield return new WaitForSeconds(0.5f);
            elapsedTime += 0.5f;
        }
        
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogError("Photon 연결 시간 초과");
            UpdateStatusText("네트워크 연결 시간 초과");
            ShowPopup("네트워크 연결에 시간이 너무 오래 걸립니다. 다시 시도해주세요.");
            ResetLoginUI();
        }
    }

    private IEnumerator MonitorPhotonConnection()
    {
        ClientState lastState = PhotonNetwork.NetworkClientState;
        
        while (!PhotonNetwork.IsConnected && connectionTimeoutCoroutine != null)
        {
            ClientState currentState = PhotonNetwork.NetworkClientState;
            
            if (currentState != lastState)
            {
                Debug.Log($"Photon 상태 변경: {lastState} -> {currentState}");
                lastState = currentState;
                
                if (currentState == ClientState.ConnectedToMasterServer && !isPhotonConnected)
                {
                    Debug.Log("마스터 서버 연결 감지 - 수동 처리");
                    OnConnectedToMaster();
                }
            }
            
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void SetupPhotonCustomProperties()
    {
        if (GameManager.Instance != null && PhotonNetwork.IsConnected && PhotonNetwork.LocalPlayer != null)
        {
            Debug.Log("커스텀 프로퍼티 설정 중...");
            
            var customProps = GameManager.Instance.GetUserCustomProperties();
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
            
            StartCoroutine(WaitForCustomPropertiesSet());
        }
        else
        {
            Debug.LogError("커스텀 프로퍼티 설정 실패 - 필요한 조건 미충족");
        }
    }

    private IEnumerator WaitForCustomPropertiesSet()
    {
        float waitTime = 0f;
        const float maxWait = 5f;
        
        while (waitTime < maxWait)
        {
            if (PhotonNetwork.LocalPlayer?.CustomProperties != null && 
                PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("firebaseUID"))
            {
                string setUID = PhotonNetwork.LocalPlayer.CustomProperties["firebaseUID"]?.ToString();
                if (setUID == GameManager.Instance?.UserID)
                {
                    Debug.Log("커스텀 프로퍼티 설정 완료");
                    isCustomPropertiesSet = true;
                    CheckLoginComplete();
                    yield break;
                }
            }
            
            yield return new WaitForSeconds(0.1f);
            waitTime += 0.1f;
        }
        
        Debug.LogError("커스텀 프로퍼티 설정 시간 초과");
        UpdateStatusText("사용자 정보 동기화 실패");
        ResetLoginUI();
    }

    private void CheckLoginComplete()
    {
        if (isFirebaseLoggedIn && isPhotonConnected && isCustomPropertiesSet)
        {
            if (connectionTimeoutCoroutine != null)
            {
                StopCoroutine(connectionTimeoutCoroutine);
                connectionTimeoutCoroutine = null;
            }
            
            Debug.Log("모든 로그인 조건 충족 - 로비로 이동");
            UpdateStatusText("로그인 완료 - 로비로 이동 중...");
            
            emailInput.text = "";
            passInput.text = "";
            
            StartCoroutine(LoadLobbyScene());
        }
    }

    private IEnumerator LoadLobbyScene()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("LobbyScene");
    }

    private void ResetLoginUI()
    {
        loginButton.interactable = true;
        ResetLoginStates();
        UpdateStatusText("로그인 대기 중...");
    }

    private void UpdateStatusText(string status)
    {
        if (statusText != null)
        {
            statusText.text = status;
        }
        Debug.Log($"[로그인 상태] {status}");
    }

    #region Photon Callbacks

    public void OnConnectedToMaster()
    {
        Debug.Log("Photon 마스터 서버 연결 완료");
        UpdateStatusText("Photon 연결 완료 - 사용자 정보 설정 중...");
        isPhotonConnected = true;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPhotonConnectionStatus(true);
        }
        
        StartCoroutine(DelayedSetupCustomProperties());
    }

    private IEnumerator DelayedSetupCustomProperties()
    {
        yield return new WaitForSeconds(0.3f);
        SetupPhotonCustomProperties();
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"Photon 연결 해제: {cause}");
        UpdateStatusText($"Photon 연결 해제: {cause}");
        
        isPhotonConnected = false;
        isCustomPropertiesSet = false;
        
        if (connectionTimeoutCoroutine != null)
        {
            StopCoroutine(connectionTimeoutCoroutine);
            connectionTimeoutCoroutine = null;
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPhotonConnectionStatus(false);
        }
        
        if (isFirebaseLoggedIn && cause != DisconnectCause.DisconnectByClientLogic)
        {
            StartCoroutine(ReconnectToPhoton());
        }
    }

    public void OnConnectFailed()
    {
        Debug.LogError("Photon 연결 실패");
        UpdateStatusText("네트워크 연결 실패");
        ShowPopup("네트워크 연결에 실패했습니다. 인터넷 연결을 확인해주세요.");
        ResetLoginUI();
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
        Debug.Log($"지역 목록 수신: {regionHandler.SummaryToCache}");
    }

    public void OnCustomAuthenticationResponse(System.Collections.Generic.Dictionary<string, object> data)
    {
        Debug.Log("커스텀 인증 응답 수신");
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
        Debug.LogError($"커스텀 인증 실패: {debugMessage}");
        UpdateStatusText("인증 실패");
        ShowPopup("사용자 인증에 실패했습니다.");
        ResetLoginUI();
    }

    // IMatchmakingCallbacks
    public void OnFriendListUpdate(System.Collections.Generic.List<FriendInfo> friendList) { }
    public void OnCreatedRoom() { }
    public void OnCreateRoomFailed(short returnCode, string message) { }
    public void OnJoinedRoom() { }
    public void OnJoinRoomFailed(short returnCode, string message) { }
    public void OnJoinRandomFailed(short returnCode, string message) { }
    public void OnLeftRoom() { }

    private IEnumerator ReconnectToPhoton()
    {
        UpdateStatusText("Photon 재연결 중...");
        yield return new WaitForSeconds(2f);
        
        if (isFirebaseLoggedIn && !PhotonNetwork.IsConnected)
        {
            Debug.Log("Photon 재연결 시도");
            ConnectToPhoton();
        }
    }

    #endregion
    
    private IEnumerator PeriodicConnectionCheck()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            
            // 로그인 진행 중이고 아직 연결되지 않은 경우만 체크
            if (isFirebaseLoggedIn && !isPhotonConnected)
            {
                // 실제로 연결되었는데 콜백이 호출되지 않은 경우 수동 처리
                if (PhotonNetwork.IsConnected && PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
                {
                    Debug.Log("연결되었지만 콜백 미호출 - 수동 처리");
                    OnConnectedToMaster();
                    break;
                }
            }
            
            // 로그인 완료되면 체크 중단
            if (isFirebaseLoggedIn && isPhotonConnected && isCustomPropertiesSet)
            {
                break;
            }
        }
    }

    private void ShowPopup(string message)
    {
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowPopup(message);
        }
    }
    
}
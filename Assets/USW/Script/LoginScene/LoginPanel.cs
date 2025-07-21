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
            ShowPopup("이메일과 비밀번호를 입력해주세요.");
            return;
        }

        loginButton.interactable = false;
        ResetLoginStates();

        FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(emailInput.text, passInput.text)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    ShowPopup("로그인이 취소되었습니다.");
                    ResetLoginUI();
                    return;
                }
                if (task.IsFaulted)
                {
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
                
                StartCoroutine(DelayedPhotonConnect());
            });
    }

    // 회원가입 후 자동 로그인을 위한 메서드
    public void SetCredentialsAndLogin(string email, string password)
    {
        emailInput.text = email;
        passInput.text = password;
        
        StartCoroutine(DelayedAutoLogin());
    }

    private IEnumerator DelayedAutoLogin()
    {
        yield return new WaitForSeconds(0.5f);
        Login();
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
                ShowPopup("네트워크 연결에 실패했습니다.");
                ResetLoginUI();
            }
        }
        else
        {
            Debug.LogError("사용자 정보 없음");
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
            
            // 기존 프로퍼티 완전 정리 후 새로 설정
            var emptyProps = new ExitGames.Client.Photon.Hashtable();
            emptyProps["firebaseUID"] = null;
            emptyProps["email"] = null;
            emptyProps["displayName"] = null;
            emptyProps["loginTime"] = null;
            PhotonNetwork.LocalPlayer.SetCustomProperties(emptyProps);
            
            // 잠시 대기 후 새 프로퍼티 설정
            StartCoroutine(SetNewCustomProperties());
        }
        else
        {
            Debug.LogError("커스텀 프로퍼티 설정 실패 - 필요한 조건 미충족");
        }
    }

    private IEnumerator SetNewCustomProperties()
    {
        yield return new WaitForSeconds(0.3f);
        
        if (GameManager.Instance != null && PhotonNetwork.IsConnected && PhotonNetwork.LocalPlayer != null)
        {
            var customProps = GameManager.Instance.GetUserCustomProperties();
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
            
            Debug.Log($"새 커스텀 프로퍼티 설정 완료 - UID: {GameManager.Instance.UserID}");
            
            StartCoroutine(WaitForCustomPropertiesSet());
        }
    }

    private IEnumerator WaitForCustomPropertiesSet()
    {
        float waitTime = 0f;
        const float maxWait = 8f; // 시간 연장
        
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
                else
                {
                    Debug.LogWarning($"UID 불일치 - 예상: {GameManager.Instance?.UserID}, 실제: {setUID}");
                    
                    // 불일치 시 재시도
                    if (waitTime > 3f) // 3초 후에도 불일치면 강제 재설정
                    {
                        Debug.Log("UID 불일치로 인한 강제 재설정");
                        var customProps = GameManager.Instance.GetUserCustomProperties();
                        PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
                    }
                }
            }
            
            yield return new WaitForSeconds(0.2f);
            waitTime += 0.2f;
        }
        
        Debug.LogError("커스텀 프로퍼티 설정 시간 초과");
        
        // 시간 초과시에도 일단 진행 (로그인은 성공했으니까)
        isCustomPropertiesSet = true;
        CheckLoginComplete();
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
            
            emailInput.text = "";
            passInput.text = "";
            
            StartCoroutine(CheckAllSystemsReadyAndMoveToLobby());
        }
    }

    private IEnumerator CheckAllSystemsReadyAndMoveToLobby()
    {
        // 1. Firebase Database 초기화 대기
        yield return StartCoroutine(WaitForFirebaseDatabase());
        
        // 2. GameManager 완전 준비 대기
        yield return StartCoroutine(WaitForGameManagerReady());
        
        // 3. UserManager 준비 대기 (선택사항)
        yield return StartCoroutine(WaitForUserManagerReady());
        
        // 4. 최종 상태 확인
        if (AreAllSystemsReady())
        {
            emailInput.text = "";
            passInput.text = "";
            
            yield return new WaitForSeconds(0.5f);
            SceneManager.LoadScene("LobbyScene");
        }
        else
        {
            ShowPopup("시스템 초기화에 실패했습니다. 다시 시도해주세요.");
            ResetLoginUI();
        }
    }

    private IEnumerator WaitForFirebaseDatabase()
    {
        float waitTime = 0f;
        const float maxWait = 10f;
        
        while (waitTime < maxWait)
        {
            // Firebase Database 초기화 확인
            if (FirebaseManager.IsFullyInitialized())
            {
                Debug.Log("Firebase Database 준비 완료");
                yield break;
            }
            
            // IsDatabaseReady만 확인 (Database가 실패해도 진행)
            if (FirebaseManager.IsFirebaseReady && FirebaseManager.IsAuthReady)
            {
                Debug.LogWarning("Firebase Database 실패했지만 진행 (로컬 모드)");
                yield break;
            }
            
            yield return new WaitForSeconds(0.1f);
            waitTime += 0.1f;
        }
        
        Debug.LogWarning("Firebase Database 초기화 타임아웃 - 로컬 모드로 진행");
    }

    private IEnumerator WaitForGameManagerReady()
    {
        float waitTime = 0f;
        const float maxWait = 5f;
        
        while (waitTime < maxWait)
        {
            // GameManager 완전 준비 확인
            if (GameManager.Instance != null && 
                GameManager.Instance.IsFirebaseLoggedIn && 
                GameManager.Instance.IsPhotonConnected &&
                !string.IsNullOrEmpty(GameManager.Instance.UserID))
            {
                Debug.Log("GameManager 완전 준비 완료");
                yield break;
            }
            
            yield return new WaitForSeconds(0.1f);
            waitTime += 0.1f;
        }
        
        Debug.LogError("GameManager 준비 타임아웃");
    }

    private IEnumerator WaitForUserManagerReady()
    {
        float waitTime = 0f;
        const float maxWait = 5f;
        
        while (waitTime < maxWait)
        {
            // UserManager 존재 확인 (없어도 진행)
            if (UserManager.Instance != null)
            {
                Debug.Log("UserManager 준비 완료");
                yield break;
            }
            
            yield return new WaitForSeconds(0.1f);
            waitTime += 0.1f;
        }
        
        Debug.LogWarning("UserManager 타임아웃 - 없어도 진행");
    }

    private bool AreAllSystemsReady()
    {
        // 필수 조건들
        bool gameManagerReady = GameManager.Instance != null && 
                               GameManager.Instance.IsFirebaseLoggedIn && 
                               GameManager.Instance.IsPhotonConnected &&
                               !string.IsNullOrEmpty(GameManager.Instance.UserID);
        
        bool firebaseReady = FirebaseManager.IsFirebaseReady && FirebaseManager.IsAuthReady;
        
        bool loginStatesReady = isFirebaseLoggedIn && isPhotonConnected && isCustomPropertiesSet;
        
        return gameManagerReady && firebaseReady && loginStatesReady;
    }
    
    private IEnumerator LoadLobbyScene()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("LobbyScene");
    }

    private void ResetLoginUI()
    {
        loginButton.interactable = true;
        ResetLoginStates();
    }

    #region Photon Callbacks

    public void OnConnectedToMaster()
    {
        Debug.Log("Photon 마스터 서버 연결 완료");
        isPhotonConnected = true;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPhotonConnectionStatus(true);
            
            // 연결 직후 강제 동기화
            GameManager.Instance.ForceSyncFirebasePhoton();
        }
        
        StartCoroutine(DelayedSetupCustomProperties());
    }

    private IEnumerator DelayedSetupCustomProperties()
    {
        // 조금 더 길게 대기
        yield return new WaitForSeconds(0.5f);
        SetupPhotonCustomProperties();
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"Photon 연결 해제: {cause}");
        
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
        ShowPopup("사용자 인증에 실패했습니다.");
        ResetLoginUI();
    }

    private IEnumerator ReconnectToPhoton()
    {
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
        Debug.Log("PeriodicConnectionCheck 시작");
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
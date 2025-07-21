using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }

    public string UserID { get; private set; }
    public string UserEmail { get; private set; }
    public string UserName { get; private set; }
    public bool IsLoggedIn { get; private set; }

    public bool IsFirebaseLoggedIn { get; private set; }
    public bool IsPhotonConnected { get; private set; }

    private bool isUserInfoSet = false;
    private bool isCleaningUp = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager 초기화 완료");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(SetupFirebaseAuthListener());
        
        FirebaseManager.OnUserLoggedOut += OnFirebaseUserLoggedOut;
    }

    private IEnumerator SetupFirebaseAuthListener()
    {
        Debug.Log("Firebase Auth 리스너 설정 중...");
        
        float waitTime = 0f;
        while (FirebaseManager.Auth == null && waitTime < 10f)
        {
            yield return new WaitForSeconds(0.1f);
            waitTime += 0.1f;
        }

        if (FirebaseManager.Auth != null)
        {
            FirebaseManager.Auth.StateChanged += OnFirebaseAuthStateChanged;
            Debug.Log("Firebase Auth 리스너 등록 완료");
        }
        else
        {
            Debug.LogError("Firebase Auth 초기화 실패");
        }
    }

    private void OnDestroy()
    {
        if (FirebaseManager.Auth != null)
        {
            FirebaseManager.Auth.StateChanged -= OnFirebaseAuthStateChanged;
        }
        
        FirebaseManager.OnUserLoggedOut -= OnFirebaseUserLoggedOut;
    }

    private void OnFirebaseAuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (isCleaningUp) return;
        
        FirebaseAuth auth = sender as FirebaseAuth;
        if (auth != null)
        {
            if (auth.CurrentUser != null)
            {
                SetUserInfo(auth.CurrentUser);
                
                // TokenSystem null 체크 강화
                StartCoroutine(NotifyTokenSystemAfterDelay());
            }
            else
            {
                Debug.Log("Firebase 로그아웃 감지됨");
                PerformLogoutCleanup();
            }
        }
    }
    
    private void OnFirebaseUserLoggedOut()
    {
        Debug.Log("Firebase 로그아웃 이벤트 수신");
        PerformLogoutCleanup();
    }

    public void SetUserInfo(FirebaseUser user)
    {
        if (user != null && !isCleaningUp)
        {
            UserID = user.UserId;
            UserEmail = user.Email;
            UserName = string.IsNullOrEmpty(user.DisplayName) ? "사용자" : user.DisplayName;
            IsLoggedIn = true;
            IsFirebaseLoggedIn = true;
            isUserInfoSet = true;
            
            Debug.Log($"사용자 정보 설정: {UserName} ({UserEmail})");
            Debug.Log($"Firebase UID: {UserID}");
            
            StartCoroutine(NotifyTokenSystemAfterDelay());
        }
        else if (!isCleaningUp)
        {
            Debug.LogWarning("잘못된 사용자 정보");
            ClearUserInfo();
        }
    }
    
    private IEnumerator NotifyTokenSystemAfterDelay()
    {
        float waitTime = 0f;
        while (waitTime < 5f)
        {
            // TokenSystem null 체크 강화
            try
            {
                if (TokenSystem.Instance != null && 
                    TokenSystem.Instance.gameObject != null && 
                    !TokenSystem.Instance.Equals(null))
                {
                    TokenSystem.Instance.OnUserLoggedIn();
                    Debug.Log("TokenSystem에 로그인 알림 완료");
                    yield break;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"TokenSystem 알림 실패: {e.Message}");
            }
            
            yield return new WaitForSeconds(0.1f);
            waitTime += 0.1f;
        }
        
        Debug.LogWarning("TokenSystem을 찾을 수 없습니다");
    }

    public void ClearUserInfo()
    {
        if (isCleaningUp) return;
        
        Debug.Log("사용자 정보 초기화 시작");
        
        UserID = null;
        UserEmail = null;
        UserName = null;
        IsLoggedIn = false;
        IsFirebaseLoggedIn = false;
        IsPhotonConnected = false;
        isUserInfoSet = false;
        
        Debug.Log("사용자 정보 초기화 완료");
    }
    
    private void PerformLogoutCleanup()
    {
        if (isCleaningUp) return;
        
        isCleaningUp = true;
        Debug.Log("로그아웃 정리 시작");
        
        ClearUserInfo();
        
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Photon 연결 해제 시작");
            
            // 커스텀 프로퍼티 완전 정리
            if (PhotonNetwork.LocalPlayer != null)
            {
                var emptyProps = new ExitGames.Client.Photon.Hashtable();
                emptyProps["firebaseUID"] = null;
                emptyProps["email"] = null;
                emptyProps["displayName"] = null;
                emptyProps["loginTime"] = null;
                PhotonNetwork.LocalPlayer.SetCustomProperties(emptyProps);
                Debug.Log("Photon 커스텀 프로퍼티 정리 완료");
            }
            
            PhotonNetwork.Disconnect();
        }
        
        // TokenSystem null 체크 강화
        try
        {
            if (TokenSystem.Instance != null && 
                TokenSystem.Instance.gameObject != null && 
                !TokenSystem.Instance.Equals(null))
            {
                TokenSystem.Instance.OnUserLoggedOut();
                Debug.Log("TokenSystem에 로그아웃 알림");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"TokenSystem 로그아웃 알림 실패: {e.Message}");
        }
        
        ResetGameState();
        
        Debug.Log("로그아웃 정리 완료");
        isCleaningUp = false;
    }
    
    private void ResetGameState()
    {
        Debug.Log("게임 상태 리셋");
        
        PlayerPrefs.DeleteKey("TempGameData");
        PlayerPrefs.DeleteKey("CurrentSession");
        PlayerPrefs.Save();
    }

    public void SetPhotonConnectionStatus(bool connected)
    {
        bool wasConnected = IsPhotonConnected;
        IsPhotonConnected = connected;
        
        if (wasConnected != connected)
        {
            Debug.Log($"Photon 연결 상태: {(connected ? "연결됨" : "연결 해제됨")}");
            
            if (connected && !isCleaningUp)
            {
                CheckFirebasePhotonSync();
            }
        }
    }

    private void CheckFirebasePhotonSync()
    {
        if (!IsLoggedIn || isCleaningUp)
        {
            Debug.LogWarning("Firebase 로그인 상태가 아니거나 정리 중");
            return;
        }

        bool allSynced = true;
        bool needsForceSync = false;

        if (PhotonNetwork.AuthValues != null)
        {
            string photonUserId = PhotonNetwork.AuthValues.UserId;
            if (photonUserId == UserID)
            {
                Debug.Log("AuthValues 동기화 완료");
            }
            else
            {
                Debug.LogError($"AuthValues 불일치 - Firebase: {UserID}, Photon: {photonUserId}");
                allSynced = false;
                needsForceSync = true;
            }
        }
        else
        {
            Debug.LogWarning("PhotonNetwork.AuthValues가 null");
            allSynced = false;
            needsForceSync = true;
        }

        if (PhotonNetwork.LocalPlayer != null)
        {
            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("firebaseUID"))
            {
                string customUID = PhotonNetwork.LocalPlayer.CustomProperties["firebaseUID"]?.ToString();
                if (customUID == UserID)
                {
                    Debug.Log("커스텀 프로퍼티 동기화 완료");
                }
                else
                {
                    Debug.LogError($"커스텀 프로퍼티 불일치 - Firebase: {UserID}, CustomProp: {customUID}");
                    allSynced = false;
                    needsForceSync = true;
                }
            }
            else
            {
                Debug.LogWarning("커스텀 프로퍼티에 firebaseUID 없음");
                allSynced = false;
                needsForceSync = true;
            }
        }
        else
        {
            Debug.LogWarning("PhotonNetwork.LocalPlayer가 null");
            allSynced = false;
        }

        // 불일치 발견 시 자동 강제 동기화
        if (needsForceSync && PhotonNetwork.IsConnected)
        {
            Debug.Log("동기화 불일치 감지 - 강제 동기화 시작");
            StartCoroutine(ForcePhotonSync());
        }

        Debug.Log($"전체 동기화 상태: {(allSynced ? "성공" : "실패")}");
    }

    private IEnumerator ForcePhotonSync()
    {
        yield return new WaitForSeconds(0.5f);
        
        if (!isCleaningUp && PhotonNetwork.IsConnected && !string.IsNullOrEmpty(UserID))
        {
            Debug.Log("강제 동기화 실행");
            
            // AuthValues 강제 업데이트
            PhotonNetwork.AuthValues = new AuthenticationValues(UserID);
            PhotonNetwork.NickName = GetPhotonPlayerName();
            
            // 커스텀 프로퍼티 강제 업데이트
            var customProps = GetUserCustomProperties();
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
            
            Debug.Log($"강제 동기화 완료 - UID: {UserID}");
            
            // 1초 후 재확인
            yield return new WaitForSeconds(1f);
            CheckFirebasePhotonSync();
        }
    }

    public string GetPhotonPlayerName()
    {
        if (!isUserInfoSet || isCleaningUp)
        {
            Debug.LogWarning("사용자 정보가 설정되지 않음");
            return "Guest";
        }
        
        return string.IsNullOrEmpty(UserName) ? "Guest" : UserName;
    }

    public ExitGames.Client.Photon.Hashtable GetUserCustomProperties()
    {
        if (!isUserInfoSet || isCleaningUp)
        {
            Debug.LogError("사용자 정보가 설정되지 않은 상태에서 커스텀 프로퍼티 요청");
            return new ExitGames.Client.Photon.Hashtable();
        }

        var props = new ExitGames.Client.Photon.Hashtable();
        props["firebaseUID"] = UserID;
        props["email"] = UserEmail;
        props["displayName"] = UserName;
        props["loginTime"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        return props;
    }

    public string GetPlayerFirebaseUID(Player player)
    {
        if (player?.CustomProperties != null && player.CustomProperties.TryGetValue("firebaseUID", out object uid))
        {
            return uid.ToString();
        }
        
        Debug.LogWarning($"플레이어 {player?.NickName}의 Firebase UID 없음");
        return null;
    }

    public bool IsFullyLoggedIn()
    {
        return IsFirebaseLoggedIn && IsPhotonConnected && !string.IsNullOrEmpty(UserID) && isUserInfoSet && !isCleaningUp;
    }

    public string GenerateUserToken()
    {
        if (string.IsNullOrEmpty(UserID) || isCleaningUp)
        {
            Debug.LogWarning("UserID가 없거나 정리 중이어서 토큰 생성 불가");
            return null;
        }
        
        string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        return $"{UserID}_{timestamp}";
    }

    public void ForceSyncFirebasePhoton()
    {
        if (isCleaningUp)
        {
            Debug.LogWarning("정리 중이어서 동기화를 수행할 수 없습니다");
            return;
        }
        
        Debug.Log("Firebase-Photon 강제 동기화 시작");
        
        if (!IsLoggedIn)
        {
            Debug.LogError("Firebase 로그인 상태가 아님");
            return;
        }

        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogError("Photon 연결 상태가 아님");
            return;
        }

        PhotonNetwork.AuthValues = new AuthenticationValues(UserID);
        PhotonNetwork.NickName = GetPhotonPlayerName();
        
        var customProps = GetUserCustomProperties();
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
        
        Debug.Log("강제 동기화 완료");
        StartCoroutine(CheckSyncResult());
    }

    private IEnumerator CheckSyncResult()
    {
        yield return new WaitForSeconds(1f);
        if (!isCleaningUp)
        {
            CheckFirebasePhotonSync();
        }
    }
    
    public void UpdateUserName(string newUserName)
    {
        if (!string.IsNullOrEmpty(newUserName) && !isCleaningUp)
        {
            UserName = newUserName;
        
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.NickName = newUserName;
            }
        
            Debug.Log($"사용자 이름 업데이트: {newUserName}");
        }
    }
    
    public void DestroyManager()
    {
        Debug.Log("매니저 완전 제거 시작");
        
        if (!isCleaningUp)
        {
            PerformLogoutCleanup();
        }
        
        if (instance == this)
        {
            instance = null;
        }
        
        Destroy(gameObject);
        Debug.Log("매니저 완전 제거 완료");
    }
}
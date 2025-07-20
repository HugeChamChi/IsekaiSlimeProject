using Firebase.Auth;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }

    // 사용자 정보
    public string UserID { get; private set; }
    public string UserEmail { get; private set; }
    public string UserName { get; private set; }
    public bool IsLoggedIn { get; private set; }

    // 연결 상태
    public bool IsFirebaseLoggedIn { get; private set; }
    public bool IsPhotonConnected { get; private set; }

    private bool isUserInfoSet = false;

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
    }

    private void OnFirebaseAuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        FirebaseAuth auth = sender as FirebaseAuth;
        if (auth != null)
        {
            if (auth.CurrentUser != null)
            {
                SetUserInfo(auth.CurrentUser);
            }
            else
            {
                ClearUserInfo();
            }
        }
    }

    public void SetUserInfo(FirebaseUser user)
    {
        if (user != null)
        {
            UserID = user.UserId;
            UserEmail = user.Email;
            UserName = string.IsNullOrEmpty(user.DisplayName) ? "사용자" : user.DisplayName;
            IsLoggedIn = true;
            IsFirebaseLoggedIn = true;
            isUserInfoSet = true;
            
            Debug.Log($"사용자 정보 설정: {UserName} ({UserEmail})");
            Debug.Log($"Firebase UID: {UserID}");
        }
        else
        {
            Debug.LogWarning("잘못된 사용자 정보");
            ClearUserInfo();
        }
    }

    public void ClearUserInfo()
    {
        Debug.Log("사용자 정보 초기화");
        
        UserID = null;
        UserEmail = null;
        UserName = null;
        IsLoggedIn = false;
        IsFirebaseLoggedIn = false;
        IsPhotonConnected = false;
        isUserInfoSet = false;
    }

    public void SetPhotonConnectionStatus(bool connected)
    {
        bool wasConnected = IsPhotonConnected;
        IsPhotonConnected = connected;
        
        if (wasConnected != connected)
        {
            Debug.Log($"Photon 연결 상태: {(connected ? "연결됨" : "연결 해제됨")}");
            
            if (connected)
            {
                CheckFirebasePhotonSync();
            }
        }
    }

    // Firebase와 Photon UID 동기화 확인
    private void CheckFirebasePhotonSync()
    {
        if (!IsLoggedIn)
        {
            Debug.LogWarning("Firebase 로그인 상태가 아님");
            return;
        }

        bool allSynced = true;

        // AuthValues 동기화 확인
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
            }
        }
        else
        {
            Debug.LogWarning("PhotonNetwork.AuthValues가 null");
            allSynced = false;
        }

        // 커스텀 프로퍼티 동기화 확인
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
                }
            }
            else
            {
                Debug.LogWarning("커스텀 프로퍼티에 firebaseUID 없음");
                allSynced = false;
            }
        }
        else
        {
            Debug.LogWarning("PhotonNetwork.LocalPlayer가 null");
            allSynced = false;
        }

        Debug.Log($"전체 동기화 상태: {(allSynced ? "성공" : "실패")}");
    }

    public string GetPhotonPlayerName()
    {
        if (!isUserInfoSet)
        {
            Debug.LogWarning("사용자 정보가 설정되지 않음");
            return "Guest";
        }
        
        return string.IsNullOrEmpty(UserName) ? "Guest" : UserName;
    }

    public ExitGames.Client.Photon.Hashtable GetUserCustomProperties()
    {
        if (!isUserInfoSet)
        {
            Debug.LogError("사용자 정보가 설정되지 않은 상태에서 커스텀 프로퍼티 요청");
            return new ExitGames.Client.Photon.Hashtable();
        }

        var props = new ExitGames.Client.Photon.Hashtable();
        props["firebaseUID"] = UserID;
        props["email"] = UserEmail;
        props["displayName"] = UserName;
        props["loginTime"] = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        return props;
    }

    public string GetPlayerFirebaseUID(Photon.Realtime.Player player)
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
        return IsFirebaseLoggedIn && IsPhotonConnected && !string.IsNullOrEmpty(UserID) && isUserInfoSet;
    }

    public string GenerateUserToken()
    {
        if (string.IsNullOrEmpty(UserID))
        {
            Debug.LogWarning("UserID가 없어서 토큰 생성 불가");
            return null;
        }
        
        string timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        return $"{UserID}_{timestamp}";
    }

    // Firebase-Photon 동기화 강제 실행
    public void ForceSyncFirebasePhoton()
    {
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

        // AuthValues 재설정
        PhotonNetwork.AuthValues = new AuthenticationValues(UserID);
        PhotonNetwork.NickName = GetPhotonPlayerName();
        
        // 커스텀 프로퍼티 재설정
        var customProps = GetUserCustomProperties();
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
        
        Debug.Log("강제 동기화 완료");
        
        // 동기화 결과 확인
        StartCoroutine(CheckSyncResult());
    }

    private IEnumerator CheckSyncResult()
    {
        yield return new WaitForSeconds(1f);
        CheckFirebasePhotonSync();
    }
}
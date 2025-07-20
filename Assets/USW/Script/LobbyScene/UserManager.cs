using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class UserManager : MonoBehaviour
{
    private static UserManager instance;
    public static UserManager Instance { get { return instance; } }
    
    private DatabaseReference userRef;
    private bool isFirebaseEnabled = false;
    private bool isInitialized = false;
    
    private string currentUID = "";
    private string currentEmail = "";
    private string currentDisplayName = "";
    
    public static event Action<string> OnDisplayNameChanged;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("UserManager 초기화 완료");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        StartCoroutine(WaitForInitialization());
    }
    
    private IEnumerator WaitForInitialization()
    {
        Debug.Log("UserManager 초기화 대기 중...");
        
        while (GameManager.Instance == null || 
               !FirebaseManager.IsFullyInitialized() ||
               !GameManager.Instance.IsFirebaseLoggedIn)
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        Debug.Log("의존성 준비 완료 - UserManager 초기화 시작");
        InitializeUserManager();
    }
    
    private void InitializeUserManager()
    {
        if (GameManager.Instance.IsFirebaseLoggedIn && FirebaseManager.Database != null)
        {
            userRef = FirebaseManager.GetUserRef(GameManager.Instance.UserID);
            
            if (userRef != null)
            {
                isFirebaseEnabled = true;
                Debug.Log("Firebase 사용자 관리 시스템 활성화");
            }
            else
            {
                Debug.LogWarning("Firebase UserRef 생성 실패");
            }
        }
        else
        {
            Debug.LogWarning("Firebase 미연결");
        }
        
        UpdateCurrentUserInfo();
        isInitialized = true;
    }
    
    private void UpdateCurrentUserInfo()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsLoggedIn)
        {
            currentUID = GameManager.Instance.UserID;
            currentEmail = GameManager.Instance.UserEmail;
            currentDisplayName = GameManager.Instance.UserName;
            
            Debug.Log($"사용자 정보 업데이트: {currentDisplayName} ({GetUID()})");
        }
    }
    
    #region 공개 메서드
    
    public string GetUID()
    {
        return currentUID;
    }
    
    public string GetDisplayName()
    {
        return string.IsNullOrEmpty(currentDisplayName) ? "Guest" : currentDisplayName;
    }
    
    public string GetEmail()
    {
        return currentEmail;
    }
    
    public bool IsReady()
    {
        return isInitialized;
    }
    
    #endregion
    
    #region 닉네임 변경
    
    public void UpdateDisplayName(string newDisplayName, System.Action<bool> onComplete = null)
    {
        if (!isInitialized || string.IsNullOrEmpty(newDisplayName))
        {
            Debug.LogWarning("UserManager가 초기화되지 않았거나 잘못된 닉네임");
            onComplete?.Invoke(false);
            return;
        }
        
        if (newDisplayName.Length < 2 || newDisplayName.Length > 10)
        {
            Debug.LogWarning("닉네임은 2~10자여야 합니다");
            onComplete?.Invoke(false);
            return;
        }
        
        StartCoroutine(UpdateDisplayNameProcess(newDisplayName, onComplete));
    }
    
    private IEnumerator UpdateDisplayNameProcess(string newDisplayName, System.Action<bool> onComplete)
    {
        bool success = true;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateUserName(newDisplayName);
        }
        
        if (isFirebaseEnabled)
        {
            bool authUpdateComplete = false;
            bool authUpdateSuccess = false;
            
            FirebaseManager.UpdateUserProfile(newDisplayName, (result) =>
            {
                authUpdateSuccess = result;
                authUpdateComplete = true;
            });
            
            while (!authUpdateComplete)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            if (!authUpdateSuccess)
            {
                Debug.LogWarning("Firebase Auth 프로필 업데이트 실패");
                success = false;
            }
            
            if (authUpdateSuccess && userRef != null)
            {
                var profileData = new System.Collections.Generic.Dictionary<string, object>
                {
                    ["displayName"] = newDisplayName,
                    ["lastUpdated"] = ServerValue.Timestamp
                };
                
                bool dbUpdateComplete = false;
                
                userRef.Child("profile").UpdateChildrenAsync(profileData).ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted)
                    {
                        Debug.LogWarning("Firebase Database 프로필 업데이트 실패");
                    }
                    dbUpdateComplete = true;
                });
                
                float waitTime = 0f;
                while (!dbUpdateComplete && waitTime < 3f)
                {
                    yield return new WaitForSeconds(0.1f);
                    waitTime += 0.1f;
                }
            }
        }
        
        if (success)
        {
            currentDisplayName = newDisplayName;
            OnDisplayNameChanged?.Invoke(newDisplayName);
            Debug.Log($"닉네임 변경 완료: {newDisplayName}");
        }
        
        onComplete?.Invoke(success);
    }
    
    #endregion
    
    #region 회원탈퇴
    
    public void DeleteAccount(System.Action<bool> onComplete = null)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("UserManager가 초기화되지 않음");
            onComplete?.Invoke(false);
            return;
        }
        
        StartCoroutine(DeleteAccountProcess(onComplete));
    }
    
    private IEnumerator DeleteAccountProcess(System.Action<bool> onComplete)
    {
        Debug.Log("계정 탈퇴 처리 시작...");
        
        bool success = true;
        
        if (isFirebaseEnabled && userRef != null)
        {
            bool dataDeleteComplete = false;
            
            userRef.RemoveValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Firebase 사용자 데이터 삭제 실패");
                    success = false;
                }
                else
                {
                    Debug.Log("Firebase 사용자 데이터 삭제 완료");
                }
                dataDeleteComplete = true;
            });
            
            while (!dataDeleteComplete)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        if (isFirebaseEnabled)
        {
            bool authDeleteComplete = false;
            
            FirebaseManager.DeleteCurrentUserAccount((result) =>
            {
                if (!result)
                {
                    Debug.LogError("Firebase Auth 계정 삭제 실패");
                    success = false;
                }
                else
                {
                    Debug.Log("Firebase Auth 계정 삭제 완료");
                }
                authDeleteComplete = true;
            });
            
            while (!authDeleteComplete)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("로컬 데이터 삭제 완료");
        
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearUserInfo();
        }
        
        Debug.Log($"계정 탈퇴 처리 완료: {(success ? "성공" : "부분 실패")}");
        
        onComplete?.Invoke(success);
        
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("LoginScene");
    }
    
    #endregion
    
    #region 유틸리티
    
    public void RefreshUserInfo()
    {
        UpdateCurrentUserInfo();
    }
    
    [ContextMenu("사용자 정보 출력")]
    public void PrintUserInfo()
    {
        Debug.Log($"=== 사용자 정보 ===");
        Debug.Log($"UID: {GetUID()}");
        Debug.Log($"Email: {GetEmail()}");
        Debug.Log($"Display Name: {GetDisplayName()}");
        Debug.Log($"Firebase Enabled: {isFirebaseEnabled}");
        Debug.Log($"Initialized: {isInitialized}");
    }
    
    #endregion
}
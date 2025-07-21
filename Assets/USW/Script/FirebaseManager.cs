using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine.SceneManagement;

public class FirebaseManager : MonoBehaviour
{
    private static FirebaseManager instance;
    public static FirebaseManager Instance { get { return instance; } }

    private static FirebaseApp app;
    public static FirebaseApp App { get { return app; } }

    private static FirebaseAuth auth;
    public static FirebaseAuth Auth { get { return auth; } }
    
    private static FirebaseDatabase database;
    public static FirebaseDatabase Database { get { return database; } }
    
    public static bool IsFirebaseReady { get; private set; } = false;
    public static bool IsAuthReady { get; private set; } = false;
    public static bool IsDatabaseReady { get; private set; } = false;
    
    public static event Action OnFirebaseInitialized;
    public static event Action OnAllSystemsReady;
    public static event Action<FirebaseUser> OnUserDataChanged;
    public static event Action OnUserLoggedOut;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("FirebaseManager 인스턴스 생성");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(InitializeFirebase());
    }
    
    private IEnumerator InitializeFirebase()
    {
        Debug.Log("Firebase 초기화 시작...");
        
        var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => dependencyTask.IsCompleted);
        
        DependencyStatus dependencyStatus = dependencyTask.Result;
        if (dependencyStatus == DependencyStatus.Available)
        {
            app = FirebaseApp.DefaultInstance;
            IsFirebaseReady = true;
            Debug.Log("Firebase App 초기화 완료");
            
            InitializeAuth();
            InitializeDatabase();
            
            OnFirebaseInitialized?.Invoke();
            StartCoroutine(CheckAllSystemsReady());
        }
        else
        {
            Debug.LogError($"Firebase 의존성 오류: {dependencyStatus}");
            app = null;
            auth = null;
            database = null;
        }
    }
    
    private void InitializeAuth()
    {
        try
        {
            auth = FirebaseAuth.DefaultInstance;
            IsAuthReady = true;
            Debug.Log("Firebase Auth 초기화 완료");
            
            if (auth != null)
            {
                auth.StateChanged += OnAuthStateChanged;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Firebase Auth 초기화 실패: {e.Message}");
            IsAuthReady = false;
        }
    }
    
    private void InitializeDatabase()
    {
        try
        {
            string databaseURL = "https://isekaislimeproject-default-rtdb.asia-southeast1.firebasedatabase.app/";
            Debug.Log($"Firebase Database URL 설정: {databaseURL}");
        
            if (app != null)
            {
                database = FirebaseDatabase.GetInstance(app, databaseURL);
            }
            else
            {
                Debug.LogError("Firebase App이 null입니다.");
                IsDatabaseReady = false;
                return;
            }
        
            if (database != null)
            {
                try
                {
                    #if UNITY_EDITOR
                    database.SetPersistenceEnabled(false);
                    Debug.Log("에디터");
                    #else
                    database.SetPersistenceEnabled(true);
                    Debug.Log("활성화")
                    #endif
                }
                catch
                {
                    
                }
            
                IsDatabaseReady = true;
                Debug.Log("Firebase Database 초기화 완료");
                StartCoroutine(MonitorDatabaseConnection());
            }
            else
            {
                Debug.LogError("Firebase Database 인스턴스 생성 실패");
                IsDatabaseReady = false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Firebase Database 초기화 실패: {e.Message}");
            IsDatabaseReady = false;
        }
    }

    private void OnAuthStateChanged(object sender, EventArgs eventArgs)
    {
        FirebaseAuth authSender = sender as FirebaseAuth;
        if (authSender != null)
        {
            FirebaseUser user = authSender.CurrentUser;
            OnUserDataChanged?.Invoke(user);

            if (user != null)
            {
                Debug.Log($"사용자 로그인: {user.Email}");
                NotifyTokenSystemLogin();
            }
            else
            {
                Debug.Log("사용자 로그아웃");
                OnUserLoggedOut?.Invoke();
                NotifyTokenSystemLogout();
            }
        }
    }
    
    private void NotifyTokenSystemLogin()
    {
        if (TokenSystem.Instance != null)
        {
            TokenSystem.Instance.OnUserLoggedIn();
        }
    }
    
    private void NotifyTokenSystemLogout()
    {
        if (TokenSystem.Instance != null)
        {
            TokenSystem.Instance.OnUserLoggedOut();
        }
    }

    private IEnumerator CheckAllSystemsReady()
    {
        while (!IsFirebaseReady || !IsAuthReady || !IsDatabaseReady)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.Log("모든 Firebase 시스템 준비 완료!");
        OnAllSystemsReady?.Invoke();
    }
    
    private IEnumerator MonitorDatabaseConnection()
    {
        if (database == null) yield break;
        
        var connectedRef = database.GetReference(".info/connected");
        
        connectedRef.ValueChanged += (sender, args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError($"Database 연결 오류: {args.DatabaseError.Message}");
                return;
            }
            
            bool connected = (bool)args.Snapshot.Value;
            Debug.Log($"Database 연결 상태: {(connected ? "연결됨" : "연결 해제됨")}");
        };
    }
    
    #region 계정 관리 메서드
    
    public static void UpdateUserProfile(string displayName, Action<bool> onComplete = null)
    {
        if (auth?.CurrentUser == null)
        {
            Debug.LogError("로그인된 사용자가 없습니다.");
            onComplete?.Invoke(false);
            return;
        }
        
        UserProfile profile = new UserProfile
        {
            DisplayName = displayName
        };
        
        auth.CurrentUser.UpdateUserProfileAsync(profile).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log($"사용자 프로필 업데이트 완료: {displayName}");
                onComplete?.Invoke(true);
            }
            else
            {
                Debug.LogError($"사용자 프로필 업데이트 실패: {task.Exception?.GetBaseException()?.Message}");
                onComplete?.Invoke(false);
            }
        });
    }
    
    public static void DeleteCurrentUserAccount(Action<bool> onComplete = null)
    {
        if (auth?.CurrentUser == null)
        {
            Debug.LogError("로그인된 사용자가 없습니다.");
            onComplete?.Invoke(false);
            return;
        }
        
        string userID = auth.CurrentUser.UserId;
        Debug.Log($"계정 삭제 시작 - UserID: {userID}");
        
        var userRef = GetUserRef(userID);
        if (userRef != null)
        {
            userRef.RemoveValueAsync().ContinueWithOnMainThread(dataTask =>
            {
                if (dataTask.IsCompletedSuccessfully)
                {
                    Debug.Log("사용자 데이터 삭제 완료");
                    
                    auth.CurrentUser.DeleteAsync().ContinueWithOnMainThread(authTask =>
                    {
                        if (authTask.IsCompletedSuccessfully)
                        {
                            Debug.Log("계정 삭제 완료");
                            PerformPostDeletionCleanup();
                            onComplete?.Invoke(true);
                        }
                        else
                        {
                            Debug.LogError($"계정 삭제 실패: {authTask.Exception?.GetBaseException()?.Message}");
                            HandleAccountDeletionError(authTask.Exception, onComplete);
                        }
                    });
                }
                else
                {
                    Debug.LogError($"사용자 데이터 삭제 실패: {dataTask.Exception?.GetBaseException()?.Message}");
                    onComplete?.Invoke(false);
                }
            });
        }
        else
        {
            Debug.LogError("사용자 참조를 가져올 수 없습니다.");
            onComplete?.Invoke(false);
        }
    }
    
    private static void HandleAccountDeletionError(Exception exception, Action<bool> onComplete)
    {
        string errorMessage = exception?.GetBaseException()?.Message ?? "알 수 없는 오류";
        
        if (errorMessage.Contains("requires-recent-login"))
        {
            Debug.LogWarning("계정 삭제를 위해 재인증이 필요합니다.");
        }
        else
        {
            Debug.LogError($"계정 삭제 중 예상치 못한 오류: {errorMessage}");
        }
        
        onComplete?.Invoke(false);
    }
    
    private static void PerformPostDeletionCleanup()
    {
        Debug.Log("계정 삭제 후 정리 시작");
        
        PlayerPrefs.DeleteKey("LastLoginEmail");
        PlayerPrefs.DeleteKey("AutoLogin");
        PlayerPrefs.Save();
        
        Debug.Log("계정 삭제 후 정리 완료");
    }
    
    public static void SendPasswordResetEmail(string email, Action<bool> onComplete = null)
    {
        if (auth == null)
        {
            Debug.LogError("Firebase Auth가 초기화되지 않았습니다.");
            onComplete?.Invoke(false);
            return;
        }
        
        auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log($"비밀번호 재설정 이메일 발송 완료: {email}");
                onComplete?.Invoke(true);
            }
            else
            {
                Debug.LogError($"비밀번호 재설정 이메일 발송 실패: {task.Exception?.GetBaseException()?.Message}");
                onComplete?.Invoke(false);
            }
        });
    }
    
    public static void SignOut()
    {
        Debug.Log("로그아웃 시작");
        
        if (auth != null)
        {
            auth.SignOut();
            Debug.Log("Firebase Auth 로그아웃 완료");
        }
        else
        {
            Debug.LogWarning("Firebase Auth가 초기화되지 않았습니다.");
        }
        
        PerformLogoutCleanup();
    }
    
    public static void SignOutWithSceneTransition(string targetScene = "LoginScene", Action onComplete = null)
    {
        Debug.Log($"Scene 전환을 포함한 로그아웃 시작 - 대상: {targetScene}");
        
        SignOut();
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearUserInfo();
            Debug.Log("GameManager 정리 완료");
        }
        
        if (Instance != null)
        {
            Instance.StartCoroutine(DelayedSceneTransition(targetScene, onComplete));
        }
        else
        {
            SceneManager.LoadScene(targetScene);
            onComplete?.Invoke();
        }
    }
    
    private static IEnumerator DelayedSceneTransition(string targetScene, Action onComplete)
    {
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log($"{targetScene}으로 Scene 전환 시작");
        SceneManager.LoadScene(targetScene);
        
        onComplete?.Invoke();
    }
    
    private static void PerformLogoutCleanup()
    {
        Debug.Log("로그아웃 정리 시작");
        
        PlayerPrefs.DeleteKey("AutoLogin");
        PlayerPrefs.Save();
        
        Debug.Log("로그아웃 정리 완료");
    }
    
    #endregion
    
    #region 데이터베이스 참조 메서드
    
    public static DatabaseReference GetUserRef(string userID)
    {
        if (database == null || string.IsNullOrEmpty(userID))
        {
            Debug.LogWarning("Database가 초기화되지 않았거나 유효하지 않은 UserID");
            return null;
        }
        
        return database.GetReference("users").Child(userID);
    }
    
    public static DatabaseReference GetGlobalSettingsRef()
    {
        if (database == null)
        {
            Debug.LogWarning("Database가 초기화되지 않음");
            return null;
        }
        
        return database.GetReference("globalSettings");
    }
    
    #endregion
    
    #region 상태 확인 메서드
    
    public static bool IsFullyInitialized()
    {
        return IsFirebaseReady && IsAuthReady && IsDatabaseReady;
    }
    
    public static bool IsUserLoggedIn()
    {
        return auth?.CurrentUser != null;
    }
    
    public static FirebaseUser GetCurrentUser()
    {
        return auth?.CurrentUser;
    }
    
    public static string GetCurrentUserID()
    {
        return auth?.CurrentUser?.UserId;
    }
    
    public static string GetCurrentUserEmail()
    {
        return auth?.CurrentUser?.Email;
    }
    
    public static string GetCurrentUserDisplayName()
    {
        return auth?.CurrentUser?.DisplayName ?? "Guest";
    }
    
    #endregion
    
    #region 유틸리티 메서드
    
    public static void CheckNetworkConnection(Action<bool> onResult)
    {
        if (database == null)
        {
            onResult?.Invoke(false);
            return;
        }
        
        var connectedRef = database.GetReference(".info/connected");
        connectedRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                bool connected = (bool)task.Result.Value;
                onResult?.Invoke(connected);
            }
            else
            {
                onResult?.Invoke(false);
            }
        });
    }
    
    public static void GetServerTimestamp(Action<long> onResult)
    {
        if (database == null)
        {
            onResult?.Invoke(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            return;
        }
        
        var timestampRef = database.GetReference(".info/serverTimeOffset");
        timestampRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                long offset = Convert.ToInt64(task.Result.Value ?? 0);
                long serverTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + offset;
                onResult?.Invoke(serverTime);
            }
            else
            {
                onResult?.Invoke(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
        });
    }
    
    #endregion
    
    private void OnDestroy()
    {
        if (auth != null)
        {
            auth.StateChanged -= OnAuthStateChanged;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;

public class TokenSystem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image tokenFillImage;
    [SerializeField] private TextMeshProUGUI tokenCountText;
    [SerializeField] private TextMeshProUGUI regenTimeText;
    [SerializeField] private Button testButton;
    
    [Header("Token Settings")]
    [SerializeField] private int maxTokens = 100;
    [SerializeField] private int tokenRegenRate = 1;
    [SerializeField] private float regenIntervalMinutes = 10f;
    
    private static TokenSystem instance;
    public static TokenSystem Instance { get { return instance; } }
    
    private DatabaseReference userTokenRef;
    private DatabaseReference tokenRequestRef;
    private DatabaseReference globalSettingsRef;
    
    private bool isInitialized = false;
    private int currentTokens = 100;
    private long lastServerUpdateTime = 0;
    private bool isProcessingRequest = false;
    
    private Coroutine regenTimerCoroutine; // 코루틴 참조 저장용
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        SetupTestButton();
        StartCoroutine(InitializeSecureTokenSystem());
    }
    
    private void SetupTestButton()
    {
        if (testButton != null)
        {
            testButton.onClick.RemoveAllListeners();
            testButton.onClick.AddListener(() => RequestUseTokens(10));
        }
    }
    
    private IEnumerator InitializeSecureTokenSystem()
    {
        Debug.Log("보안 토큰 시스템 초기화 시작");
        
        // Firebase 준비 대기
        while (!FirebaseManager.IsFullyInitialized())
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        // 사용자 로그인 대기
        while (!FirebaseManager.IsUserLoggedIn())
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        string userID = FirebaseManager.GetCurrentUserID();
        if (!string.IsNullOrEmpty(userID))
        {
            // 참조 설정
            userTokenRef = FirebaseManager.Database.GetReference($"users/{userID}/tokens");
            tokenRequestRef = FirebaseManager.Database.GetReference($"users/{userID}/tokenRequests");
            globalSettingsRef = FirebaseManager.Database.GetReference("globalSettings/tokenSettings");
            
            // 글로벌 설정 로드
            yield return StartCoroutine(LoadGlobalSettings());
            
            // 토큰 데이터 로드
            LoadSecureTokenData();
            
            // 토큰 요청 처리 리스너 설정
            SetupTokenRequestProcessor();
        }
        
        isInitialized = true;
        Debug.Log("보안 토큰 시스템 초기화 완료");
    }
    
    private IEnumerator LoadGlobalSettings()
    {
        bool settingsLoaded = false;
        
        globalSettingsRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully && task.Result.Exists)
            {
                var settings = task.Result.Value as IDictionary;
                if (settings != null)
                {
                    if (settings.Contains("maxTokens"))
                        maxTokens = Convert.ToInt32(settings["maxTokens"]);
                    
                    if (settings.Contains("regenIntervalMinutes"))
                        regenIntervalMinutes = Convert.ToSingle(settings["regenIntervalMinutes"]);
                    
                    if (settings.Contains("regenRate"))
                        tokenRegenRate = Convert.ToInt32(settings["regenRate"]);
                }
            }
            settingsLoaded = true;
        });
        
        while (!settingsLoaded)
        {
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    private void LoadSecureTokenData()
    {
        if (userTokenRef == null) return;
        
        userTokenRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                var snapshot = task.Result;
                if (snapshot.Exists)
                {
                    var data = snapshot.Value as IDictionary;
                    if (data != null)
                    {
                        if (data.Contains("currentTokens"))
                            currentTokens = Convert.ToInt32(data["currentTokens"]);
                        
                        if (data.Contains("lastUpdateTime"))
                            lastServerUpdateTime = Convert.ToInt64(data["lastUpdateTime"]);
                        
                        UpdateUI();
                        
                        // 서버에 업데이트 요청 (오프라인 회복 계산은 서버에서)
                        RequestTokenUpdate();
                    }
                }
                else
                {
                    CreateSecureInitialData();
                }
            }
            else
            {
                Debug.LogError("토큰 데이터 로드 실패");
                CreateSecureInitialData();
            }
        });
        
        // 토큰 데이터 변화 감지
        userTokenRef.ValueChanged += OnTokenDataChanged;
    }
    
    private void OnTokenDataChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("토큰 데이터 변화 감지 오류: " + args.DatabaseError.Message);
            return;
        }
        
        var data = args.Snapshot.Value as IDictionary;
        if (data != null)
        {
            int newTokens = data.Contains("currentTokens") ? Convert.ToInt32(data["currentTokens"]) : currentTokens;
            
            if (newTokens != currentTokens)
            {
                currentTokens = newTokens;
                UpdateUI();
            }
            
            if (data.Contains("lastUpdateTime"))
                lastServerUpdateTime = Convert.ToInt64(data["lastUpdateTime"]);
        }
    }
    
    private void CreateSecureInitialData()
    {
        FirebaseManager.GetServerTimestamp((serverTime) =>
        {
            var requestData = new Dictionary<string, object>
            {
                ["updateRequest"] = serverTime,
                ["initialSetup"] = true
            };
            
            tokenRequestRef.SetValueAsync(requestData);
        });
    }
    
    private void SetupTokenRequestProcessor()
    {
        // 토큰 요청 처리 (이 부분이 "서버" 역할)
        tokenRequestRef.ValueChanged += ProcessTokenRequests;
    }
    
    private void ProcessTokenRequests(object sender, ValueChangedEventArgs args)
    {
        if (isProcessingRequest) return;
        if (args.DatabaseError != null) return;
        if (!args.Snapshot.Exists) return;
        
        isProcessingRequest = true;
        
        var requestData = args.Snapshot.Value as IDictionary;
        if (requestData == null)
        {
            isProcessingRequest = false;
            return;
        }
        
        FirebaseManager.GetServerTimestamp((serverTime) =>
        {
            ProcessServerSideTokenLogic(requestData, serverTime);
        });
    }
    
    private void ProcessServerSideTokenLogic(IDictionary requestData, long serverTime)
    {
        // 현재 토큰 데이터 가져오기
        userTokenRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsCompletedSuccessfully)
            {
                isProcessingRequest = false;
                return;
            }
            
            var currentData = task.Result.Value as IDictionary;
            if (currentData == null)
            {
                isProcessingRequest = false;
                return;
            }
            
            int tokens = currentData.Contains("currentTokens") ? Convert.ToInt32(currentData["currentTokens"]) : maxTokens;
            long lastRegenTime = currentData.Contains("lastRegenTime") ? Convert.ToInt64(currentData["lastRegenTime"]) : serverTime;
            
            // 서버 시간 기반 토큰 회복 계산
            long timeDiff = serverTime - lastRegenTime;
            double minutesPassed = timeDiff / (1000.0 * 60.0); // 밀리초를 분으로 변환
            int tokensToAdd = (int)(minutesPassed / regenIntervalMinutes) * tokenRegenRate;
            
            if (tokensToAdd > 0)
            {
                tokens = Mathf.Min(tokens + tokensToAdd, maxTokens);
                lastRegenTime = serverTime;
            }
            
            // 토큰 사용 요청 처리
            if (requestData.Contains("useTokenRequest"))
            {
                int tokensToUse = Convert.ToInt32(requestData["useTokenRequest"]);
                if (tokens >= tokensToUse)
                {
                    tokens -= tokensToUse;
                    Debug.Log($"토큰 사용: -{tokensToUse} (남은 {tokens})");
                }
                else
                {
                    Debug.Log($"토큰 부족: 필요={tokensToUse}, 보유={tokens}");
                }
            }
            
            // 업데이트된 토큰 데이터 저장
            var updatedData = new Dictionary<string, object>
            {
                ["currentTokens"] = tokens,
                ["maxTokens"] = maxTokens,
                ["lastUpdateTime"] = serverTime,
                ["lastRegenTime"] = lastRegenTime
            };
            
            userTokenRef.SetValueAsync(updatedData).ContinueWithOnMainThread(saveTask =>
            {
                if (saveTask.IsCompletedSuccessfully)
                {
                    // 요청 정리
                    tokenRequestRef.RemoveValueAsync();
                }
                
                isProcessingRequest = false;
            });
        });
    }
    
    public void RequestUseTokens(int amount)
    {
        if (!isInitialized || isProcessingRequest)
        {
            Debug.LogWarning("토큰 시스템이 준비되지 않았거나 처리 중입니다");
            return;
        }
        
        if (currentTokens < amount)
        {
            Debug.Log($"토큰 부족: 필요={amount}, 보유={currentTokens}");
            return;
        }
        
        // 서버에 토큰 사용 요청
        FirebaseManager.GetServerTimestamp((serverTime) =>
        {
            var requestData = new Dictionary<string, object>
            {
                ["useTokenRequest"] = amount,
                ["requestTime"] = serverTime
            };
            
            tokenRequestRef.SetValueAsync(requestData);
        });
    }
    
    // 호환성을 위한 즉시 확인 메서드 (기존 코드용)
    public bool UseTokens(int amount)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("토큰 시스템이 준비되지 않았습니다");
            return false;
        }
        
        if (currentTokens < amount)
        {
            return false;
        }
        
        // 즉시 토큰 사용 요청 (서버 처리)
        RequestUseTokens(amount);
        return true; // 요청 성공 (실제 토큰 차감은 서버에서)
    }
    
    // 콜백 방식 토큰 사용 
    public void UseTokensWithCallback(int amount, Action<bool> onComplete)
    {
        if (!isInitialized)
        {
            onComplete?.Invoke(false);
            return;
        }
        
        if (currentTokens < amount)
        {
            onComplete?.Invoke(false);
            return;
        }
        
        int tokensBeforeRequest = currentTokens;
        RequestUseTokens(amount);
        
        // 토큰 변화 감지를 위한 코루틴 시작
        StartCoroutine(WaitForTokenChange(tokensBeforeRequest, amount, onComplete));
    }
    
    private IEnumerator WaitForTokenChange(int originalTokens, int requestedAmount, Action<bool> onComplete)
    {
        float timeout = 5f; // 5초 타임아웃
        float elapsed = 0f;
        
        while (elapsed < timeout)
        {
            // 토큰이 줄어들었으면 성공
            if (currentTokens == originalTokens - requestedAmount)
            {
                onComplete?.Invoke(true);
                yield break;
            }
            
            // 토큰이 그대로면 아직 처리 중
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        
        // 타임아웃 시 실패로 처리
        onComplete?.Invoke(false);
    }
    
    public void RequestTokenUpdate()
    {
        if (!isInitialized || isProcessingRequest)
            return;
        
        // 서버에 토큰 업데이트 요청
        FirebaseManager.GetServerTimestamp((serverTime) =>
        {
            var requestData = new Dictionary<string, object>
            {
                ["updateRequest"] = serverTime
            };
            
            tokenRequestRef.SetValueAsync(requestData);
        });
    }
    
    private void UpdateUI()
    {
        if (tokenCountText != null)
        {
            tokenCountText.text = $"{currentTokens}/{maxTokens}";
        }
        
        if (tokenFillImage != null)
        {
            float fillAmount = (float)currentTokens / maxTokens;
            tokenFillImage.fillAmount = fillAmount;
        }
        
        // 실시간 타이머 시작/중지 관리
        ManageRegenTimerCoroutine();
    }
    
    // 코루틴 관리 메서드
    private void ManageRegenTimerCoroutine()
    {
        // 토큰이 풀이면 타이머 중지
        if (currentTokens >= maxTokens)
        {
            if (regenTimerCoroutine != null)
            {
                StopCoroutine(regenTimerCoroutine);
                regenTimerCoroutine = null;
            }
            
            if (regenTimeText != null)
                regenTimeText.text = "FULL";
            
            return;
        }
        
        // 토큰이 부족하면 타이머 시작 (이미 실행 중이 아닐 때만)
        if (regenTimerCoroutine == null)
        {
            regenTimerCoroutine = StartCoroutine(RegenTimerCoroutine());
        }
    }
    
    // 1초마다 실행되는 코루틴
    private IEnumerator RegenTimerCoroutine()
    {
        while (currentTokens < maxTokens)
        {
            UpdateRegenTimerDisplay();
            yield return new WaitForSeconds(1f); // 1초마다 실행
        }
        
        // 토큰이 풀이 되면 자동 종료
        if (regenTimeText != null)
            regenTimeText.text = "FULL";
        
        regenTimerCoroutine = null;
    }
    
    // 개선된 타이머 표시 메서드 (서버 요청 없이 로컬 계산)
    private void UpdateRegenTimerDisplay()
    {
        if (regenTimeText == null || lastServerUpdateTime == 0) 
            return;
        
        // 로컬 시간으로 계산 (서버 요청 없음)
        long currentTimeMillis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long timeSinceLastRegen = currentTimeMillis - lastServerUpdateTime;
        double minutesSinceRegen = timeSinceLastRegen / (1000.0 * 60.0);
        double minutesUntilNext = regenIntervalMinutes - (minutesSinceRegen % regenIntervalMinutes);
        
        if (minutesUntilNext <= 0)
        {
            // 서버에 업데이트 요청 (토큰 회복 시간이 됨)
            RequestTokenUpdate();
            return;
        }
        
        int minutes = Mathf.Max(0, (int)minutesUntilNext);
        int seconds = Mathf.Max(0, (int)((minutesUntilNext - minutes) * 60));
        
        regenTimeText.text = $"{minutes:D2}:{seconds:D2}";
    }
    
    public void OnUserLoggedIn()
    {
        if (!isInitialized)
        {
            StartCoroutine(InitializeSecureTokenSystem());
        }
        else
        {
            RequestTokenUpdate();
        }
    }
    
    public void OnUserLoggedOut()
    {
        // 코루틴 정지
        StopAllCoroutines();
        
        // 리스너 해제
        if (userTokenRef != null)
            userTokenRef.ValueChanged -= OnTokenDataChanged;
        
        if (tokenRequestRef != null)
            tokenRequestRef.ValueChanged -= ProcessTokenRequests;
        
        userTokenRef = null;
        tokenRequestRef = null;
        globalSettingsRef = null;
        
        isInitialized = false;
        currentTokens = maxTokens;
        lastServerUpdateTime = 0;
        isProcessingRequest = false;
        
        // 타이머 코루틴 정리
        if (regenTimerCoroutine != null)
        {
            StopCoroutine(regenTimerCoroutine);
            regenTimerCoroutine = null;
        }
        
        UpdateUI();
    }
    
    // 읽기 전용 메서드들
    public int GetCurrentTokens() => currentTokens;
    public int GetMaxTokens() => maxTokens;
    public bool HasEnoughTokens(int amount) => currentTokens >= amount;
    
    // 서버 기반 남은 시간 확인 (분 단위)
    public float GetMinutesUntilNextRegen()
    {
        if (currentTokens >= maxTokens || lastServerUpdateTime == 0)
            return 0f;
        
        DateTime lastUpdateDateTime = DateTimeOffset.FromUnixTimeMilliseconds(lastServerUpdateTime).DateTime;
        DateTime now = DateTime.UtcNow;
        
        TimeSpan timeSinceUpdate = now - lastUpdateDateTime;
        double minutesSinceUpdate = timeSinceUpdate.TotalMinutes;
        double minutesUntilNext = regenIntervalMinutes - (minutesSinceUpdate % regenIntervalMinutes);
        
        return Mathf.Max(0f, (float)minutesUntilNext);
    }
    
    // 관리자용 (테스트)
    [ContextMenu("토큰 업데이트 요청")]
    public void DebugRequestUpdate()
    {
        RequestTokenUpdate();
    }
    
    [ContextMenu("10개 토큰 사용 요청")]
    public void DebugUseTokens()
    {
        RequestUseTokens(10);
    }
}
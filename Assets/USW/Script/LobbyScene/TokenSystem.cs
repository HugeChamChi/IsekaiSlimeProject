using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TokenSystem : MonoBehaviour
{
    [Header("Token Settings")]
    [SerializeField] private int maxTokens = 100;
    [SerializeField] private int tokensPerUse = 10;
    [SerializeField] private float rechargeTimeMinutes = 10f;
    
    [Header("UI Components")]
    [SerializeField] private Image tokenFillImage; 
    [SerializeField] private TextMeshProUGUI tokenText; 
    [SerializeField] private TextMeshProUGUI timeLeftText; 
    [SerializeField] private Button useTokenButton; 
    
    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 1f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private int currentTokens;
    private float timeUntilNextRecharge;
    private bool isRecharging = false;
    private Coroutine animationCoroutine;
    private Coroutine rechargeCoroutine;
    
    private void Start()
    {
        InitializeTokenSystem();
        SetupUI();
    }
    
    private void InitializeTokenSystem()
    {
        currentTokens = maxTokens;
        timeUntilNextRecharge = 0f;
        
        // 저장된 데이터가 있다면 불러오기 (PlayerPrefs 사용)
        LoadTokenData();
        
        UpdateUI();
        
        // 리차지가 필요한 경우 코루틴 시작
        if (currentTokens < maxTokens)
        {
            StartRechargeCoroutine();
        }
    }
    
    private void SetupUI()
    {
        if (useTokenButton != null)
        {
            useTokenButton.onClick.AddListener(UseTokens);
        }
        
        UpdateButtonInteractable();
    }
    
    public void UseTokens()
    {
        if (currentTokens < tokensPerUse)
        {
            Debug.Log($"토큰이 부족합니다. 현재: {currentTokens}, 필요: {tokensPerUse}");
            return;
        }
        
        // 토큰 사용
        int previousTokens = currentTokens;
        currentTokens -= tokensPerUse;
        
     
        StartTokenAnimation(previousTokens, currentTokens);
        
      
        if (!isRecharging && currentTokens < maxTokens)
        {
            StartRechargeCoroutine();
        }
        
        // 데이터 저장
        SaveTokenData();
        
        UpdateButtonInteractable();
        
    }
    
    private void StartTokenAnimation(int fromTokens, int toTokens)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        
        animationCoroutine = StartCoroutine(AnimateTokenFill(fromTokens, toTokens));
    }
    
    private IEnumerator AnimateTokenFill(int fromTokens, int toTokens)
    {
        float startTime = Time.time;
        float fromFillAmount = (float)fromTokens / maxTokens;
        float toFillAmount = (float)toTokens / maxTokens;
        
        while (Time.time - startTime < animationDuration)
        {
            float progress = (Time.time - startTime) / animationDuration;
            float curveValue = animationCurve.Evaluate(progress);
            
            
            float currentFillAmount = Mathf.Lerp(fromFillAmount, toFillAmount, curveValue);
            if (tokenFillImage != null)
            {
                tokenFillImage.fillAmount = currentFillAmount;
            }
            
           
            int displayTokens = Mathf.RoundToInt(Mathf.Lerp(fromTokens, toTokens, curveValue));
            UpdateTokenText(displayTokens);
            
            yield return null;
        }
        
        // 최종 값 설정
        if (tokenFillImage != null)
        {
            tokenFillImage.fillAmount = toFillAmount;
        }
        UpdateTokenText(toTokens);
        
        animationCoroutine = null;
    }
    
    private void StartRechargeCoroutine()
    {
        if (rechargeCoroutine != null)
        {
            StopCoroutine(rechargeCoroutine);
        }
        
        isRecharging = true;
        
        // 시간이 설정되지 않았다면 새로운 리차지 시작
        if (timeUntilNextRecharge <= 0)
        {
            timeUntilNextRecharge = rechargeTimeMinutes * 60f;
        }
        
        rechargeCoroutine = StartCoroutine(RechargeTokens());
    }
    
    private IEnumerator RechargeTokens()
    {
        while (currentTokens < maxTokens)
        {
            // 타이머 업데이트
            while (timeUntilNextRecharge > 0)
            {
                timeUntilNextRecharge -= Time.deltaTime;
                UpdateTimeLeftText();
                yield return null;
            }
            
            // 토큰 충전
            if (currentTokens < maxTokens)
            {
                int previousTokens = currentTokens;
                currentTokens++;
                
                
                StartTokenAnimation(previousTokens, currentTokens);
                
            
                timeUntilNextRecharge = rechargeTimeMinutes * 60f;
                
             
                SaveTokenData();
                
                UpdateButtonInteractable();
                
            }
        }
        
        // 모든 토큰이 충전되면 리차지 종료
        isRecharging = false;
        timeUntilNextRecharge = 0;
        UpdateTimeLeftText();
        SaveTokenData();
        
        rechargeCoroutine = null;
    }
    
    private void UpdateUI()
    {
        UpdateTokenFill();
        UpdateTokenText(currentTokens);
        UpdateTimeLeftText();
    }
    
    private void UpdateTokenFill()
    {
        if (tokenFillImage != null)
        {
            tokenFillImage.fillAmount = (float)currentTokens / maxTokens;
        }
    }
    
    private void UpdateTokenText(int displayTokens)
    {
        if (tokenText != null)
        {
            tokenText.text = $"{displayTokens} / {maxTokens}";
        }
    }
    
    private void UpdateTimeLeftText()
    {
        if (timeLeftText != null)
        {
            if (isRecharging && timeUntilNextRecharge > 0)
            {
                int minutes = Mathf.FloorToInt(timeUntilNextRecharge / 60f);
                int seconds = Mathf.FloorToInt(timeUntilNextRecharge % 60f);
                timeLeftText.text = $"Time left {minutes:00}:{seconds:00}";
            }
            else
            {
                timeLeftText.text = "Time left 00:00";
            }
        }
    }
    
    private void UpdateButtonInteractable()
    {
        if (useTokenButton != null)
        {
            useTokenButton.interactable = currentTokens >= tokensPerUse;
        }
    }
    
    #region Save/Load System
    
    private void SaveTokenData()
    {
        PlayerPrefs.SetInt("CurrentTokens", currentTokens);
        PlayerPrefs.SetFloat("TimeUntilNextRecharge", timeUntilNextRecharge);
        PlayerPrefs.SetString("LastSaveTime", System.DateTime.Now.ToBinary().ToString());
        PlayerPrefs.Save();
    }
    
    private void LoadTokenData()
    {
        if (PlayerPrefs.HasKey("CurrentTokens"))
        {
            currentTokens = PlayerPrefs.GetInt("CurrentTokens", maxTokens);
            timeUntilNextRecharge = PlayerPrefs.GetFloat("TimeUntilNextRecharge", 0f);
            
            // 오프라인 시간 계산
            if (PlayerPrefs.HasKey("LastSaveTime"))
            {
                string lastSaveTimeString = PlayerPrefs.GetString("LastSaveTime");
                if (long.TryParse(lastSaveTimeString, out long lastSaveTimeBinary))
                {
                    System.DateTime lastSaveTime = System.DateTime.FromBinary(lastSaveTimeBinary);
                    double offlineSeconds = (System.DateTime.Now - lastSaveTime).TotalSeconds;
                    
                    ProcessOfflineTime(offlineSeconds);
                }
            }
        }
    }
    
    private void ProcessOfflineTime(double offlineSeconds)
    {
        if (currentTokens >= maxTokens || offlineSeconds <= 0)
            return;
        
        // 남은 리차지 시간 차감
        if (timeUntilNextRecharge > 0)
        {
            timeUntilNextRecharge -= (float)offlineSeconds;
        }
        
        // 오프라인 동안 충전된 토큰 계산
        while (timeUntilNextRecharge <= 0 && currentTokens < maxTokens)
        {
            currentTokens++;
            timeUntilNextRecharge += rechargeTimeMinutes * 60f;
        }
        
        // 최대 토큰 수 제한
        if (currentTokens >= maxTokens)
        {
            currentTokens = maxTokens;
            timeUntilNextRecharge = 0;
        }
    }
    
    #endregion
    
    #region Public Methods (디버깅용)
    
    [ContextMenu("Add 10 Tokens")]
    public void AddTokensForTesting()
    {
        int previousTokens = currentTokens;
        currentTokens = Mathf.Min(currentTokens + 10, maxTokens);
        StartTokenAnimation(previousTokens, currentTokens);
        SaveTokenData();
    }
    
    [ContextMenu("Reset Tokens")]
    public void ResetTokens()
    {
        currentTokens = maxTokens;
        timeUntilNextRecharge = 0;
        isRecharging = false;
        
        if (rechargeCoroutine != null)
        {
            StopCoroutine(rechargeCoroutine);
            rechargeCoroutine = null;
        }
        
        UpdateUI();
        SaveTokenData();
    }
    
    #endregion
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveTokenData();
        }
        else
        {
            LoadTokenData();
            UpdateUI();
            
            if (currentTokens < maxTokens && !isRecharging)
            {
                StartRechargeCoroutine();
            }
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveTokenData();
        }
        else
        {
            LoadTokenData();
            UpdateUI();
            
            if (currentTokens < maxTokens && !isRecharging)
            {
                StartRechargeCoroutine();
            }
        }
    }
}
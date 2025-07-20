using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SettingPanels : MonoBehaviour
{
    [Header("Tab Buttons")]
    [SerializeField] private Button graphicTabButton;
    [SerializeField] private Button soundTabButton;
    [SerializeField] private Button accountTabButton;
    [SerializeField] private Button backButton;
    
    [Header("Tab Content Panels")]
    [SerializeField] private GameObject graphicPanel;
    [SerializeField] private GameObject soundPanel;
    [SerializeField] private GameObject accountPanel;
    
    [Header("Tab Visual Feedback")]
    [SerializeField] private Image[] tabHighlights;
    [SerializeField] private Color activeTabColor = Color.white;
    [SerializeField] private Color inactiveTabColor = Color.gray;
    
    [Header("Graphic Settings")]
    [SerializeField] private Button lowQualityButton;
    [SerializeField] private Button mediumQualityButton;
    [SerializeField] private Button highQualityButton;
    
    [Header("Quality Button Visual")]
    [SerializeField] private Color selectedQualityColor = Color.green;
    [SerializeField] private Color unselectedQualityColor = Color.white;
    
    [Header("Sound Settings")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TextMeshProUGUI bgmValueText;
    [SerializeField] private TextMeshProUGUI sfxValueText;
    
    [Header("Account Settings")]
    [SerializeField] private TextMeshProUGUI uidText;
    [SerializeField] private TextMeshProUGUI emailText;
    [SerializeField] private TextMeshProUGUI currentNicknameText;
    
    [Header("Nickname Change")]
    [SerializeField] private GameObject nicknameEditPanel;
    [SerializeField] private TMP_InputField nicknameInputField;
    [SerializeField] private Button editNicknameButton;
    [SerializeField] private Button saveNicknameButton;
    [SerializeField] private Button cancelNicknameButton;
    
    [Header("Account Management")]
    [SerializeField] private Button deleteAccountButton;
    [SerializeField] private Button logoutButton;
    
    [Header("Animation Settings")]
    [SerializeField] private float tabTransitionDuration = 0.3f;
    [SerializeField] private AnimationCurve tabTransitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    public enum SettingTab
    {
        Graphic,
        Sound,
        Account
    }
    
    public enum QualityLevel
    {
        Low = 0,
        Medium = 1,
        High = 2
    }
    
    private SettingTab currentTab = SettingTab.Sound;
    private bool isNicknameEditing = false;
    private QualityLevel currentQuality = QualityLevel.Medium;
    
    private float bgmVolume = 1f;
    private float sfxVolume = 1f;
    
    private bool isProcessingLogout = false;
    private bool isProcessingAccountDeletion = false;
    
    private void Start()
    {
        InitializeUI();
        SetupEvents();
        LoadSettings();
        SwitchTab(SettingTab.Sound);
    }
    
    private void OnEnable()
    {
        UpdateAccountInfo();
    }
    
    private void InitializeUI()
    {
        if (graphicPanel != null) graphicPanel.SetActive(false);
        if (soundPanel != null) soundPanel.SetActive(false);
        if (accountPanel != null) accountPanel.SetActive(false);
        if (nicknameEditPanel != null) nicknameEditPanel.SetActive(false);
        
        SetNicknameEditMode(false);
    }
    
    private void SetupEvents()
    {
        if (graphicTabButton != null) graphicTabButton.onClick.AddListener(() => SwitchTab(SettingTab.Graphic));
        if (soundTabButton != null) soundTabButton.onClick.AddListener(() => SwitchTab(SettingTab.Sound));
        if (accountTabButton != null) accountTabButton.onClick.AddListener(() => SwitchTab(SettingTab.Account));
        if (backButton != null) backButton.onClick.AddListener(OnBackClick);
        
        if (lowQualityButton != null) lowQualityButton.onClick.AddListener(() => OnQualityButtonClick(QualityLevel.Low));
        if (mediumQualityButton != null) mediumQualityButton.onClick.AddListener(() => OnQualityButtonClick(QualityLevel.Medium));
        if (highQualityButton != null) highQualityButton.onClick.AddListener(() => OnQualityButtonClick(QualityLevel.High));
        
        if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        
        if (editNicknameButton != null) editNicknameButton.onClick.AddListener(OnEditNicknameClick);
        if (saveNicknameButton != null) saveNicknameButton.onClick.AddListener(OnSaveNicknameClick);
        if (cancelNicknameButton != null) cancelNicknameButton.onClick.AddListener(OnCancelNicknameClick);
        
        if (deleteAccountButton != null) deleteAccountButton.onClick.AddListener(OnDeleteAccountClick);
        if (logoutButton != null) logoutButton.onClick.AddListener(OnLogoutClick);
        
        if (nicknameInputField != null)
        {
            nicknameInputField.onEndEdit.AddListener(delegate { 
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    OnSaveNicknameClick();
                }
            });
        }
    }
    
    #region 탭 전환 시스템
    
    public void SwitchTab(SettingTab tab)
    {
        if (currentTab == tab) return;
        
        if (isNicknameEditing)
        {
            SetNicknameEditMode(false);
        }
        
        StartCoroutine(SwitchTabWithAnimation(tab));
    }
    
    private IEnumerator SwitchTabWithAnimation(SettingTab newTab)
    {
        GameObject currentPanel = GetCurrentPanel();
        if (currentPanel != null)
        {
            yield return StartCoroutine(FadePanel(currentPanel, 1f, 0f));
            currentPanel.SetActive(false);
        }
        
        currentTab = newTab;
        GameObject newPanel = GetCurrentPanel();
        if (newPanel != null)
        {
            newPanel.SetActive(true);
            yield return StartCoroutine(FadePanel(newPanel, 0f, 1f));
        }
        
        UpdateTabHighlights();
        OnTabSwitched(newTab);
    }
    
    private IEnumerator FadePanel(GameObject panel, float from, float to)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }
        
        float startTime = Time.time;
        while (Time.time - startTime < tabTransitionDuration)
        {
            float progress = (Time.time - startTime) / tabTransitionDuration;
            float curveValue = tabTransitionCurve.Evaluate(progress);
            canvasGroup.alpha = Mathf.Lerp(from, to, curveValue);
            yield return null;
        }
        
        canvasGroup.alpha = to;
    }
    
    private GameObject GetCurrentPanel()
    {
        switch (currentTab)
        {
            case SettingTab.Graphic: return graphicPanel;
            case SettingTab.Sound: return soundPanel;
            case SettingTab.Account: return accountPanel;
            default: return null;
        }
    }
    
    private void UpdateTabHighlights()
    {
        if (tabHighlights == null || tabHighlights.Length < 3) return;
        
        for (int i = 0; i < tabHighlights.Length; i++)
        {
            if (tabHighlights[i] != null)
            {
                tabHighlights[i].color = (i == (int)currentTab) ? activeTabColor : inactiveTabColor;
            }
        }
    }
    
    private void OnTabSwitched(SettingTab tab)
    {
        switch (tab)
        {
            case SettingTab.Graphic:
                UpdateGraphicSettings();
                break;
            case SettingTab.Sound:
                UpdateSoundSettings();
                break;
            case SettingTab.Account:
                UpdateAccountInfo();
                break;
        }
    }
    
    #endregion
    
    #region Graphic Settings
    
    private void UpdateGraphicSettings()
    {
        UpdateQualityButtonVisuals();
    }
    
    private void OnQualityButtonClick(QualityLevel quality)
    {
        currentQuality = quality;
        QualitySettings.SetQualityLevel((int)quality);
        PlayerPrefs.SetInt("QualityLevel", (int)quality);
        PlayerPrefs.Save();
        UpdateQualityButtonVisuals();
        Debug.Log($"품질 설정 변경: {quality}");
    }
    
    private void UpdateQualityButtonVisuals()
    {
        SetQualityButtonColor(lowQualityButton, currentQuality == QualityLevel.Low);
        SetQualityButtonColor(mediumQualityButton, currentQuality == QualityLevel.Medium);
        SetQualityButtonColor(highQualityButton, currentQuality == QualityLevel.High);
    }
    
    private void SetQualityButtonColor(Button button, bool isSelected)
    {
        if (button == null) return;
        
        ColorBlock colors = button.colors;
        colors.normalColor = isSelected ? selectedQualityColor : unselectedQualityColor;
        colors.highlightedColor = isSelected ? selectedQualityColor : unselectedQualityColor;
        colors.selectedColor = isSelected ? selectedQualityColor : unselectedQualityColor;
        button.colors = colors;
    }
    
    #endregion
    
    #region Sound Settings
    
    private void UpdateSoundSettings()
    {
        if (bgmSlider != null)
        {
            bgmSlider.value = bgmVolume;
            UpdateBGMValueText();
        }
        
        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVolume;
            UpdateSFXValueText();
        }
    }
    
    private void OnBGMVolumeChanged(float value)
    {
        bgmVolume = value;
        SaveAudioSettings();
        UpdateBGMValueText();
    }
    
    private void OnSFXVolumeChanged(float value)
    {
        sfxVolume = value;
        SaveAudioSettings();
        UpdateSFXValueText();
    }
    
    private void UpdateBGMValueText()
    {
        if (bgmValueText != null)
        {
            bgmValueText.text = Mathf.RoundToInt(bgmVolume * 100).ToString();
        }
    }
    
    private void UpdateSFXValueText()
    {
        if (sfxValueText != null)
        {
            sfxValueText.text = Mathf.RoundToInt(sfxVolume * 100).ToString();
        }
    }
    
    private void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }
    
    #endregion
    
    #region Account Settings
    
    private void UpdateAccountInfo()
    {
        if (!FirebaseManager.IsUserLoggedIn())
        {
            SetAccountInfoText("정보 로딩 중...");
            return;
        }
        
        if (uidText != null)
        {
            uidText.text = $"{FirebaseManager.GetCurrentUserID()}";
        }
        
        if (emailText != null)
        {
            emailText.text = $"{FirebaseManager.GetCurrentUserEmail()}";
        }
        
        if (currentNicknameText != null)
        {
            currentNicknameText.text = $"{FirebaseManager.GetCurrentUserDisplayName()}";
        }
    }
    
    private void SetAccountInfoText(string message)
    {
        if (uidText != null) uidText.text = message;
        if (emailText != null) emailText.text = "";
        if (currentNicknameText != null) currentNicknameText.text = "";
    }
    
    public void OnEditNicknameClick()
    {
        SetNicknameEditMode(true);
        
        if (nicknameInputField != null)
        {
            nicknameInputField.text = FirebaseManager.GetCurrentUserDisplayName();
            nicknameInputField.Select();
        }
    }
    
    public void OnSaveNicknameClick()
    {
        if (nicknameInputField == null) return;
        
        string newNickname = nicknameInputField.text.Trim();
        
        if (string.IsNullOrEmpty(newNickname))
        {
            ShowMessage("닉네임을 입력해주세요.");
            return;
        }
        
        if (newNickname.Length < 2 || newNickname.Length > 10)
        {
            ShowMessage("닉네임은 2~10자로 입력해주세요.");
            return;
        }
        
        if (newNickname == FirebaseManager.GetCurrentUserDisplayName())
        {
            SetNicknameEditMode(false);
            return;
        }
        
        ShowMessage("닉네임 변경 중...");
        
        FirebaseManager.UpdateUserProfile(newNickname, (success) =>
        {
            if (success)
            {
                ShowMessage("닉네임이 변경되었습니다.");
                SetNicknameEditMode(false);
                UpdateAccountInfo();
                
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.UpdateUserName(newNickname);
                }
            }
            else
            {
                ShowMessage("닉네임 변경에 실패했습니다.");
            }
        });
    }
    
    public void OnCancelNicknameClick()
    {
        SetNicknameEditMode(false);
    }
    
    private void SetNicknameEditMode(bool editing)
    {
        isNicknameEditing = editing;
        
        if (editNicknameButton != null) editNicknameButton.gameObject.SetActive(!editing);
        if (nicknameEditPanel != null) nicknameEditPanel.SetActive(editing);
    }
    
    public void OnLogoutClick()
    {
        if (isProcessingLogout)
        {
            Debug.Log("이미 로그아웃 처리 중입니다.");
            return;
        }
        
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowConfirmationPopup(
                "정말 로그아웃 하시겠습니까?",
                onYes: ConfirmLogout,
                onNo: () => Debug.Log("로그아웃 취소됨")
            );
        }
        else
        {
            Debug.LogWarning("PopupManager가 없어서 확인 없이 로그아웃을 진행합니다.");
            ConfirmLogout();
        }
    }
    
    private void ConfirmLogout()
    {
        if (isProcessingLogout) return;
        
        isProcessingLogout = true;
        
        Debug.Log("로그아웃 시작");
        
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowLoadingPopup("로그아웃 중...");
        }
        
        StartCoroutine(ProcessLogout());
    }
    
    private IEnumerator ProcessLogout()
    {
        Debug.Log("로그아웃 프로세스 시작");
        
        bool logoutSuccess = false;
        string errorMessage = "";
        
        try
        {
            FirebaseManager.SignOut();
            Debug.Log("Firebase 로그아웃 완료");
            logoutSuccess = true;
        }
        catch (System.Exception e)
        {
            errorMessage = e.Message;
            Debug.LogError($"Firebase 로그아웃 중 오류: {errorMessage}");
        }
        
        if (logoutSuccess)
        {
            try
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ClearUserInfo();
                    Debug.Log("GameManager 사용자 정보 정리 완료");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"GameManager 정리 중 오류: {e.Message}");
            }
        }
        
        yield return new WaitForSeconds(0.5f);
        
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.HideLoadingPopup();
        }
        
        if (logoutSuccess)
        {
            try
            {
                CleanupDontDestroyOnLoadObjects();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"오브젝트 정리 중 오류: {e.Message}");
            }
            
            try
            {
                Debug.Log("LoginScene으로 전환 시작");
                SceneManager.LoadScene("LoginScene");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Scene 전환 중 오류: {e.Message}");
                if (PopupManager.Instance != null)
                {
                    PopupManager.Instance.ShowPopup("Scene 전환 중 오류가 발생했습니다.");
                }
                isProcessingLogout = false;
            }
        }
        else
        {
            if (PopupManager.Instance != null)
            {
                PopupManager.Instance.ShowPopup($"로그아웃 중 오류가 발생했습니다.\n{errorMessage}");
            }
            isProcessingLogout = false;
        }
    }
    
    public void OnDeleteAccountClick()
    {
        if (isProcessingAccountDeletion)
        {
            Debug.Log("이미 계정 탈퇴 처리 중입니다.");
            return;
        }
        
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowConfirmationPopup(
                "정말로 계정을 탈퇴하시겠습니까?\n\n모든 데이터가 영구 삭제되며\n복구할 수 없습니다!",
                onYes: ConfirmDeleteAccount,
                onNo: () => Debug.Log("계정 탈퇴 취소됨")
            );
        }
        else
        {
            Debug.LogWarning("PopupManager가 없어서 확인 없이 계정 탈퇴를 진행합니다.");
            ConfirmDeleteAccount();
        }
    }
    
    private void ConfirmDeleteAccount()
    {
        if (isProcessingAccountDeletion) return;
        
        isProcessingAccountDeletion = true;
        
        Debug.Log("계정 탈퇴 시작");
        
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowLoadingPopup("계정 탈퇴 처리 중...");
        }
        
        StartCoroutine(ProcessAccountDeletion());
    }
    
    private IEnumerator ProcessAccountDeletion()
    {
        Debug.Log("계정 탈퇴 프로세스 시작");
        
        bool deletionSuccess = false;
        bool deletionCompleted = false;
        
        FirebaseManager.DeleteCurrentUserAccount((success) =>
        {
            deletionSuccess = success;
            deletionCompleted = true;
            
            if (success)
            {
                Debug.Log("Firebase 계정 삭제 성공");
            }
            else
            {
                Debug.LogError("Firebase 계정 삭제 실패");
            }
        });
        
        float timeout = 0f;
        while (!deletionCompleted && timeout < 10f)
        {
            yield return new WaitForSeconds(0.1f);
            timeout += 0.1f;
        }
        
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.HideLoadingPopup();
        }
        
        if (deletionSuccess)
        {
            if (PopupManager.Instance != null)
            {
                PopupManager.Instance.ShowPopupWithAutoClose("계정 탈퇴가 완료되었습니다.", 2f);
            }
            
            yield return new WaitForSeconds(2f);
            
            try
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ClearUserInfo();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"GameManager 정리 중 오류: {e.Message}");
            }
            
            try
            {
                CleanupDontDestroyOnLoadObjects();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"오브젝트 정리 중 오류: {e.Message}");
            }
            
            try
            {
                Debug.Log("계정 탈퇴 완료 - LoginScene으로 전환");
                SceneManager.LoadScene("LoginScene");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Scene 전환 중 오류: {e.Message}");
                if (PopupManager.Instance != null)
                {
                    PopupManager.Instance.ShowPopup("Scene 전환 중 오류가 발생했습니다.");
                }
                isProcessingAccountDeletion = false;
            }
        }
        else
        {
            if (PopupManager.Instance != null)
            {
                PopupManager.Instance.ShowPopup("계정 탈퇴 중 오류가 발생했습니다.\n잠시 후 다시 시도해주세요.");
            }
            
            isProcessingAccountDeletion = false;
        }
    }
    
    private void CleanupDontDestroyOnLoadObjects()
    {
        Debug.Log("DontDestroyOnLoad 오브젝트 정리 시작");
        
        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
            Debug.Log("GameManager 정리 완료");
        }
        
        Debug.Log("DontDestroyOnLoad 오브젝트 정리 완료");
    }
    
    #endregion
    
    #region Common UI Methods
    
    private void ShowMessage(string message)
    {
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowPopup(message);
        }
        else
        {
            Debug.Log($"[SettingPanel] {message}");
        }
    }
    
    public void OnBackClick()
    {
        if (isNicknameEditing)
        {
            SetNicknameEditMode(false);
            return;
        }
        
        gameObject.SetActive(false);
    }
    
    #endregion
    
    #region Settings Load/Save
    
    private void LoadSettings()
    {
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        
        int qualityLevel = PlayerPrefs.GetInt("QualityLevel", 1);
        currentQuality = (QualityLevel)qualityLevel;
        QualitySettings.SetQualityLevel(qualityLevel);
    }
    
    #endregion
    
    #region Public Methods
    
    public void OpenPanel()
    {
        gameObject.SetActive(true);
        SwitchTab(SettingTab.Sound);
    }
    
    public void ClosePanel()
    {
        OnBackClick();
    }
    
    #endregion
}
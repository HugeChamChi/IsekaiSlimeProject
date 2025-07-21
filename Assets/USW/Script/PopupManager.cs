using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Extensions;

public class PopupManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] TMP_Text statusText;
    [SerializeField] Button closeButton;
    [SerializeField] Canvas canvas;
    
    [Header("Confirmation Popup")]
    [SerializeField] GameObject confirmationPanel;
    [SerializeField] TMP_Text confirmationText;
    [SerializeField] Button yesButton;
    [SerializeField] Button noButton;
    
    [Header("Password Change Popup")]
    [SerializeField] GameObject passwordChangePanel;
    [SerializeField] TMP_InputField currentPasswordInput;
    [SerializeField] TMP_InputField newPasswordInput;
    [SerializeField] Button changePasswordButton;
    [SerializeField] Button cancelPasswordButton;
    
    private static PopupManager instance;
    public static PopupManager Instance 
    { 
        get 
        {
            if (instance == null)
            {
                CreatePopupManager();
            }
            return instance;
        } 
    }
    
    private Action onYesCallback;
    private Action onNoCallback;
    private bool isProcessingPasswordChange = false;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); 
            
            if (canvas != null)
            {
                canvas.sortingOrder = 1000; 
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SetupButtons();
        gameObject.SetActive(false);
    }
    
    private void SetupButtons()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePopup);
        }
        
        if (yesButton != null)
        {
            yesButton.onClick.AddListener(() => {
                onYesCallback?.Invoke();
                ClosePopup();
            });
        }
        
        if (noButton != null)
        {
            noButton.onClick.AddListener(() => {
                onNoCallback?.Invoke();
                ClosePopup();
            });
        }
        
        if (changePasswordButton != null)
        {
            changePasswordButton.onClick.AddListener(OnChangePasswordClick);
        }
        
        if (cancelPasswordButton != null)
        {
            cancelPasswordButton.onClick.AddListener(OnCancelPasswordClick);
        }
        
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
        
        if (passwordChangePanel != null)
        {
            passwordChangePanel.SetActive(false);
        }
        
        // Enter 키 처리
        if (newPasswordInput != null)
        {
            newPasswordInput.onEndEdit.AddListener(delegate { 
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    OnChangePasswordClick();
                }
            });
        }
    }

    private static void CreatePopupManager()
    {
        GameObject popupPrefab = Resources.Load<GameObject>("PopupManager");
        
        if (popupPrefab != null)
        {
            GameObject popupGO = Instantiate(popupPrefab);
            popupGO.name = "PopupManager"; 
        }
        else
        {
            Debug.LogError("PopupManager 프리팹을 찾을 수 없습니다. Resources 폴더에 있는지 확인하세요.");
        }
    }
    
    public void ShowPopup(string message)
    {
        HideAllPanels();
        
        if (statusText != null)
        {
            statusText.text = message;
        }
        
        ShowMainPanel();
        gameObject.SetActive(true);
    }

    public void ShowConfirmationPopup(string message, Action onYes, Action onNo = null)
    {
        onYesCallback = onYes;
        onNoCallback = onNo;
        
        if (confirmationText != null)
        {
            confirmationText.text = message;
        }
        
        HideAllPanels();
        ShowConfirmationPanel();
        gameObject.SetActive(true);
    }
    
    public void ShowPasswordChangePopup()
    {
        HideAllPanels();
        ShowPasswordChangePanel();
        gameObject.SetActive(true);
        
        // 입력 필드 초기화 및 포커스
        if (currentPasswordInput != null)
        {
            currentPasswordInput.text = "";
            currentPasswordInput.Select();
        }
        
        if (newPasswordInput != null)
        {
            newPasswordInput.text = "";
        }
        
        isProcessingPasswordChange = false;
    }
    
    private void ShowMainPanel()
    {
        if (statusText != null && statusText.transform.parent != null)
        {
            statusText.transform.parent.gameObject.SetActive(true);
        }
    }
    
    private void ShowConfirmationPanel()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
        }
    }
    
    private void ShowPasswordChangePanel()
    {
        if (passwordChangePanel != null)
        {
            passwordChangePanel.SetActive(true);
        }
    }
    
    private void HideAllPanels()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
        
        if (passwordChangePanel != null)
        {
            passwordChangePanel.SetActive(false);
        }
        
        if (statusText != null && statusText.transform.parent != null)
        {
            statusText.transform.parent.gameObject.SetActive(false);
        }
    }

    #region Password Change Logic
    
    private void OnChangePasswordClick()
    {
        if (isProcessingPasswordChange)
        {
            Debug.Log("이미 비밀번호 변경 처리 중입니다.");
            return;
        }
        
        if (currentPasswordInput == null || newPasswordInput == null)
        {
            ShowPopup("필수 입력 필드가 없습니다.");
            return;
        }
        
        string currentPassword = currentPasswordInput.text.Trim();
        string newPassword = newPasswordInput.text.Trim();
        
        if (string.IsNullOrEmpty(currentPassword))
        {
            ShowPopup("현재 비밀번호를 입력해주세요.");
            return;
        }
        
        if (string.IsNullOrEmpty(newPassword))
        {
            ShowPopup("새 비밀번호를 입력해주세요.");
            return;
        }
        
        if (newPassword.Length < 6)
        {
            ShowPopup("새 비밀번호는 6자 이상이어야 합니다.");
            return;
        }
        
        if (currentPassword == newPassword)
        {
            ShowPopup("새 비밀번호는 현재 비밀번호와 달라야 합니다.");
            return;
        }
        
        isProcessingPasswordChange = true;
        ShowLoadingPopup("비밀번호 변경 중...");
        
        StartCoroutine(ProcessPasswordChange(currentPassword, newPassword));
    }
    
    private IEnumerator ProcessPasswordChange(string currentPassword, string newPassword)
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            HideLoadingPopup();
            ShowPopup("로그인되지 않은 사용자입니다.");
            isProcessingPasswordChange = false;
            yield break;
        }
        
        bool reauthSuccess = false;
        bool reauthCompleted = false;
        string reauthErrorMessage = "";
        
        Debug.Log("현재 비밀번호로 재인증 시도");
        
        // 현재 비밀번호로 재인증
        Credential credential = EmailAuthProvider.GetCredential(user.Email, currentPassword);
        
        user.ReauthenticateAsync(credential).ContinueWithOnMainThread(reauthTask =>
        {
            reauthCompleted = true;
            
            if (reauthTask.IsCanceled)
            {
                reauthErrorMessage = "재인증이 취소되었습니다.";
                Debug.LogError("재인증 취소");
            }
            else if (reauthTask.IsFaulted)
            {
                reauthErrorMessage = "현재 비밀번호가 올바르지 않습니다.";
                Debug.LogError($"재인증 실패: {reauthTask.Exception}");
            }
            else
            {
                reauthSuccess = true;
                Debug.Log("재인증 성공");
            }
        });
        
        // 재인증 완료 대기
        float timeout = 0f;
        while (!reauthCompleted && timeout < 10f)
        {
            yield return new WaitForSeconds(0.1f);
            timeout += 0.1f;
        }
        
        if (!reauthSuccess)
        {
            HideLoadingPopup();
            ShowPopup(string.IsNullOrEmpty(reauthErrorMessage) ? "재인증에 실패했습니다." : reauthErrorMessage);
            isProcessingPasswordChange = false;
            yield break;
        }
        
        bool changeSuccess = false;
        bool changeCompleted = false;
        string changeErrorMessage = "";
        
        Debug.Log("비밀번호 변경 시도");
        
        // 비밀번호 변경
        user.UpdatePasswordAsync(newPassword).ContinueWithOnMainThread(changeTask =>
        {
            changeCompleted = true;
            
            if (changeTask.IsCanceled)
            {
                changeErrorMessage = "비밀번호 변경이 취소되었습니다.";
                Debug.LogError("비밀번호 변경 취소");
            }
            else if (changeTask.IsFaulted)
            {
                changeErrorMessage = "비밀번호 변경에 실패했습니다.";
                Debug.LogError($"비밀번호 변경 실패: {changeTask.Exception}");
            }
            else
            {
                changeSuccess = true;
                Debug.Log("비밀번호 변경 성공");
            }
        });
        
        // 비밀번호 변경 완료 대기
        timeout = 0f;
        while (!changeCompleted && timeout < 10f)
        {
            yield return new WaitForSeconds(0.1f);
            timeout += 0.1f;
        }
        
        HideLoadingPopup();
        
        if (changeSuccess)
        {
            ShowPopupWithAutoClose("비밀번호가 성공적으로 변경되었습니다.", 2f);
        }
        else
        {
            ShowPopup(string.IsNullOrEmpty(changeErrorMessage) ? "비밀번호 변경에 실패했습니다." : changeErrorMessage);
        }
        
        isProcessingPasswordChange = false;
    }
    
    private void OnCancelPasswordClick()
    {
        ClosePopup();
    }
    
    #endregion

    public void ClosePopup()
    {
        onYesCallback = null;
        onNoCallback = null;
        isProcessingPasswordChange = false;
        
        CancelInvoke(nameof(ClosePopup));
        
        gameObject.SetActive(false);
    }

    public void ShowPopupWithAutoClose(string message, float duration = 3f)
    {
        ShowPopup(message);
        
        CancelInvoke(nameof(ClosePopup));
        Invoke(nameof(ClosePopup), duration);
    }
    
    public void ShowLoadingPopup(string message = "처리 중...")
    {
        ShowPopup(message);
        
        if (closeButton != null)
        {
            closeButton.interactable = false;
        }
    }
    
    public void HideLoadingPopup()
    {
        if (closeButton != null)
        {
            closeButton.interactable = true;
        }
        
        ClosePopup();
    }
}
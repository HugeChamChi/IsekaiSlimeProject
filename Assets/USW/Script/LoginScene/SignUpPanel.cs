// SignUpPanel.cs - 색상 코드 제거 버전
using Firebase.Auth;
using Firebase.Extensions;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System;

public class SignUpPanel : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] GameObject loginPanel;

    [Header("Input Fields")]
    [SerializeField] TMP_InputField emailInput;
    [SerializeField] TMP_InputField passInput;
    [SerializeField] TMP_InputField passConfirmInput;

    [Header("Buttons")]
    [SerializeField] Button signUpButton;
    [SerializeField] Button cancelButton;

    [Header("UI Feedback")]
    [SerializeField] TMP_Text statusText;

    private bool isSigningUp = false;

    private void Awake()
    {
        SetupButtonListeners();
        ResetUI();
    }

    private void SetupButtonListeners()
    {
        signUpButton.onClick.AddListener(SignUp);
        cancelButton.onClick.AddListener(Cancel);
    }

    private void ResetUI()
    {
        isSigningUp = false;
        UpdateStatusText("회원가입 정보를 입력해주세요");
        
        signUpButton.interactable = true;
        signUpButton.GetComponentInChildren<TMP_Text>().text = "회원가입";
    }

    #region 이메일 형식 검증 (로컬)

    private bool IsValidEmailFormat(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, pattern);
        }
    }

    #endregion

    #region 회원가입

    private void SignUp()
    {
        // 입력 검증
        if (!ValidateInputs())
        {
            return;
        }

        // 회원가입 진행
        isSigningUp = true;
        signUpButton.interactable = false;
        signUpButton.GetComponentInChildren<TMP_Text>().text = "가입 중...";
        UpdateStatusText("회원가입 진행 중...");

        // Firebase 회원가입 시도 
        string email = emailInput.text.Trim();
        string password = passInput.text;

        FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    HandleSignUpResult(false, "회원가입이 취소되었습니다");
                    return;
                }

                if (task.IsFaulted)
                {
                    // Firebase 오류 처리 (이메일 중복 포함)
                    string errorMessage = HandleFirebaseException(task.Exception);
                    HandleSignUpResult(false, errorMessage);
                    return;
                }

                // 회원가입 성공
                FirebaseUser user = task.Result.User;
                UpdateStatusText("회원가입이 완료되었습니다!");
                
                // GameManager에 사용자 정보 저장
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetUserInfo(user);
                }

                // 회원가입 후 로그아웃 (로그인 화면에서 다시 로그인하도록)
                FirebaseAuth.DefaultInstance.SignOut();
                
                ResetSignUpUI();
                gameObject.SetActive(false); 
                loginPanel.SetActive(true);  
                
                ShowCelebrationPopup();
            });
    }

    #endregion

    #region Firebase 오류 처리

    private string HandleFirebaseException(AggregateException exception)
    {
        if (exception != null && exception.InnerExceptions.Count > 0)
        {
            string errorMessage = exception.InnerExceptions[0].Message;
            return GetUserFriendlyErrorFromMessage(errorMessage);
        }
        return "알 수 없는 오류가 발생했습니다";
    }

    private string GetUserFriendlyErrorFromMessage(string errorMessage)
    {
        if (string.IsNullOrEmpty(errorMessage))
            return "알 수 없는 오류가 발생했습니다";
            
        errorMessage = errorMessage.ToLower();
        
        if (errorMessage.Contains("email-already-in-use") || errorMessage.Contains("email already in use"))
        {
            return "이미 사용 중인 이메일입니다. 다른 이메일을 사용해주세요.";
        }
        else if (errorMessage.Contains("invalid-email"))
        {
            return "올바른 이메일 주소를 입력해 주세요";
        }
        else if (errorMessage.Contains("weak-password"))
        {
            return "비밀번호는 6자리 이상이어야 합니다";
        }
        else if (errorMessage.Contains("network") || errorMessage.Contains("connection"))
        {
            return "네트워크 연결을 확인해주세요";
        }
        else if (errorMessage.Contains("too-many-requests"))
        {
            return "너무 많은 요청이 발생했습니다. 잠시 후 다시 시도해주세요.";
        }
        else
        {
            return $"인증 오류: {errorMessage}";
        }
    }

    private void HandleSignUpResult(bool success, string message)
    {
        ResetSignUpUI();
        UpdateStatusText(message);
    }

    #endregion

    #region 입력 검증

    private bool ValidateInputs()
    {
        // 이메일 형식 검증
        string email = emailInput.text.Trim();
        if (!IsValidEmailFormat(email))
        {
            UpdateStatusText("올바른 이메일 형식을 입력해주세요");
            ShowPopup("올바른 이메일 형식을 입력해주세요");
            
            return false;
        }

        // 비밀번호 검증
        if (string.IsNullOrEmpty(passInput.text))
        {
            UpdateStatusText("비밀번호를 입력해주세요");
            ShowPopup("비밀번호를 입력해주세요");
            return false;
        }

        if (passInput.text.Length < 6)
        {
            UpdateStatusText("비밀번호는 6자리 이상이어야 합니다");
            ShowPopup("비밀번호는 6자리 이상이어야 합니다");
            return false;
        }

        // 비밀번호 확인
        if (passInput.text != passConfirmInput.text)
        {
            UpdateStatusText("비밀번호가 일치하지 않습니다");
            ShowPopup("비밀번호가 일치하지 않습니다");
            return false;
        }

        return true;
    }

    #endregion

    #region UI 관리

    private void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"[회원가입 상태] {message}");
    }

    private void ResetSignUpUI()
    {
        isSigningUp = false;
        signUpButton.interactable = true;
        signUpButton.GetComponentInChildren<TMP_Text>().text = "회원가입";
    }

    private void Cancel()
    {
        ResetInputs();
        loginPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    private void ResetInputs()
    {
        emailInput.text = "";
        passInput.text = "";
        passConfirmInput.text = "";
        ResetUI();
    }

    private IEnumerator ReturnToLoginAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 입력 필드 초기화
        ResetInputs();
        
        // 로그인 화면으로 이동
        loginPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    #endregion

    #region  팝업 관련 메서드 추가

    private void ShowPopup(string message)
    {
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowPopup(message);
        }
    }
    
    private void ShowCelebrationPopup()
    {
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowPopup(" 회원가입 성공!");
        }
    }

    #endregion
}
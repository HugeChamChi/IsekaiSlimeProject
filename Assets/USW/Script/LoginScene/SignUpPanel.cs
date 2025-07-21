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
    [SerializeField] Button emailCheckButton; // 이메일 중복체크 버튼

    private bool isSigningUp = false;
    private bool isEmailChecking = false;
    private bool isEmailVerified = false; // 이메일 중복체크 완료 여부
    private string verifiedEmail = ""; // 중복체크 완료된 이메일

    private void Awake()
    {
        SetupButtonListeners();
        ResetUI();
    }

    private void SetupButtonListeners()
    {
        if (signUpButton != null)
            signUpButton.onClick.AddListener(SignUp);
        
        if (cancelButton != null)
            cancelButton.onClick.AddListener(Cancel);
        
        if (emailCheckButton != null)
            emailCheckButton.onClick.AddListener(CheckEmailDuplication);

        // 이메일 입력 필드가 변경되면 체크 상태 리셋
        if (emailInput != null)
            emailInput.onValueChanged.AddListener(OnEmailInputChanged);
    }

    private void ResetUI()
    {
        isSigningUp = false;
        isEmailChecking = false;
        isEmailVerified = false;
        verifiedEmail = "";

        if (signUpButton != null)
        {
            signUpButton.interactable = false; // 초기에는 비활성화 (이메일 체크 완료 후 활성화)
            var signUpText = signUpButton.GetComponentInChildren<TMP_Text>();
            if (signUpText != null)
                signUpText.text = "회원가입";
        }

        if (emailCheckButton != null)
        {
            emailCheckButton.interactable = true;
            var checkText = emailCheckButton.GetComponentInChildren<TMP_Text>();
            if (checkText != null)
                checkText.text = "중복확인";
        }
    }

    private void OnEmailInputChanged(string newEmail)
    {
        // 이메일이 변경되면 중복체크 상태 리셋
        if (verifiedEmail != newEmail.Trim())
        {
            isEmailVerified = false;
            verifiedEmail = "";
            UpdateSignUpButtonState();

            // 중복확인 버튼 텍스트 리셋
            if (emailCheckButton != null)
            {
                var checkText = emailCheckButton.GetComponentInChildren<TMP_Text>();
                if (checkText != null)
                    checkText.text = "중복확인";
            }
        }
    }

    private void UpdateSignUpButtonState()
    {
        // 이메일 중복체크가 완료된 경우에만 회원가입 버튼 활성화
        if (signUpButton != null)
            signUpButton.interactable = !isSigningUp;
    }

    #region 이메일 중복체크

    private void CheckEmailDuplication()
    {
        if (emailInput == null)
        {
            ShowPopup("이메일 입력 필드를 찾을 수 없습니다");
            return;
        }

        string email = emailInput.text.Trim();

        // 이메일 형식 검증
        if (!IsValidEmailFormat(email))
        {
            ShowPopup("올바른 이메일 형식을 입력해주세요");
            return;
        }

        if (isEmailChecking)
        {
            return; // 이미 체크 중이면 중복 실행 방지
        }

        isEmailChecking = true;
        
        if (emailCheckButton != null)
        {
            emailCheckButton.interactable = false;
            var checkText = emailCheckButton.GetComponentInChildren<TMP_Text>();
            if (checkText != null)
                checkText.text = "확인중...";
        }

        // 임시 계정 생성 시도로 이메일 중복 체크
        FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(email, "DummyPassword")
            .ContinueWithOnMainThread(task =>
            {
                ResetEmailCheckUI();

                if (task.IsCanceled)
                {
                    ShowPopup("이메일 확인이 취소되었습니다");
                    return;
                }

                if (task.IsFaulted)
                {
                    // 오류 발생 - 이메일이 이미 존재하는 경우
                    string errorMessage = task.Exception?.GetBaseException()?.Message ?? "";

                    if (errorMessage.ToLower().Contains("email-already-in-use") ||
                        errorMessage.ToLower().Contains("email already in use") ||
                        errorMessage.ToLower().Contains("already in use by another account"))
                    {
                        ShowPopup("이미 존재하는 이메일입니다");
                    }
                    else
                    {
                        ShowPopup(GetUserFriendlyErrorFromMessage(errorMessage));
                    }
                    return;
                }

                // 성공 - 사용 가능한 이메일이므로 즉시 삭제
                FirebaseUser tempUser = task.Result.User;

                // 임시 계정 삭제
                tempUser.DeleteAsync().ContinueWithOnMainThread(deleteTask =>
                {
                    if (deleteTask.IsCompletedSuccessfully)
                    {
                        // 이메일 사용 가능
                        isEmailVerified = true;
                        verifiedEmail = email;
                        UpdateSignUpButtonState();

                        ShowPopup("사용 가능한 이메일입니다!");

                        if (emailCheckButton != null)
                        {
                            var checkText = emailCheckButton.GetComponentInChildren<TMP_Text>();
                            if (checkText != null)
                                checkText.text = "확인완료";
                        }
                    }
                    else
                    {
                        // 삭제 실패 - 경고 메시지
                        Debug.LogError($"임시 계정 삭제 실패: {deleteTask.Exception?.GetBaseException()?.Message}");
                        ShowPopup("이메일 확인 과정에서 오류가 발생했습니다. 다시 시도해주세요.");
                    }
                });
            });
    }

    private void ResetEmailCheckUI()
    {
        isEmailChecking = false;
        if (emailCheckButton != null)
            emailCheckButton.interactable = true;
    }

    #endregion

    #region 이메일 형식 검증

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
        if (emailInput == null)
        {
            ShowPopup("이메일 입력 필드를 찾을 수 없습니다");
            return;
        }

        // 이메일 중복체크 완료 여부 확인
        if (!isEmailVerified || verifiedEmail != emailInput.text.Trim())
        {
            ShowPopup("이메일 중복 확인을 먼저 완료해주세요");
            return;
        }

        // 입력 검증
        if (!ValidateInputs())
        {
            return;
        }

        // 회원가입 진행
        isSigningUp = true;
        
        if (signUpButton != null)
        {
            signUpButton.interactable = false;
            var signUpText = signUpButton.GetComponentInChildren<TMP_Text>();
            if (signUpText != null)
                signUpText.text = "가입 중...";
        }

        // Firebase 회원가입 시도
        string email = emailInput.text.Trim();
        string password = passInput != null ? passInput.text : "";

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
                    // Firebase 오류 처리
                    string errorMessage = HandleFirebaseException(task.Exception);
                    HandleSignUpResult(false, errorMessage);
                    return;
                }

                // 회원가입 성공
                ShowCelebrationPopup();

                // 회원가입 후 즉시 로그아웃 (GameManager 호출 없이)
                FirebaseAuth.DefaultInstance.SignOut();
                
                // 로그아웃 완료 후 로그인 화면으로 이동
                StartCoroutine(WaitForSignOutComplete());
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

        if (errorMessage.Contains("email-already-in-use") || 
            errorMessage.Contains("email already in use") ||
            errorMessage.Contains("already in use by another account"))
        {
            return "이미 존재하는 이메일입니다";
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
            return "너무 많은 요청이 발생했습니다. 잠시 후 다시 시도해주세요";
        }
        else
        {
            return "오류가 발생했습니다. 다시 시도해주세요";
        }
    }

    private void HandleSignUpResult(bool success, string message)
    {
        ResetSignUpUI();
        ShowPopup(message);
    }

    #endregion

    #region 입력 검증

    private bool ValidateInputs()
    {
        if (emailInput == null || passInput == null || passConfirmInput == null)
        {
            ShowPopup("입력 필드를 찾을 수 없습니다");
            return false;
        }

        // 이메일 형식 검증
        string email = emailInput.text.Trim();
        if (!IsValidEmailFormat(email))
        {
            ShowPopup("올바른 이메일 형식을 입력해주세요");
            return false;
        }

        // 비밀번호 검증
        if (string.IsNullOrEmpty(passInput.text))
        {
            ShowPopup("비밀번호를 입력해주세요");
            return false;
        }

        if (passInput.text.Length < 6)
        {
            ShowPopup("비밀번호는 6자리 이상이어야 합니다");
            return false;
        }

        // 비밀번호 확인
        if (passInput.text != passConfirmInput.text)
        {
            ShowPopup("비밀번호가 일치하지 않습니다");
            return false;
        }

        return true;
    }

    #endregion

    #region UI 관리

    private void ResetSignUpUI()
    {
        isSigningUp = false;
        
        if (signUpButton != null)
        {
            signUpButton.interactable = isEmailVerified; // 이메일 체크 상태에 따라
            var signUpText = signUpButton.GetComponentInChildren<TMP_Text>();
            if (signUpText != null)
                signUpText.text = "회원가입";
        }
    }

    private void Cancel()
    {
        ResetInputs();
        
        if (loginPanel != null)
        {
            loginPanel.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    private void ResetInputs()
    {
        if (emailInput != null) emailInput.text = "";
        if (passInput != null) passInput.text = "";
        if (passConfirmInput != null) passConfirmInput.text = "";
        ResetUI();
    }

    private IEnumerator WaitForSignOutComplete()
    {
        // 로그아웃 완료까지 기다리기 (최대 3초)
        float waitTime = 0f;
        const float maxWaitTime = 3f;
        
        while (FirebaseAuth.DefaultInstance.CurrentUser != null && waitTime < maxWaitTime)
        {
            yield return new WaitForSeconds(0.1f);
            waitTime += 0.1f;
        }
        
        // 추가 안전 대기
        yield return new WaitForSeconds(0.5f);
        
        // UI 상태 리셋
        ResetSignUpUI();
        
        // 로그인 화면으로 이동
        if (loginPanel != null)
        {
            gameObject.SetActive(false);
            loginPanel.SetActive(true);
            
            Debug.Log("회원가입 완료 - 로그인 화면으로 이동");
        }
    }

    #endregion

    #region 팝업 관련 메서드

    private void ShowPopup(string message)
    {
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowPopup(message);
        }
        else
        {
            Debug.LogWarning($"PopupManager를 찾을 수 없습니다: {message}");
        }
    }

    private void ShowCelebrationPopup()
    {
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowPopup("회원가입 성공!");
        }
        else
        {
            Debug.Log("회원가입 성공!");
        }
    }

    #endregion
}
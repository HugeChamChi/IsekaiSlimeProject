using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    [Header("Login UI Input")] [SerializeField]
    private GameObject loginPanel;

    [SerializeField] TMP_InputField loginIDInput;
    [SerializeField] TMP_InputField loginPasswordInput;

    [Header("Register UI INPUT")] [SerializeField]
    private GameObject registerPanel;

    [SerializeField] TMP_InputField registerIDInput;
    [SerializeField] TMP_InputField registerPasswordInput;

    private void Start()
    {
        if (registerPanel != null)
        {
            registerPanel.SetActive(false);
        }
    }

    public void LoginClick()
    {
        if (string.IsNullOrWhiteSpace(loginIDInput.text) || string.IsNullOrWhiteSpace(loginPasswordInput.text))
        {
            Debug.LogError("아이디와 비밀번호를 입력해주세요");
            return;
        }
        SceneManager.LoadScene("LobbyScene");
        
    }

    public void RegisterClick()
    {
        if (string.IsNullOrWhiteSpace(registerIDInput.text) || string.IsNullOrWhiteSpace(registerPasswordInput.text))
        {
            Debug.LogError("회원 가입을 위해서 아이디와 비밀번호를 입력해주세요");
            return;
        }
        
    }

    public void OpenRegisterPanel()
    {
        if (loginPanel != null)
        {
            loginPanel.SetActive(false);
        }
        
        if (registerPanel != null)
        {
            registerPanel.SetActive(true);

            if (registerIDInput != null)
            {
                registerIDInput.text = "";
                registerIDInput.Select();
                registerIDInput.ActivateInputField();
            }

            if (registerPasswordInput != null) registerPasswordInput.text = "";
        }
    }

    public void CloseRegisterPanel()
    {
        if (registerPanel != null)
        {
            registerPanel.SetActive(false);
        }

        if (loginPanel != null)
        {
            loginPanel.SetActive(true);

            if (loginIDInput != null)
            {
                loginIDInput.Select();
                loginIDInput.ActivateInputField();
            }
        }
    }
}
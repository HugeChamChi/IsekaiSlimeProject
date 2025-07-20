using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
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
        HideConfirmationPanel();
        
        if (statusText != null)
        {
            statusText.text = message;
        }
        
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
        
        ShowConfirmationPanel();
        gameObject.SetActive(true);
    }
    
    private void ShowConfirmationPanel()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
        }
        
        if (statusText != null && statusText.transform.parent != null)
        {
            statusText.transform.parent.gameObject.SetActive(false);
        }
    }
    
    private void HideConfirmationPanel()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
        
        if (statusText != null && statusText.transform.parent != null)
        {
            statusText.transform.parent.gameObject.SetActive(true);
        }
    }

    public void ClosePopup()
    {
        onYesCallback = null;
        onNoCallback = null;
        
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
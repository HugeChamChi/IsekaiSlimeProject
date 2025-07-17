using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    /// <summary>
    /// 간단한 팝업
    /// PopupManager.Instance.ShowPopup("메시지 내용");
    /// 자동으로 닫히는 팝업
    /// PopupManager.Instance.ShowPopupWithAutoClose("3초 후 닫힘", 3f);
    /// 수동으로 닫기
    /// PopupManager.Instance.ClosePopup(); 
    /// </summary>
    
    [Header("UI Elements")]
    [SerializeField] TMP_Text statusText;
    [SerializeField] Button closeButton;
    [SerializeField] Canvas canvas;
    
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

        // 버튼 리스너 설정
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePopup);
        }
        
        gameObject.SetActive(false);
    }

    // 프리팹에서 자동으로 PopupManager 생성
    private static void CreatePopupManager()
    {
        // Resources 폴더에서 프리팹 로드
        GameObject popupPrefab = Resources.Load<GameObject>("PopupManager");
        
        if (popupPrefab != null)
        {
            GameObject popupGO = Instantiate(popupPrefab);
            popupGO.name = "PopupManager"; 
        }
    }
    
    // 팝업 보여주기
    public void ShowPopup(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        
        gameObject.SetActive(true);
    }

    // 팝업 닫기
    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }

    // 자동으로 닫히는 팝업
    public void ShowPopupWithAutoClose(string message, float duration = 3f)
    {
        ShowPopup(message);
        
        // 기존 Invoke 취소 후 새로 설정
        CancelInvoke(nameof(ClosePopup));
        Invoke(nameof(ClosePopup), duration);
    }
    
    
}
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
            // 인스턴스가 없으면 자동으로 생성
            if (instance == null)
            {
                CreatePopupManager();
            }
            return instance;
        } 
    }
    
    private void Awake()
    {
        // 싱글톤 설정
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 유지
            
            // Canvas 설정 
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

        // 시작 시 숨김
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
            popupGO.name = "PopupManager"; // 이름 정리
        }
        else
        {
            Debug.LogError("PopupManager 프리팹을 Resources 폴더에서 찾을 수 없습니다!");
            
            // 프리팹이 없으면 빈 GameObject로 생성
            CreateEmptyPopupManager();
        }
    }

    // 프리팹이 없을 때 임시로 생성
    private static void CreateEmptyPopupManager()
    {
        GameObject popupGO = new GameObject("PopupManager");
        Canvas canvas = popupGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        
        popupGO.AddComponent<GraphicRaycaster>();
        PopupManager popupManager = popupGO.AddComponent<PopupManager>();
        
        Debug.LogWarning("PopupManager가 프리팹 없이 생성되었습니다. UI 요소를 수동으로 설정해주세요.");
    }

    // 팝업 보여주기
    public void ShowPopup(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        
        gameObject.SetActive(true);
        Debug.Log($"[팝업 표시] {message}");
    }

    // 팝업 닫기
    public void ClosePopup()
    {
        gameObject.SetActive(false);
        Debug.Log("[팝업 닫힌함]");
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
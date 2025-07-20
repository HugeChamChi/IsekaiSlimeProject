using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : DesignPattern.Singleton<UI_Manager>
{
    private Canvas mainCanvas;
    public Ultimate_Panel ultimate;
    public CardPanel cardPanel;
    public CharacterInfoPanel characterInfoPanel;
    public WarningMessagePanel warningMessagePanel;

    private void Start()
    {
        CreateMainCanvas();
        InitInGamePanel();
    }

    public void InitInGamePanel()
    {
        ultimate = Instantiate(Resources.Load<Ultimate_Panel>("UI/Ultimate_Panel"), mainCanvas.transform);
        cardPanel = Instantiate(Resources.Load<CardPanel>("UI/CardPanel"), mainCanvas.transform);
        characterInfoPanel = Instantiate(Resources.Load<CharacterInfoPanel>("UI/CharacterInfoPanel"), mainCanvas.transform);
        warningMessagePanel = Instantiate(Resources.Load<WarningMessagePanel>("UI/WarningMessagePanel"), mainCanvas.transform);
    }

    public void DestroyInGameUI()
    {
        Destroy(ultimate.gameObject);               ultimate = null;
        Destroy(cardPanel.gameObject);              cardPanel = null;
        Destroy(characterInfoPanel.gameObject);     characterInfoPanel = null;
        Destroy(warningMessagePanel.gameObject);    warningMessagePanel = null;
    }

    /// <summary>
    /// 메인 켄버스 동적생성 함수
    /// </summary>   
    private void CreateMainCanvas()
    {
        mainCanvas = new GameObject("MainCanvas").AddComponent<Canvas>();
        mainCanvas.pixelPerfect = true;
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler         = mainCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode          = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution  = new Vector2(1920, 1080);

        mainCanvas.AddComponent<RectTransform>();
        mainCanvas.AddComponent<GraphicRaycaster>();

        mainCanvas.transform.SetParent(transform, false);
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : DesignPattern.Singleton<UI_Manager>
{
    private Canvas mainCanvas;
    public Ultimate_Panel ultimate;

    private void Start()
    {
        CreateMainCanvas();

        ultimate = Instantiate(Resources.Load<Ultimate_Panel>("UI/Ultimate_Panel"), mainCanvas.transform);
    }

    private void CreateMainCanvas()
    {
        mainCanvas = new GameObject().AddComponent<Canvas>();
        mainCanvas.AddComponent<RectTransform>();
        CanvasScaler scaler = mainCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        mainCanvas.AddComponent<GraphicRaycaster>();
        mainCanvas.transform.SetParent(transform, false);
    }
}

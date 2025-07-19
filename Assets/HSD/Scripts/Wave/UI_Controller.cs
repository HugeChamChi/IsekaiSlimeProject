using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEditor.iOS;

public class UI_Controller
{
    private UnityEngine.Transform root;
    private Canvas waveCanvas;
    public BossAppearsPanel BossAppearsPanel;
    public InGameUIPanel wavePanel;

    public void Init(UnityEngine.Transform _root)
    {        
        root = _root;
        CreateWaveCanvas();
        BossAppearsPanel = Object.Instantiate(Resources.Load<BossAppearsPanel>("UI/BossAppearsPanel"), waveCanvas.transform);        
        wavePanel = Object.Instantiate(Resources.Load<InGameUIPanel>("UI/InGameUIPanel"));
        wavePanel.Init(root.GetComponent<WaveController>());
    }

    private void CreateWaveCanvas()
    {
        waveCanvas = new GameObject("MainCanvas").AddComponent<Canvas>();
        waveCanvas.pixelPerfect = true;
        waveCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = waveCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        waveCanvas.AddComponent<RectTransform>();
        waveCanvas.AddComponent<GraphicRaycaster>();

        waveCanvas.transform.SetParent(root, false);
    }
}

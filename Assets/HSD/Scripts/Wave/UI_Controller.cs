using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEditor.iOS;

public class UI_Controller
{
    private WaveController waveController;
    private Canvas waveCanvas;
    public BossAppearsPanel BossAppearsPanel;
    public BossClearPanel BossClearPanel;
    public InGameUIPanel wavePanel;

    public void Init(WaveController _waveController)
    {
        waveController = _waveController;
        CreateWaveCanvas();
        wavePanel = Object.Instantiate(Resources.Load<InGameUIPanel>("UI/InGameUIPanel"));
        wavePanel.Init(_waveController);
        wavePanel.transform.SetParent(waveCanvas.transform, false);

        BossAppearsPanel = Object.Instantiate(Resources.Load<BossAppearsPanel>("UI/BossAppearsPanel"), waveCanvas.transform);
        BossAppearsPanel.transform.SetParent(waveCanvas.transform, false);

        BossClearPanel = Object.Instantiate(Resources.Load<BossClearPanel>("UI/BossClearPanel"), waveCanvas.transform);        
        BossClearPanel.transform.SetParent(waveCanvas.transform, false);
    }

    private void CreateWaveCanvas()
    {
        waveCanvas = new GameObject("MainCanvas").AddComponent<Canvas>();
        waveCanvas.pixelPerfect = true;
        waveCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = waveCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        waveCanvas.AddComponent<GraphicRaycaster>();

        waveCanvas.transform.SetParent(waveController.transform, false);
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InGameUIPanel : MonoBehaviour
{
    private WaveController waveController;

    [SerializeField] WaveView waveView;
    public SettingPanel settingPanel;

    public void Init(WaveController _waveController)
    {
        waveController = _waveController;
        Subscribe();
        settingPanel = Instantiate(Resources.Load<SettingPanel>("UI/SettingPanel"), transform);
    }    

    private void OnDisable()
    {
        UnSubscribe();
    }

    private void Subscribe()
    {
        waveController.Info.CurWaveIdx.AddEvent(waveView.UpdateWaveText);
        waveController.Info.MonsterCount.AddEvent(waveView.UpdateMonsterCountText);
        waveController.Info.WaveTimer.AddEvent(waveView.UpdateTimerText);
    }

    private void UnSubscribe()
    {
        waveController.Info.CurWaveIdx.RemoveEvent(waveView.UpdateWaveText);
        waveController.Info.MonsterCount.RemoveEvent(waveView.UpdateMonsterCountText);
        waveController.Info.WaveTimer.RemoveEvent(waveView.UpdateTimerText);
    }

    public void OpenSetiingPanel()
    {
        settingPanel.gameObject.SetActive(true);
    }
}
